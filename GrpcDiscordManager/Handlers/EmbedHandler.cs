using System.Threading.Tasks;
using Discord;

namespace GrpcDiscordManager.Handlers;
static class EmbedHandler
{
    public static async Task<Embed> CreateBasicEmbed(string description, Color color) => await Task.Run(() => new EmbedBuilder() { Description = description, Color = color }.Build());
    public static async Task<Embed> CreateErrorEmbed(string error) => await Task.Run(() => new EmbedBuilder() { Description = error, Color = Color.Red }.Build());
    public static async Task<Embed> CreateFatalErrorEmbed(string error) => await Task.Run(() =>
    {
        if (error.Length > 2048)
        {
            error = error[..2048];
        }
        return new EmbedBuilder() { Title = "Непредвиденная ошибка", Description = error, Color = Color.Red }.Build();
    });
}
