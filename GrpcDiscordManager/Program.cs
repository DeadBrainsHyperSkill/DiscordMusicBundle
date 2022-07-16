using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using GrpcDiscordManager.Services;

namespace GrpcDiscordManager;

class Program
{
    private record BalancerConfiguration (string Token, string GuildId, Dictionary<ulong, string> PlayerIdSocketPathPairs);

    private static Task Main()
    {
        var jsonString = File.ReadAllText("appsettings.json");
        BalancerConfiguration config = JsonSerializer.Deserialize<BalancerConfiguration>(jsonString);
        return new DiscordService(Convert.ToUInt64(config.GuildId), config.PlayerIdSocketPathPairs).InitializeAsync(config.Token);
    }
}