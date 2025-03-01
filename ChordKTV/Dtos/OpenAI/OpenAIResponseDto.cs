using System.Text.Json.Serialization;

namespace ChordKTV.Dtos.OpenAI;

public class OpenAIResponseDto
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("object")]
    public string Object { get; set; } = string.Empty;

    [JsonPropertyName("created")]
    public long Created { get; set; }

    [JsonPropertyName("model")]
    public string Model { get; set; } = string.Empty;

    [JsonPropertyName("choices")]
    public List<OpenAIChoiceDto> Choices { get; set; } = new();

    [JsonPropertyName("usage")]
    public OpenAIUsageDto Usage { get; set; } = new();
}

public class OpenAIChoiceDto
{
    [JsonPropertyName("index")]
    public int Index { get; set; }

    [JsonPropertyName("message")]
    public OpenAIMessageDto Message { get; set; } = new();

    [JsonPropertyName("finish_reason")]
    public string FinishReason { get; set; } = string.Empty;
}

public class OpenAIMessageDto
{
    [JsonPropertyName("role")]
    public string Role { get; set; } = string.Empty;

    [JsonPropertyName("content")]
    public string Content { get; set; } = string.Empty;
}

public class OpenAIUsageDto
{
    [JsonPropertyName("prompt_tokens")]
    public int PromptTokens { get; set; }

    [JsonPropertyName("completion_tokens")]
    public int CompletionTokens { get; set; }

    [JsonPropertyName("total_tokens")]
    public int TotalTokens { get; set; }
} 