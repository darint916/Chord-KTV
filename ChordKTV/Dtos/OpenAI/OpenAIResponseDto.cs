using System.Text.Json.Serialization;

namespace ChordKTV.Dtos.OpenAI;

public class OpenAIResponseDto
{
    public string Id { get; set; } = string.Empty;
    
    [JsonPropertyName("object")]
    public string ObjectType { get; set; } = string.Empty;
    
    public long Created { get; set; }
    public string Model { get; set; } = string.Empty;
    public List<OpenAIChoiceDto> Choices { get; set; } = new();
    public OpenAIUsageDto Usage { get; set; } = new();
}

public class OpenAIChoiceDto
{
    public int Index { get; set; }
    public OpenAIMessageDto Message { get; set; } = new();
    
    [JsonPropertyName("finish_reason")]
    public string FinishReason { get; set; } = string.Empty;
}

public class OpenAIMessageDto
{
    public string Role { get; set; } = string.Empty;
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
