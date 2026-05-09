using System.ComponentModel.DataAnnotations;

namespace Cars.ApiCommon.Auth;

public class JwtOptions
{
    public const string SectionKey = "Jwt";

    [Required, MinLength(32)]
    public required string Secret { get; set; }

    public int ExpiryMinutes { get; set; } = 60;
}
