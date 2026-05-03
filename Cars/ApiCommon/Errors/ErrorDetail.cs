using System.Text.Json.Serialization;

namespace Cars.ApiCommon.Errors;

public class ErrorDetail(
    string code,
    string message,
    string? target = null)
{
    [JsonPropertyName("code")]
    public string Code { get; set; } = code;

    [JsonPropertyName("message")]
    public string Message { get; set; } = message;

    [JsonPropertyName("target")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Target { get; set; } = target;

    [JsonPropertyName("details")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<ErrorDetail>? Details { get; set; }

    [JsonPropertyName("innerError")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public InnerError? InnerError { get; set; }
}
