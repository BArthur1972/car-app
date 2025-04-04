using System.Text.Json.Serialization;

namespace Cars.DataAccess.Entities
{
    public class Car
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("make")]
        public string Make { get; set; }

        [JsonPropertyName("model")]
        public string Model { get; set; }

        [JsonPropertyName("year")]
        public int Year { get; set; }

        [JsonPropertyName("imageUrl")]
        public string? ImageUrl { get; set; }

        public Car(string make, string model, int year, string? imageUrl = null)
        {
            Id = Guid.NewGuid().ToString();
            Make = make;
            Model = model;
            Year = year;
            ImageUrl = imageUrl;
        }

        public override string ToString()
        {
            return $"Car with id: {Id}, is a {Year} {Make} {Model}.";
        }
    }
}
