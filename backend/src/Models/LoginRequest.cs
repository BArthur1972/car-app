using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace Cars.Models;

[method: JsonConstructor, SetsRequiredMembers]
public class LoginRequest(string email, string password)
{
    [Required, EmailAddress]
    public required string Email { get; set; } = email;

    [Required]
    public required string Password { get; set; } = password;
}
