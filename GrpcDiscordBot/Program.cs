using Microsoft.AspNetCore.Server.Kestrel.Core;
using GrpcDiscordBot.Services;
using Discord.WebSocket;
using Victoria;

namespace GrpcDiscordBot;

class Program
{
    static void Main(string[] args)
    {
        string socketPath = Path.Combine(Path.GetTempPath(), args[0]);
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.ConfigureKestrel(options =>
        {
            if (File.Exists(socketPath))
            {
                File.Delete(socketPath);
            }
            options.ListenUnixSocket(socketPath, listenOptions =>
            {
                listenOptions.Protocols = HttpProtocols.Http2;
            });
        });

        builder.Services
        .AddSingleton<DiscordSocketClient>()
        .AddSingleton<LavaNode>()
        .AddSingleton<LavaConfig>()
        .AddSingleton<DiscordService>()
        .AddSingleton<CommandService>()
        .AddGrpc();
        builder.Services.AddGrpcClient<Commander.CommanderBase>();
        
        var app = builder.Build();
        Task.Run(() => app.Services.GetService<DiscordService>().InitializeAsync(args[1]));
        app.MapGrpcService<CommandService>();
        app.Run();
    }
}



