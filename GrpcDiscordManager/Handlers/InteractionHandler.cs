using System;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Discord.WebSocket;
using Discord.Interactions;

namespace GrpcDiscordManager.Handlers
{
    class InteractionHandler
    {
        private readonly DiscordSocketClient _client;
        private readonly InteractionService _commands;
        private readonly IServiceProvider _services;

        public InteractionHandler(IServiceProvider services)
        {
            _commands = services.GetRequiredService<InteractionService>();
            _client = services.GetRequiredService<DiscordSocketClient>();
            _services = services;
        }

        public async Task InitializeAsync()
        {
            _client.InteractionCreated += (command) => _commands.ExecuteCommandAsync(new SocketInteractionContext(_client, command), _services);
            _commands.SlashCommandExecuted += async (_, interactionContext, result) =>
            {
                if (!result.IsSuccess)
                {
                    await interactionContext.Interaction.RespondAsync(embed: await EmbedHandler.CreateFatalErrorEmbed(result.ErrorReason));
                }
            };

            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
        }
    }
}
