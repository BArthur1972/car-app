using System.Text.Json.Serialization;

namespace Cars.DataAccess.Entities;

public class User(
    string username,
    string email,
    string passwordHash)
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [JsonPropertyName("username")]
    public string Username { get; set; } = username;

    [JsonPropertyName("email")]
    public string Email { get; set; } = email;

    [JsonPropertyName("passwordHash")]
    public string PasswordHash { get; set; } = passwordHash;

    public override string ToString()
    {
        return $"User with id: {Id}, username: {Username}, email: {Email}.";
    }
}
