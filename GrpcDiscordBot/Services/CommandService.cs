using System.Text;
using Grpc.Core;
using Discord.WebSocket;
using Discord;
using Victoria;
using Victoria.Enums;
using Victoria.EventArgs;

namespace GrpcDiscordBot.Services;

public class CommandService : Commander.CommanderBase
{
    private readonly DiscordSocketClient _client;
    private readonly LavaNode _lavaNode;

    public CommandService(IServiceProvider serviceProvider)
    {
        _client = serviceProvider.GetRequiredService<DiscordSocketClient>();
        _lavaNode = serviceProvider.GetRequiredService<LavaNode>();
    }

    public override async Task<CommandResponse> PlayCommand(PlayRequest request, ServerCallContext context)
    {
        IGuild guild = _client.GetGuild(request.GuildId);

        var search = await _lavaNode.SearchYouTubeAsync(request.Search);

        if (search.Status == Victoria.Responses.Search.SearchStatus.NoMatches)
        {
            return new CommandResponse { Error = $"По запросу **{request.Search}** совпадений не найдено" };
        }

        await _lavaNode.JoinAsync(guild.GetVoiceChannelAsync(request.VoiceChannelId).Result);

        var player = _lavaNode.GetPlayer(guild);
        if (player.PlayerState == PlayerState.Playing || player.PlayerState == PlayerState.Paused)
        {
            var track = search.Tracks.First();
            player.Queue.Enqueue(track);
            return new CommandResponse { Message = $"В очередь добавлен следующий трек: **{track.Title}**" };
        }
        else
        {
            var track = search.Tracks.First();

            await player.PlayAsync(track);
            return new CommandResponse { Message = $"В данный момент воспроизводится: **{track.Title}**" };
        }
    }

    public override async Task<CommandResponse> SkipCommand(CommandRequest request, ServerCallContext context)
    {
        IGuild guild = _client.GetGuild(request.GuildId);

        if (!_lavaNode.HasPlayer(guild))
        {
            return new CommandResponse { Error = "Бот **не присоединён** к голосовому каналу" };
        }

        var player = _lavaNode.GetPlayer(guild);

        if (player.Queue.Count < 1)
        {
            await player.StopAsync();
            player.Queue.Clear();
            return new CommandResponse { Message = "Воспроизведение треков **завершено**" };
        }
        else
        {
            var currentTrack = player.Track;
            await player.SkipAsync();
            return new CommandResponse { Message = $"Трек - **{currentTrack.Title}** пропущен \n В данный момент воспроизводится: **{player.Track.Title}**" };
        }
    }

    public override async Task<CommandResponse> StopCommand(CommandRequest request, ServerCallContext context)
    {
        IGuild guild = _client.GetGuild(request.GuildId);

        if (!_lavaNode.HasPlayer(guild))
        {
            return new CommandResponse { Error = "Бот **не присоединён** к голосовому каналу" };
        }

        var player = _lavaNode.GetPlayer(guild);

        await player.StopAsync();
        player.Queue.Clear();

        return new CommandResponse { Message = "Воспроизведение треков **завершено**, очередь треков сброшена" };
    }

    public override async Task<CommandResponse> QueueCommand(CommandRequest request, ServerCallContext context)
    {
        IGuild guild = _client.GetGuild(request.GuildId);

        if (!_lavaNode.HasPlayer(guild))
        {
            return new CommandResponse { Error = "Бот **не присоединён** к голосовому каналу" };
        }

        var player = _lavaNode.GetPlayer(guild);

        var descriptionBuilder = new StringBuilder();

        if (player.Queue.Count < 1 && player.Track != null)
        {
            return new CommandResponse { Message = $"В данный момент воспроизводится: **{player.Track.Title}** \n\nКакие-либо другие треки **отсутствуют** в очереди" };
        }
        else
        {
            int trackNum = 2;
            foreach (LavaTrack track in player.Queue)
            {

                if (trackNum < 20)
                {
                    descriptionBuilder.Append($"{trackNum}. {track.Title}\n");
                    trackNum++;
                }
            }
            return await Task.Run(() => new CommandResponse { Message = $"В данный момент воспроизводится: \n**1. {player.Track.Title}** \n**{descriptionBuilder}**" });
        }
    }

