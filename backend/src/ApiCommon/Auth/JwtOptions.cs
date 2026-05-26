using System.ComponentModel.DataAnnotations;

namespace Cars.ApiCommon.Auth;

public class JwtOptions
{
    public const string SectionKey = "Jwt";

    // Used for ad hoc development testing only
    [MinLength(32)]
    public string? Secret { get; set; }

    // Used for Docker Compose and Production
    public string? CertificateBase64 { get; set; }

    public string? CertificatePassword { get; set; }
    
    public required string Issuer { get; set; }
    
    public required string Audience { get; set; }

    public int ExpiryMinutes { get; set; }
}
