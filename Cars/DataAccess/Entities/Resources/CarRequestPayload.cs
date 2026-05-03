using System.Text.Json.Serialization;

namespace Cars.DataAccess.Entities.Resources;

[method: JsonConstructor]
public class CarRequestPayload(
    string make,
    string model,
    int year,
    string? imageUrl)
{
    public required string Make { get; set; } = make;

    public required string Model { get; set; } = model;

    public required int Year { get; set; } = year;

    public string? ImageUrl { get; set; } = imageUrl;

    public override string ToString()
    {
        return $"Car is a {Year} {Make} {Model}.";
    }
}
