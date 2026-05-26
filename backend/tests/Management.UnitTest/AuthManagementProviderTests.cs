using Cars.ApiCommon.Auth;
using Cars.ApiCommon.Exceptions;
using Cars.DataAccess;
using Cars.DataAccess.Entities;
using Cars.Management;
using Cars.Models;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Moq;

namespace Management.UnitTest;

public class AuthManagementProviderTests
{
    private readonly Mock<IUserDataProvider> userDataProviderMock = new();
    private readonly Mock<IPasswordHasher<User>> passwordHasherMock = new();
    private readonly Mock<IJwtTokenService> jwtTokenServiceMock = new();
    private readonly Mock<ILogger<AuthManagementProvider>> loggerMock = new();
    private readonly AuthManagementProvider sut;

    public AuthManagementProviderTests()
    {
        jwtTokenServiceMock
            .Setup(x => x.GenerateToken(It.IsAny<User>()))
            .Returns("mock.jwt.token");

        sut = new AuthManagementProvider(
            userDataProviderMock.Object,
            passwordHasherMock.Object,
            jwtTokenServiceMock.Object,
            loggerMock.Object);
    }

    [Fact]
    public async Task RegisterAsync_CreatesUser_WhenEmailIsAvailable()
    {
        var request = new RegisterRequest("testuser", "test@example.com", "password123");

        userDataProviderMock
            .Setup(x => x.GetUserByEmailAsync(request.Email))
            .ThrowsAsync(new DataNotFoundException(message: "User not found"));

        passwordHasherMock
            .Setup(x => x.HashPassword(It.IsAny<User>(), request.Password))
            .Returns("hashed-password");

        userDataProviderMock
            .Setup(x => x.CreateUserAsync(It.IsAny<User>()))
            .Returns(Task.CompletedTask);

        await sut.RegisterAsync(request);

        userDataProviderMock.Verify(x => x.CreateUserAsync(
            It.Is<User>(u => u.PasswordHash == "hashed-password"
                          && u.Email == request.Email
                          && u.Username == request.Username)),
            Times.Once);
    }

