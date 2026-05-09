using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace Cars.Models;

[method: JsonConstructor, SetsRequiredMembers]
public class RegisterRequest(string username, string email, string password)
{
    [Required]
    public required string Username { get; set; } = username;

    [Required, EmailAddress]
    public required string Email { get; set; } = email;

    [Required, MinLength(8)]
    public required string Password { get; set; } = password;
}
