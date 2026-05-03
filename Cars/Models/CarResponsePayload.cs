using System.Text.Json.Serialization;

namespace Cars.Models;

public class CarResponsePayload(
    string id,
    string make,
    string model,
    int year,
    string? imageUrl)
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = id;

    [JsonPropertyName("make")]
    public string Make { get; set; } = make;

    [JsonPropertyName("model")]
    public string Model { get; set; } = model;

    [JsonPropertyName("year")]
    public int Year { get; set; } = year;

    [JsonPropertyName("imageUrl")]
    public string? ImageUrl { get; set; } = imageUrl;

    public override string ToString()
    {
        return $"Car with id: {Id}, is a {Year} {Make} {Model}.";
    }
}
