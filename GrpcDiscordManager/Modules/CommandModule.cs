using System;
using System.IO;
using System.Net;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Collections.Generic;
using Grpc.Net.Client;
using Discord;
using Discord.WebSocket;
using Discord.Interactions;
using GrpcDiscordManager.Handlers;

namespace GrpcDiscordManager.Modules;

public class CommandModule : InteractionModuleBase<SocketInteractionContext>
{
    private readonly Dictionary<ulong, string> _playerIdSocketPathPairs;

    public CommandModule(Dictionary<ulong, string> playerIdSocketPathPairs)
    {
        _playerIdSocketPathPairs = playerIdSocketPathPairs; 
    }

    [SlashCommand("играть", "Воспроизвести или добавить в очередь трек с YouTube.")]
    public async Task Play([Summary("трек", "Укажите название или ссылку трека на YouTube.")][Autocomplete(typeof(SuggestionsHandler))] string search)
    {
        if (((SocketGuildUser)Context.User).VoiceChannel == null)
        {
            await Context.Interaction.RespondAsync(string.Empty, embed: await EmbedHandler.CreateErrorEmbed("Вам требуется **присоединиться** к голосовому каналу"));
            return;
        }

        var playerId = await GetPlayer((SocketGuildUser)Context.User, Context.Guild);
        if (playerId == 0)
        {
            await Context.Interaction.RespondAsync(string.Empty, embed: await EmbedHandler.CreateErrorEmbed("Все музыкальные боты **заняты**."));
        }

        var client = CreateGrpcClient(_playerIdSocketPathPairs[playerId]);
        var response = await client.PlayCommandAsync(new PlayRequest { GuildId = Context.Guild.Id, VoiceChannelId = ((SocketGuildUser)Context.User).VoiceChannel.Id, Search = search });
        if (!string.IsNullOrEmpty(response.Error))
        {
            await Context.Interaction.RespondAsync(string.Empty, embed: await EmbedHandler.CreateErrorEmbed(response.Error));
        }
        else
        {
            await Context.Interaction.RespondAsync(string.Empty, embed: await EmbedHandler.CreateBasicEmbed(response.Message, Color.Blue));
        }
    }

    [SlashCommand("пропустить", "Пропустить текущий трек и воспроизвести следующий в очереди.")]
    public async Task Skip()
    {
        if (((SocketGuildUser)Context.User).VoiceChannel == null)
        {
            await Context.Interaction.RespondAsync(string.Empty, embed: await EmbedHandler.CreateErrorEmbed("Вам требуется **присоединиться** к голосовому каналу"));
            return;
        }

        var playerId = await GetPlayer((SocketGuildUser)Context.User, Context.Guild);
        if (playerId == 0)
        {
            await Context.Interaction.RespondAsync(string.Empty, embed: await EmbedHandler.CreateErrorEmbed("Все музыкальные боты **заняты**."));
        }

        var client = CreateGrpcClient(_playerIdSocketPathPairs[playerId]);
        var response = await client.SkipCommandAsync(new CommandRequest { GuildId = Context.Guild.Id });
        if (!string.IsNullOrEmpty(response.Error))
        {
            await Context.Interaction.RespondAsync(string.Empty, embed: await EmbedHandler.CreateErrorEmbed(response.Error));
        }
        else
        {
            await Context.Interaction.RespondAsync(string.Empty, embed: await EmbedHandler.CreateBasicEmbed(response.Message, Color.Green));
        }
    }

