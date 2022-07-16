using Discord;
using Discord.WebSocket;
using Victoria;

namespace GrpcDiscordBot.Services;

public class DiscordService
{
    private readonly DiscordSocketClient _client;
    private readonly LavaNode _lavaNode;
    private readonly CommandService _audioService;
    public DiscordService(IServiceProvider serviceProvider)
    {
        _client = serviceProvider.GetRequiredService<DiscordSocketClient>();
        _lavaNode = serviceProvider.GetRequiredService<LavaNode>();
        _audioService = serviceProvider.GetRequiredService<CommandService>();
    }
    public async Task InitializeAsync(string token)
    {
        _lavaNode.OnTrackEnded += _audioService.TrackEnded;
        _client.UserVoiceStateUpdated += _audioService.UsersLeftVoiceChannel;
        _client.Ready += async () => await _lavaNode.ConnectAsync();

        await _client.LoginAsync(TokenType.Bot, token);
        await _client.StartAsync();
    }
}
