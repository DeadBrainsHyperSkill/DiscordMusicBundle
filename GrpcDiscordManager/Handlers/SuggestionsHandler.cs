using System;
using System.Web;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Discord;
using Discord.Interactions;

namespace GrpcDiscordManager.Handlers;

public class SuggestionsHandler : AutocompleteHandler
{
    private static readonly HttpClient client = new();
    private static readonly Regex regex = new("\"[^\"]*\"");

    public override async Task<AutocompletionResult> GenerateSuggestionsAsync(IInteractionContext context, IAutocompleteInteraction autocompleteInteraction, IParameterInfo parameter, IServiceProvider services)
    {
        var value = autocompleteInteraction.Data.Current.Value.ToString();

        if (string.IsNullOrEmpty(value))
            return await Task.FromResult(AutocompletionResult.FromSuccess());

        List<AutocompleteResult> results = new();
        var response = Regex.Unescape(await client.GetStringAsync($"https://clients1.google.com/complete/search?client=youtube&gs_ri=youtube&ds=yt&q={value}"));
        var matches = regex.Matches(response);
        foreach (var match in matches)
        {
            var result = HttpUtility.HtmlDecode(match.ToString()).Trim('\"');
            if (result.Length != 1)
            {
                results.Add(new AutocompleteResult(result, result));
            }
        }
        return await Task.FromResult(AutocompletionResult.FromSuccess(results.Skip(1).SkipLast(3)));
    }
}
