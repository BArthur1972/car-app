using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Cars.DataAccess.Entities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Cars.ApiCommon.Auth;

public class JwtTokenService : IJwtTokenService
{
    private readonly JwtOptions jwtOptions;
    private readonly SigningCredentials signingCredentials;

    public JwtTokenService(IOptions<JwtOptions> jwtConfig)
    {
        jwtOptions = jwtConfig.Value;

        if (!string.IsNullOrEmpty(jwtOptions.CertificateBase64))
        {
            // Certificate-based signing (RS256)
            var certBytes = Convert.FromBase64String(jwtOptions.CertificateBase64);
            var certificate = X509CertificateLoader.LoadPkcs12(certBytes, jwtOptions.CertificatePassword);
            var signingKey = new RsaSecurityKey(certificate.GetRSAPrivateKey());
            signingCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.RsaSha256);
        }
        else if (!string.IsNullOrEmpty(jwtOptions.Secret))
        {
            // Symmetric secret (HS256 - development only)
            var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Secret));
            signingCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);
        }
        else
        {
            throw new InvalidOperationException(
                "JWT signing key not configured. Provide either Jwt:CertificateBase64 or Jwt:Secret");
        }
    }

    public string GenerateToken(User user)
    {
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };

        var token = new JwtSecurityToken(
            issuer: jwtOptions.Issuer,
            audience: jwtOptions.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(jwtOptions.ExpiryMinutes),
            signingCredentials: signingCredentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