    public override async Task<CommandResponse> PauseCommand(CommandRequest request, ServerCallContext context)
    {
        IGuild guild = _client.GetGuild(request.GuildId);

        if (!_lavaNode.HasPlayer(guild))
        {
            return new CommandResponse { Error = "Бот **не присоединён** к голосовому каналу" };
        }

        var player = _lavaNode.GetPlayer(guild);

        await player.PauseAsync();
        return new CommandResponse { Message = $"Воспроизведение трека **{player.Track.Title}** приостановлено" };
    }

    public override async Task<CommandResponse> ResumeCommand(CommandRequest request, ServerCallContext context)
    {
        IGuild guild = _client.GetGuild(request.GuildId);

        if (!_lavaNode.HasPlayer(guild))
        {
            return new CommandResponse { Error = "Бот **не присоединён** к голосовому каналу" };
        }

        var player = _lavaNode.GetPlayer(guild);

        await player.ResumeAsync();

        return new CommandResponse { Message = $"Воспроизведение трека - **{player.Track.Title}** возобновлено" };
    }

    public override async Task<CommandResponse> SeekCommand(SeekRequest request, ServerCallContext context)
    {
        IGuild guild = _client.GetGuild(request.GuildId);

        if (!_lavaNode.HasPlayer(guild))
        {
            return new CommandResponse { Error = "Бот **не присоединён** к голосовому каналу" };
        }

        var player = _lavaNode.GetPlayer(guild);

        if (request.Minutes >= 60 || request.Seconds >= 60)
        {
            return new CommandResponse { Error = "Тайм-код доступен в диапозоне `0:00` - `59:59`" };
        }
        else if (new TimeSpan(0, request.Minutes, request.Seconds) > player.Track.Duration)
        {
            if (player.Track.Duration.Seconds < 10)
            {
                return new CommandResponse { Error = $"Тайм-код текущего трека доступен лишь в диапозоне `0:00` - `{player.Track.Duration.Minutes}:0{player.Track.Duration.Seconds}`" };
            }
            return new CommandResponse { Error = $"Тайм-код текущего трека доступен лишь в диапозоне `0:00` - `{player.Track.Duration.Minutes}:{player.Track.Duration.Seconds}`" };
        }

        TimeSpan timeSpan = new(0, request.Minutes, request.Seconds);
        await player.SeekAsync(timeSpan);
        if (request.Seconds < 10)
        {
            return new CommandResponse { Message = $"Тайм-код установлен на `{request.Minutes}:0{request.Seconds}` в треке **{player.Track.Title}**" };
        }
        return new CommandResponse { Message = $"Тайм-код установлен на `{request.Minutes}:{request.Seconds}` в треке **{player.Track.Title}**" };
    }

    public async Task TrackEnded(TrackEndedEventArgs args)
    {
        if (args.Reason == TrackEndReason.Finished)
        {
            if (args.Player.Queue.TryDequeue(out LavaTrack lavaTrack))
            {
                await args.Player.PlayAsync(lavaTrack);
            }
            else
            {
                await _lavaNode.LeaveAsync(args.Player.VoiceChannel);
            }
        }
        else if (args.Reason == TrackEndReason.Stopped)
        {
            await _lavaNode.LeaveAsync(args.Player.VoiceChannel);
        }
    }

    public async Task UsersLeftVoiceChannel(SocketUser _, SocketVoiceState voiceStatePrevious, SocketVoiceState _1)
    {
        if (voiceStatePrevious.VoiceChannel?.Users.FirstOrDefault(u => u.Id == _client.CurrentUser.Id) != null)
        {
            if (!voiceStatePrevious.VoiceChannel.Users.Any(u => !u.IsBot))
            {
                var player = _lavaNode.GetPlayer(voiceStatePrevious.VoiceChannel.Guild);

                await player.StopAsync();
                player.Queue.Clear();
            }
        }
    }
}