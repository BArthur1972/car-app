using Cars.ApiCommon.Auth;
using Cars.ApiCommon.Exceptions;
using Cars.DataAccess;
using Cars.Models;
using Microsoft.AspNetCore.Identity;
using User = Cars.DataAccess.Entities.User;

namespace Cars.Management;

public class AuthManagementProvider(
    IUserDataProvider userDataProvider,
    IPasswordHasher<User> passwordHasher,
    IJwtTokenService jwtTokenService,
    ILogger<AuthManagementProvider> logger) : IAuthManagementProvider
{

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
        logger.LogInformation("User registered: {Email}", request.Email);
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

        var token = jwtTokenService.GenerateToken(user);
        logger.LogInformation("User logged in: {Email}", email);
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
}