    [SlashCommand("стоп", "Остановить воспроизведение и покинуть голосовой канал.")]
    public async Task Stop()
    {
        if (((SocketGuildUser)Context.User).VoiceChannel == null)
        {
            await Context.Interaction.RespondAsync(string.Empty, embed: await EmbedHandler.CreateErrorEmbed("Вам требуется **присоединиться** к голосовому каналу"));
            return;
        }

        var playerId = await GetPlayer((SocketGuildUser)Context.User, Context.Guild);
        if (playerId == 0)
        {
            await Context.Interaction.RespondAsync(string.Empty, embed: await EmbedHandler.CreateErrorEmbed("Все музыкальные боты **заняты**."));
        }

        var client = CreateGrpcClient(_playerIdSocketPathPairs[playerId]);
        var response = await client.StopCommandAsync(new CommandRequest { GuildId = Context.Guild.Id });
        if (!string.IsNullOrEmpty(response.Error))
        {
            await Context.Interaction.RespondAsync(string.Empty, embed: await EmbedHandler.CreateErrorEmbed(response.Error));
        }
        else
        {
            await Context.Interaction.RespondAsync(string.Empty, embed: await EmbedHandler.CreateBasicEmbed(response.Message, Color.Green));
        }
    }

    [SlashCommand("очередь", "Отобразить названия треков в воспроизведении.")]
    public async Task Queue()
    {
        if (((SocketGuildUser)Context.User).VoiceChannel == null)
        {
            await Context.Interaction.RespondAsync(string.Empty, embed: await EmbedHandler.CreateErrorEmbed("Вам требуется **присоединиться** к голосовому каналу"));
            return;
        }

        var playerId = await GetPlayer((SocketGuildUser)Context.User, Context.Guild);
        if (playerId == 0)
        {
            await Context.Interaction.RespondAsync(string.Empty, embed: await EmbedHandler.CreateErrorEmbed("Все музыкальные боты **заняты**."));
        }

        var client = CreateGrpcClient(_playerIdSocketPathPairs[playerId]);
        var response = await client.QueueCommandAsync(new CommandRequest { GuildId = Context.Guild.Id });
        if (!string.IsNullOrEmpty(response.Error))
        {
            await Context.Interaction.RespondAsync(string.Empty, embed: await EmbedHandler.CreateErrorEmbed(response.Error));
        }
        else
        {
            await Context.Interaction.RespondAsync(string.Empty, embed: await EmbedHandler.CreateBasicEmbed(response.Message, Color.Green));
        }
    }

    [SlashCommand("пауза", "Приостановить воспроизведение текущего трека.")]
    public async Task Pause()
    {
        if (((SocketGuildUser)Context.User).VoiceChannel == null)
        {
            await Context.Interaction.RespondAsync(string.Empty, embed: await EmbedHandler.CreateErrorEmbed("Вам требуется **присоединиться** к голосовому каналу"));
            return;
        }

        var playerId = await GetPlayer((SocketGuildUser)Context.User, Context.Guild);
        if (playerId == 0)
        {
            await Context.Interaction.RespondAsync(string.Empty, embed: await EmbedHandler.CreateErrorEmbed("Все музыкальные боты **заняты**."));
        }

        var client = CreateGrpcClient(_playerIdSocketPathPairs[playerId]);
        var response = await client.PauseCommandAsync(new CommandRequest { GuildId = Context.Guild.Id });
        if (!string.IsNullOrEmpty(response.Error))
        {
            await Context.Interaction.RespondAsync(string.Empty, embed: await EmbedHandler.CreateErrorEmbed(response.Error));
        }
        else
        {
            await Context.Interaction.RespondAsync(string.Empty, embed: await EmbedHandler.CreateBasicEmbed(response.Message, Color.Green));
        }
    }

    [SlashCommand("возобновить", "Возобновить воспроизведение текущего трека.")]
    public async Task Resume()
    {
        if (((SocketGuildUser)Context.User).VoiceChannel == null)
        {
            await Context.Interaction.RespondAsync(string.Empty, embed: await EmbedHandler.CreateErrorEmbed("Вам требуется **присоединиться** к голосовому каналу"));
            return;
        }

        var playerId = await GetPlayer((SocketGuildUser)Context.User, Context.Guild);
        if (playerId == 0)
        {
            await Context.Interaction.RespondAsync(string.Empty, embed: await EmbedHandler.CreateErrorEmbed("Все музыкальные боты **заняты**."));
        }

        var client = CreateGrpcClient(_playerIdSocketPathPairs[playerId]);
        var response = await client.ResumeCommandAsync(new CommandRequest { GuildId = Context.Guild.Id });
        if (!string.IsNullOrEmpty(response.Error))
        {
            await Context.Interaction.RespondAsync(string.Empty, embed: await EmbedHandler.CreateErrorEmbed(response.Error));
        }
        else
        {
            await Context.Interaction.RespondAsync(string.Empty, embed: await EmbedHandler.CreateBasicEmbed(response.Message, Color.Green));
        }
    }

