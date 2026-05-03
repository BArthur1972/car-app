using System.Text.Json;
using System.Text.Json.Serialization;

namespace Cars.ApiCommon.Errors;

public class InnerError(string code)
{
    [JsonPropertyName("code")]
    public string Code { get; set; } = code;

    [JsonPropertyName("innerError")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public InnerError? NestedInnerError { get; set; }

    // Additional properties can be added using an extension property
    [JsonExtensionData]
    public Dictionary<string, JsonElement> AdditionalProperties { get; set; } = [];
}