    [Fact]
    public async Task RegisterAsync_ThrowsConflictException_WhenEmailAlreadyExists()
    {
        var request = new RegisterRequest("testuser", "taken@example.com", "password123");
        var existingUser = new User("existing", "taken@example.com", "some-hash");

        userDataProviderMock
            .Setup(x => x.GetUserByEmailAsync(request.Email))
            .ReturnsAsync(existingUser);

        var act = async () => await sut.RegisterAsync(request);

        await act.Should().ThrowAsync<ConflictException>();
        userDataProviderMock.Verify(x => x.CreateUserAsync(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task RegisterAsync_NeverStoresPlainTextPassword()
    {
        var request = new RegisterRequest("testuser", "test@example.com", "plainpassword");

        userDataProviderMock
            .Setup(x => x.GetUserByEmailAsync(request.Email))
            .ThrowsAsync(new DataNotFoundException(message: "User not found"));

        passwordHasherMock
            .Setup(x => x.HashPassword(It.IsAny<User>(), request.Password))
            .Returns("hashed-password");

        userDataProviderMock
            .Setup(x => x.CreateUserAsync(It.IsAny<User>()))
            .Returns(Task.CompletedTask);

        await sut.RegisterAsync(request);

        userDataProviderMock.Verify(x => x.CreateUserAsync(
            It.Is<User>(u => u.PasswordHash != request.Password)),
            Times.Once);
    }

    [Fact]
    public async Task LoginAsync_ReturnsAuthResponseWithToken_WhenCredentialsAreValid()
    {
        var user = new User("testuser", "test@example.com", "hashed-password");

        userDataProviderMock
            .Setup(x => x.GetUserByEmailAsync(user.Email))
            .ReturnsAsync(user);

        passwordHasherMock
            .Setup(x => x.VerifyHashedPassword(user, user.PasswordHash, "password123"))
            .Returns(PasswordVerificationResult.Success);

        var result = await sut.LoginAsync(user.Email, "password123");

        result.Token.Should().NotBeNullOrEmpty();
        result.Token.Split('.').Should().HaveCount(3);
    }

    [Fact]
    public async Task LoginAsync_ThrowsUnauthorizedException_WhenEmailNotFound()
    {
        userDataProviderMock
            .Setup(x => x.GetUserByEmailAsync(It.IsAny<string>()))
            .ThrowsAsync(new DataNotFoundException(message: "User not found"));

        var act = async () => await sut.LoginAsync("ghost@example.com", "password123");

        await act.Should().ThrowAsync<UnauthorizedException>()
            .WithMessage("Invalid email or password");
    }

    [Fact]
    public async Task LoginAsync_ThrowsUnauthorizedException_WhenPasswordIsWrong()
    {
        var user = new User("testuser", "test@example.com", "hashed-password");

        userDataProviderMock
            .Setup(x => x.GetUserByEmailAsync(user.Email))
            .ReturnsAsync(user);

        passwordHasherMock
            .Setup(x => x.VerifyHashedPassword(user, user.PasswordHash, "wrongpassword"))
            .Returns(PasswordVerificationResult.Failed);

        var act = async () => await sut.LoginAsync(user.Email, "wrongpassword");

        await act.Should().ThrowAsync<UnauthorizedException>()
            .WithMessage("Invalid email or password");
    }

    [Fact]
    public async Task LoginAsync_RehashesPassword_WhenRehashNeeded()
    {
        var user = new User("testuser", "test@example.com", "old-hash");

        userDataProviderMock
            .Setup(x => x.GetUserByEmailAsync(user.Email))
            .ReturnsAsync(user);

        passwordHasherMock
            .Setup(x => x.VerifyHashedPassword(user, user.PasswordHash, "password123"))
            .Returns(PasswordVerificationResult.SuccessRehashNeeded);

        passwordHasherMock
            .Setup(x => x.HashPassword(user, "password123"))
            .Returns("new-stronger-hash");

        userDataProviderMock
            .Setup(x => x.UpdateUserAsync(It.IsAny<User>()))
            .Returns(Task.CompletedTask);

        await sut.LoginAsync(user.Email, "password123");

        userDataProviderMock.Verify(x => x.UpdateUserAsync(
            It.Is<User>(u => u.PasswordHash == "new-stronger-hash")),
            Times.Once);
    }

    [Fact]
    public async Task LoginAsync_StillReturnsToken_WhenRehashUpdateFails()
    {
        var user = new User("testuser", "test@example.com", "old-hash");

        userDataProviderMock
            .Setup(x => x.GetUserByEmailAsync(user.Email))
            .ReturnsAsync(user);

        passwordHasherMock
            .Setup(x => x.VerifyHashedPassword(user, user.PasswordHash, "password123"))
            .Returns(PasswordVerificationResult.SuccessRehashNeeded);

        passwordHasherMock
            .Setup(x => x.HashPassword(user, "password123"))
            .Returns("new-stronger-hash");

        userDataProviderMock
            .Setup(x => x.UpdateUserAsync(It.IsAny<User>()))
            .ThrowsAsync(new Exception("DB unavailable"));

        var result = await sut.LoginAsync(user.Email, "password123");

        result.Token.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task LoginAsync_StillReturnsToken_WhenRehashNeeded()
    {
        var user = new User("testuser", "test@example.com", "old-hash");

        userDataProviderMock
            .Setup(x => x.GetUserByEmailAsync(user.Email))
            .ReturnsAsync(user);

        passwordHasherMock
            .Setup(x => x.VerifyHashedPassword(user, user.PasswordHash, "password123"))
            .Returns(PasswordVerificationResult.SuccessRehashNeeded);

        passwordHasherMock
            .Setup(x => x.HashPassword(user, "password123"))
            .Returns("new-stronger-hash");

        userDataProviderMock
            .Setup(x => x.UpdateUserAsync(It.IsAny<User>()))
            .Returns(Task.CompletedTask);

        var result = await sut.LoginAsync(user.Email, "password123");

        result.Token.Should().NotBeNullOrEmpty();
        result.Token.Split('.').Should().HaveCount(3);
    }
}
