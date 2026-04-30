using System.Text.Json.Serialization;

namespace Cars.DataAccess.Entities
{
    public class Car(
        string make,
        string model,
        int year,
        string? imageUrl = null)
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = Guid.NewGuid().ToString();

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
}
