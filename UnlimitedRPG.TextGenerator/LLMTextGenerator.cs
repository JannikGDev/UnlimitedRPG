using System.Net.Http.Json;
using System.Text.Json.Serialization;
using UnlimitedRPG.Core.Interfaces;

namespace UnlimitedRPG.TextGenerator;

public class LLMTextGenerator(HttpClient httpClient) : ITextGenerator
{
    private const string Url   = "http://localhost:1234/api/v1/chat";
    private const string Model = "google/gemma-3-12b";

    public async Task<string> GenerateAsync(string systemPrompt, string inputPrompt, CancellationToken ct = default)
    {
        var requestBody = new LmStudioRequest(Model, systemPrompt, inputPrompt);

        var response = await httpClient.PostAsJsonAsync(Url, requestBody, ct);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<LmStudioResponse>(ct);

        var content = result?.Output?.FirstOrDefault(o => o.Type == "message")?.Content;
        if (content is null)
            throw new InvalidOperationException("LLM response contained no message output.");

        return content;
    }
}

file record LmStudioRequest(
    [property: JsonPropertyName("model")]         string Model,
    [property: JsonPropertyName("system_prompt")] string SystemPrompt,
    [property: JsonPropertyName("input")]         string Input
);

file record LmStudioResponse(
    [property: JsonPropertyName("output")] List<OutputItem> Output
);

file record OutputItem(
    [property: JsonPropertyName("type")]    string Type,
    [property: JsonPropertyName("content")] string Content
);
