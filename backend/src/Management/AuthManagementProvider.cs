using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Cars.ApiCommon.Auth;
using Cars.ApiCommon.Exceptions;
using Cars.DataAccess;
using Cars.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using User = Cars.DataAccess.Entities.User;

namespace Cars.Management;

public class AuthManagementProvider(
    IUserDataProvider userDataProvider,
    IPasswordHasher<User> passwordHasher,
    IOptions<JwtOptions> jwtOptions,
    ILogger<AuthManagementProvider> logger) : IAuthManagementProvider
{
    private readonly JwtOptions jwtOptions = jwtOptions.Value;

    public async Task RegisterAsync(RegisterRequest request)
    {
        try
        {
            await userDataProvider.GetUserByEmailAsync(request.Email);
            throw new ConflictException(message: "Email is already registered");
        }
        catch (DataNotFoundException)
        {
            // Email is available — proceed with registration
        }

        var user = new User(request.Username, request.Email, string.Empty);
        user.PasswordHash = passwordHasher.HashPassword(user, request.Password);

        await userDataProvider.CreateUserAsync(user);
        logger.LogInformation("Registered new user: {UserId}", user.Id);
    }

    public async Task<AuthResponse> LoginAsync(string email, string password)
    {
        User user;
        try
        {
            user = await userDataProvider.GetUserByEmailAsync(email);
        }
        catch (DataNotFoundException)
        {
            throw new UnauthorizedException(message: "Invalid email or password");
        }

        var result = passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password);

        if (result == PasswordVerificationResult.Failed)
            throw new UnauthorizedException(message: "Invalid email or password");

        if (result == PasswordVerificationResult.SuccessRehashNeeded)
            await TryRehashPasswordAsync(user, password);

        var token = GenerateJwtToken(user);
        return new AuthResponse(token);
    }

    private async Task TryRehashPasswordAsync(User user, string password)
    {
        try
        {
            user.PasswordHash = passwordHasher.HashPassword(user, password);
            await userDataProvider.UpdateUserAsync(user);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to rehash password for user {UserId}. " +
            "Login will succeed but hash upgrade was skipped.", user.Id);
        }
    }

    private string GenerateJwtToken(User user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };

        var token = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(jwtOptions.ExpiryMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