    [SlashCommand("промотать", "Промотка текущего трека к указанному временному интервалу.")]
    public async Task Seek([Summary("минута", "Укажите позицию промотки.")] int minutes, [Summary("секунда", "Укажите позицию промотки.")] int seconds)
    {
        if (((SocketGuildUser)Context.User).VoiceChannel == null)
        {
            await Context.Interaction.RespondAsync(string.Empty, embed: await EmbedHandler.CreateErrorEmbed("Вам требуется **присоединиться** к голосовому каналу"));
            return;
        }

        var playerId = await GetPlayer((SocketGuildUser)Context.User, Context.Guild);
        if (playerId == 0)
        {
            await Context.Interaction.RespondAsync(string.Empty, embed: await EmbedHandler.CreateErrorEmbed("Все музыкальные боты **заняты**."));
        }

        var client = CreateGrpcClient(_playerIdSocketPathPairs[playerId]);
        var response = await client.SeekCommandAsync(new SeekRequest { GuildId = Context.Guild.Id, Minutes = minutes, Seconds = seconds });
        if (!string.IsNullOrEmpty(response.Error))
        {
            await Context.Interaction.RespondAsync(string.Empty, embed: await EmbedHandler.CreateErrorEmbed(response.Error));
        }
        else
        {
            await Context.Interaction.RespondAsync(string.Empty, embed: await EmbedHandler.CreateBasicEmbed(response.Message, Color.Green));
        }
    }

    private async Task<ulong> GetPlayer(SocketGuildUser user, IGuild guild)
    {
        if (user.VoiceChannel.Users.FirstOrDefault(user => user.IsBot) != null)
        {
            foreach (var playerId in _playerIdSocketPathPairs.Keys)
            {
                var player = user.VoiceChannel.GetUser(playerId);
                if (player != null)
                {
                    return player.Id;
                }
            }
        }

        foreach (var playerId in _playerIdSocketPathPairs.Keys)
        {
            var player = await guild.GetUserAsync(playerId);
            if (player.VoiceChannel == null)
            {
                return player.Id;
            }
        }

        return 0;
    }

    private static Commander.CommanderClient CreateGrpcClient(string socketPath)
    {
        socketPath = Path.Combine(Path.GetTempPath(), socketPath);
        var channel = GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions
        {
            HttpHandler = UnixDomainSocketConnectionFactory.CreateHttpHandler(socketPath)
        });

        return new Commander.CommanderClient(channel);
    }

    private class UnixDomainSocketConnectionFactory
    {
        private readonly EndPoint _endPoint;

        private UnixDomainSocketConnectionFactory(EndPoint endPoint)
        {
            _endPoint = endPoint;
        }

        private async ValueTask<Stream> ConnectAsync(SocketsHttpConnectionContext _, CancellationToken cancellationToken = default)
        {
            var socket = new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.Unspecified);

            try
            {
                await socket.ConnectAsync(_endPoint, cancellationToken).ConfigureAwait(false);
                return new NetworkStream(socket, true);
            }
            catch (Exception ex)
            {
                socket.Dispose();
                throw new HttpRequestException($"Error connecting to '{_endPoint}'.", ex);
            }
        }

        public static SocketsHttpHandler CreateHttpHandler(string socketPath)
        {
            var udsEndPoint = new UnixDomainSocketEndPoint(socketPath);
            var connectionFactory = new UnixDomainSocketConnectionFactory(udsEndPoint);
            var socketsHttpHandler = new SocketsHttpHandler
            {
                ConnectCallback = connectionFactory.ConnectAsync
            };

            return socketsHttpHandler;
        }
    }
}
