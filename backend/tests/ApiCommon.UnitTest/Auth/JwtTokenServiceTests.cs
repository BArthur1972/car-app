using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Cars.ApiCommon.Auth;
using Cars.DataAccess.Entities;
using FluentAssertions;
using Microsoft.Extensions.Options;

namespace ApiCommon.UnitTest.Auth;

public class JwtTokenServiceTests
{
    [Fact]
    public void Constructor_ThrowsInvalidOperationException_WhenNeitherCertificateNorSecretProvided()
    {
        var options = Options.Create(new JwtOptions
        {
            Issuer = "test-issuer",
            Audience = "test-audience",
            ExpiryMinutes = 60
        });

        var act = () => new JwtTokenService(options);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*JWT signing key not configured*");
    }

    [Fact]
    public void Constructor_SucceedsWithSymmetricKey_WhenSecretProvided()
    {
        var options = Options.Create(new JwtOptions
        {
            Secret = "test-secret-32-characters-minimum!",
            Issuer = "test-issuer",
            Audience = "test-audience",
            ExpiryMinutes = 60
        });

        var act = () => new JwtTokenService(options);

        act.Should().NotThrow();
    }

    [Fact]
    public void Constructor_SucceedsWithCertificate_WhenCertificateBase64Provided()
    {
        using var certificate = GenerateTestCertificate();
        var certBase64 = Convert.ToBase64String(certificate.Export(X509ContentType.Pkcs12));

        var options = Options.Create(new JwtOptions
        {
            CertificateBase64 = certBase64,
            Issuer = "test-issuer",
            Audience = "test-audience",
            ExpiryMinutes = 60
        });

        var act = () => new JwtTokenService(options);

        act.Should().NotThrow();
    }

    [Fact]
    public void GenerateToken_ReturnsValidJwt_WithCorrectClaims()
    {
        var options = Options.Create(new JwtOptions
        {
            Secret = "test-secret-32-characters-minimum!",
            Issuer = "test-issuer",
            Audience = "test-audience",
            ExpiryMinutes = 60
        });

        var sut = new JwtTokenService(options);
        var user = new User("testuser", "test@example.com", "hash");

        var token = sut.GenerateToken(user);

        token.Should().NotBeNullOrEmpty();
        token.Split('.').Should().HaveCount(3); // JWT format: header.payload.signature

        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        jwtToken.Issuer.Should().Be("test-issuer");
        jwtToken.Audiences.Should().Contain("test-audience");
        jwtToken.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Sub && c.Value == user.Id);
        jwtToken.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Email && c.Value == user.Email);
        jwtToken.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Jti);
    }

    [Fact]
    public void GenerateToken_SetsExpirationTime_BasedOnExpiryMinutes()
    {
        var options = Options.Create(new JwtOptions
        {
            Secret = "test-secret-32-characters-minimum!",
            Issuer = "test-issuer",
            Audience = "test-audience",
            ExpiryMinutes = 30
        });

        var sut = new JwtTokenService(options);
        var user = new User("testuser", "test@example.com", "hash");
        var beforeGeneration = DateTime.UtcNow;

        var token = sut.GenerateToken(user);

        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        jwtToken.ValidTo.Should().BeCloseTo(beforeGeneration.AddMinutes(30), TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void GenerateToken_CreatesUniqueTokens_OnMultipleCalls()
    {
        var options = Options.Create(new JwtOptions
        {
            Secret = "test-secret-32-characters-minimum!",
            Issuer = "test-issuer",
            Audience = "test-audience",
            ExpiryMinutes = 60
        });

        var sut = new JwtTokenService(options);
        var user = new User("testuser", "test@example.com", "hash");

        var token1 = sut.GenerateToken(user);
        var token2 = sut.GenerateToken(user);

        token1.Should().NotBe(token2); // Different JTI should make them unique

        var handler = new JwtSecurityTokenHandler();
        var jwtToken1 = handler.ReadJwtToken(token1);
        var jwtToken2 = handler.ReadJwtToken(token2);

        var jti1 = jwtToken1.Claims.First(c => c.Type == JwtRegisteredClaimNames.Jti).Value;
        var jti2 = jwtToken2.Claims.First(c => c.Type == JwtRegisteredClaimNames.Jti).Value;

        jti1.Should().NotBe(jti2);
    }

    private static X509Certificate2 GenerateTestCertificate()
    {
        using var rsa = RSA.Create(2048);

        var certRequest = new CertificateRequest(
            "CN=test",
            rsa,
            HashAlgorithmName.SHA256,
            RSASignaturePadding.Pkcs1);

        return certRequest.CreateSelfSigned(
            DateTimeOffset.UtcNow.AddDays(-1),
            DateTimeOffset.UtcNow.AddDays(1));
    }
}
