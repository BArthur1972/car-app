using System.Net;
using Cars.ApiCommon.Exceptions;
using Cars.DataAccess;
using User = Cars.DataAccess.Entities.User;
using FluentAssertions;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Moq;

namespace DataAccess.UnitTest;

public class UserDataProviderTests
{
    private readonly Mock<Container> containerMock = new();
    private readonly Mock<ILogger<UserDataProvider>> loggerMock = new();
    private readonly UserDataProvider sut;

    public UserDataProviderTests()
    {
        sut = new UserDataProvider(containerMock.Object, loggerMock.Object);
    }

    [Fact]
    public async Task GetUserByIdAsync_ReturnsUser_WhenFound()
    {
        var user = new User("testuser", "test@example.com", "hash");
        var responseMock = new Mock<ItemResponse<User>>();
        responseMock.Setup(x => x.Resource).Returns(user);

        containerMock
            .Setup(x => x.ReadItemAsync<User>(user.Id, new PartitionKey(user.Id), null, default))
            .ReturnsAsync(responseMock.Object);

        var result = await sut.GetUserByIdAsync(user.Id);

        result.Should().BeEquivalentTo(user);
    }

    [Fact]
    public async Task GetUserByIdAsync_ThrowsDataNotFoundException_WhenCosmosReturns404()
    {
        var id = Guid.NewGuid().ToString();
        containerMock
            .Setup(x => x.ReadItemAsync<User>(id, new PartitionKey(id), null, default))
            .ThrowsAsync(CreateCosmosException(HttpStatusCode.NotFound));

        var act = async () => await sut.GetUserByIdAsync(id);

        await act.Should().ThrowAsync<DataNotFoundException>();
    }

    [Fact]
    public async Task GetUserByIdAsync_RethrowsException_WhenUnexpectedError()
    {
        var id = Guid.NewGuid().ToString();
        containerMock
            .Setup(x => x.ReadItemAsync<User>(id, new PartitionKey(id), null, default))
            .ThrowsAsync(new Exception("Unexpected"));

        var act = async () => await sut.GetUserByIdAsync(id);

        await act.Should().ThrowAsync<Exception>();
    }

    [Fact]
    public async Task GetUserByEmailAsync_ReturnsUser_WhenFound()
    {
        var user = new User("testuser", "test@example.com", "hash");
        SetupEmailQueryIterator([user]);

        var result = await sut.GetUserByEmailAsync(user.Email);

        result.Should().BeEquivalentTo(user);
    }

    [Fact]
    public async Task GetUserByEmailAsync_ThrowsDataNotFoundException_WhenUserNotFound()
    {
        SetupEmailQueryIterator([]);

        var act = async () => await sut.GetUserByEmailAsync("ghost@example.com");

        await act.Should().ThrowAsync<DataNotFoundException>();
    }

    [Fact]
    public async Task GetUserByEmailAsync_RethrowsCosmosException_WhenUnexpectedError()
    {
        var iteratorMock = new Mock<FeedIterator<User>>();
        iteratorMock.Setup(x => x.HasMoreResults).Returns(true);
        iteratorMock
            .Setup(x => x.ReadNextAsync(default))
            .ThrowsAsync(CreateCosmosException(HttpStatusCode.ServiceUnavailable));

        containerMock
            .Setup(x => x.GetItemQueryIterator<User>(It.IsAny<QueryDefinition>(), null, null))
            .Returns(iteratorMock.Object);

        var act = async () => await sut.GetUserByEmailAsync("test@example.com");

        await act.Should().ThrowAsync<CosmosException>();
    }

    [Fact]
    public async Task CreateUserAsync_CallsCreateItemAsync()
    {
        var user = new User("testuser", "test@example.com", "hash");
        containerMock
            .Setup(x => x.CreateItemAsync(user, new PartitionKey(user.Id), null, default))
            .ReturnsAsync(Mock.Of<ItemResponse<User>>());

        await sut.CreateUserAsync(user);

        containerMock.Verify(x => x.CreateItemAsync(user, new PartitionKey(user.Id), null, default), Times.Once);
    }

