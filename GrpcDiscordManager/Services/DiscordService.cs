using System.Threading.Tasks;
using System.Collections.Generic;
using GrpcDiscordManager.Handlers;
using Microsoft.Extensions.DependencyInjection;
using Discord;
using Discord.WebSocket;
using Discord.Interactions;

namespace GrpcDiscordManager.Services;

class DiscordService
{
    private readonly ServiceProvider _services;
    private readonly DiscordSocketClient _client;
    private readonly InteractionHandler _commandHandler;
    private readonly ulong _guildId;
    public DiscordService(ulong guildId, Dictionary<ulong, string> playerIdSocketPathPairs)
    {
        _guildId = guildId;
        _services = new ServiceCollection()
       .AddSingleton(new DiscordSocketClient(new DiscordSocketConfig() { AlwaysDownloadUsers = true, GatewayIntents = GatewayIntents.GuildMembers | GatewayIntents.Guilds | GatewayIntents.GuildVoiceStates }))
       .AddSingleton(playerIdSocketPathPairs)
       .AddSingleton<InteractionService>()
       .AddSingleton<InteractionHandler>()
       .BuildServiceProvider();

        _client = _services.GetRequiredService<DiscordSocketClient>();
        _commandHandler = _services.GetRequiredService<InteractionHandler>();

        _client.Ready += RegisterCommands;
    }

    private async Task RegisterCommands()
    {
        List<SlashCommandBuilder> guildCommands = new();

        guildCommands.Add(new SlashCommandBuilder()
            .WithName("играть")
            .WithDescription("Воспроизвести или добавить в очередь трек с YouTube.")
            .AddOption("трек", ApplicationCommandOptionType.String, "Укажите название или ссылку трека на YouTube.", true, isAutocomplete: true));

        guildCommands.Add(new SlashCommandBuilder()
            .WithName("пропустить")
            .WithDescription("Пропустить текущий трек и воспроизвести следующий в очереди."));

        guildCommands.Add(new SlashCommandBuilder()
            .WithName("стоп")
            .WithDescription("Остановить воспроизведение и покинуть голосовой канал."));

        guildCommands.Add(new SlashCommandBuilder()
            .WithName("очередь")
            .WithDescription("Отобразить названия треков в воспроизведении."));

        guildCommands.Add(new SlashCommandBuilder()
            .WithName("пауза")
            .WithDescription("Приостановить воспроизведение текущего трека."));

        guildCommands.Add(new SlashCommandBuilder()
            .WithName("возобновить")
            .WithDescription("Возобновить воспроизведение текущего трека."));

        guildCommands.Add(new SlashCommandBuilder()
            .WithName("промотать")
            .WithDescription("Промотка текущего трека к указанному временному интервалу.")
            .AddOption("минута", ApplicationCommandOptionType.Integer, "Укажите позицию промотки.", true)
            .AddOption("секунда", ApplicationCommandOptionType.Integer, "Укажите позицию промотки.", true));

        foreach (var guildCommand in guildCommands)
        {
            await _client.GetGuild(_guildId).CreateApplicationCommandAsync(guildCommand.Build());
        }
    }

    public async Task InitializeAsync(string token)
    {
        await _client.LoginAsync(TokenType.Bot, token);
        await _client.StartAsync();
        await _commandHandler.InitializeAsync();     
        await Task.Delay(-1);
    }

}