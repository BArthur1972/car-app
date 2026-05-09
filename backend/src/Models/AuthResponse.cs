using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace Cars.Models;

[method: JsonConstructor, SetsRequiredMembers]
public class AuthResponse(string token)
{
    public required string Token { get; set; } = token;
}