    [Fact]
    public async Task CreateUserAsync_ThrowsConflictException_WhenCosmosReturns409()
    {
        var user = new User("testuser", "test@example.com", "hash");
        containerMock
            .Setup(x => x.CreateItemAsync(user, new PartitionKey(user.Id), null, default))
            .ThrowsAsync(CreateCosmosException(HttpStatusCode.Conflict));

        var act = async () => await sut.CreateUserAsync(user);

        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task CreateUserAsync_RethrowsException_WhenUnexpectedError()
    {
        var user = new User("testuser", "test@example.com", "hash");
        containerMock
            .Setup(x => x.CreateItemAsync(user, new PartitionKey(user.Id), null, default))
            .ThrowsAsync(new Exception("Unexpected"));

        var act = async () => await sut.CreateUserAsync(user);

        await act.Should().ThrowAsync<Exception>();
    }

    [Fact]
    public async Task UpdateUserAsync_CallsReplaceItemAsync()
    {
        var user = new User("testuser", "test@example.com", "hash");
        containerMock
            .Setup(x => x.ReplaceItemAsync(user, user.Id, new PartitionKey(user.Id), null, default))
            .ReturnsAsync(Mock.Of<ItemResponse<User>>());

        await sut.UpdateUserAsync(user);

        containerMock.Verify(x => x.ReplaceItemAsync(user, user.Id, new PartitionKey(user.Id), null, default), Times.Once);
    }

    [Fact]
    public async Task UpdateUserAsync_ThrowsDataNotFoundException_WhenCosmosReturns404()
    {
        var user = new User("testuser", "test@example.com", "hash");
        containerMock
            .Setup(x => x.ReplaceItemAsync(user, user.Id, new PartitionKey(user.Id), null, default))
            .ThrowsAsync(CreateCosmosException(HttpStatusCode.NotFound));

        var act = async () => await sut.UpdateUserAsync(user);

        await act.Should().ThrowAsync<DataNotFoundException>();
    }

    [Fact]
    public async Task UpdateUserAsync_RethrowsException_WhenUnexpectedError()
    {
        var user = new User("testuser", "test@example.com", "hash");
        containerMock
            .Setup(x => x.ReplaceItemAsync(user, user.Id, new PartitionKey(user.Id), null, default))
            .ThrowsAsync(new Exception("Unexpected"));

        var act = async () => await sut.UpdateUserAsync(user);

        await act.Should().ThrowAsync<Exception>();
    }

    [Fact]
    public async Task DeleteUserAsync_CallsDeleteItemAsync()
    {
        var id = Guid.NewGuid().ToString();
        containerMock
            .Setup(x => x.DeleteItemAsync<User>(id, new PartitionKey(id), null, default))
            .ReturnsAsync(Mock.Of<ItemResponse<User>>());

        await sut.DeleteUserAsync(id);

        containerMock.Verify(x => x.DeleteItemAsync<User>(id, new PartitionKey(id), null, default), Times.Once);
    }

    [Fact]
    public async Task DeleteUserAsync_ThrowsDataNotFoundException_WhenCosmosReturns404()
    {
        var id = Guid.NewGuid().ToString();
        containerMock
            .Setup(x => x.DeleteItemAsync<User>(id, new PartitionKey(id), null, default))
            .ThrowsAsync(CreateCosmosException(HttpStatusCode.NotFound));

        var act = async () => await sut.DeleteUserAsync(id);

        await act.Should().ThrowAsync<DataNotFoundException>();
    }

    [Fact]
    public async Task DeleteUserAsync_RethrowsException_WhenUnexpectedError()
    {
        var id = Guid.NewGuid().ToString();
        containerMock
            .Setup(x => x.DeleteItemAsync<User>(id, new PartitionKey(id), null, default))
            .ThrowsAsync(new Exception("Unexpected"));

        var act = async () => await sut.DeleteUserAsync(id);

        await act.Should().ThrowAsync<Exception>();
    }

    private static CosmosException CreateCosmosException(HttpStatusCode statusCode)
        => new(statusCode.ToString(), statusCode, 0, string.Empty, 0);

    private void SetupEmailQueryIterator(List<User> users)
    {
        var feedResponseMock = new Mock<FeedResponse<User>>();
        feedResponseMock
            .Setup(x => x.GetEnumerator())
            .Returns(users.GetEnumerator());

        var iteratorMock = new Mock<FeedIterator<User>>();
        iteratorMock
            .SetupSequence(x => x.HasMoreResults)
            .Returns(true)
            .Returns(false);
        iteratorMock
            .Setup(x => x.ReadNextAsync(default))
            .ReturnsAsync(feedResponseMock.Object);

        containerMock
            .Setup(x => x.GetItemQueryIterator<User>(It.IsAny<QueryDefinition>(), null, null))
            .Returns(iteratorMock.Object);
    }
}
