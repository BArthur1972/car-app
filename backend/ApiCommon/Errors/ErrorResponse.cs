using System.Text.Json.Serialization;

namespace Cars.ApiCommon.Errors;

public class ErrorResponse(ErrorDetail error)
{
    [JsonPropertyName("error")]
    public ErrorDetail Error { get; set; } = error;
}
