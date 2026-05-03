using System.Text.Json.Serialization;

namespace Cars.DataAccess.Entities.Resources;

/// <summary>
/// Payload model for partial updates to a car
/// </summary>
public class CarUpdatePayload(
    string? make = null,
    string? model = null,
    int? year = null,
    string? imageUrl = null)
{
    [JsonPropertyName("make")]
    public string? Make { get; set; } = make;

    [JsonPropertyName("model")]
    public string? Model { get; set; } = model;

    [JsonPropertyName("year")]
    public int? Year { get; set; } = year;

    [JsonPropertyName("imageUrl")]
    public string? ImageUrl { get; set; } = imageUrl;

    public bool HasUpdates() => Make != null || Model != null || Year != null || ImageUrl != null;

    public override string ToString()
    {
        var props = new List<string>();
        if (Make != null) props.Add($"Make={Make}");
        if (Model != null) props.Add($"Model={Model}");
        if (Year != null) props.Add($"Year={Year}");
        if (ImageUrl != null) props.Add($"ImageUrl={ImageUrl}");
        
        return $"CarUpdate with changes to: {string.Join(", ", props)}";
    }
}
