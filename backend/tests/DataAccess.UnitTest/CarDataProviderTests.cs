using System.Net;
using Cars.ApiCommon.Exceptions;
using Cars.DataAccess;
using Cars.DataAccess.Entities;
using Cars.Models;
using FluentAssertions;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Moq;

namespace DataAccess.UnitTest;

public class CarDataProviderTests
{
    private readonly Mock<Container> containerMock = new();
    private readonly Mock<ILogger<CarDataProvider>> loggerMock = new();
    private readonly CarDataProvider sut;

    public CarDataProviderTests()
    {
        sut = new CarDataProvider(containerMock.Object, loggerMock.Object);
    }

    [Fact]
    public async Task AddCarAsync_CallsUpsertItemAsync()
    {
        var car = new Car("Toyota", "Camry", 2024);
        containerMock
            .Setup(x => x.UpsertItemAsync(car, new PartitionKey(car.Id), null, default))
            .ReturnsAsync(Mock.Of<ItemResponse<Car>>());

        await sut.AddCarAsync(car);

        containerMock.Verify(x => x.UpsertItemAsync(car, new PartitionKey(car.Id), null, default), Times.Once);
    }

    [Fact]
    public async Task AddCarAsync_ThrowsBadRequestException_WhenCosmosReturns400()
    {
        var car = new Car("Toyota", "Camry", 2024);
        var cosmosEx = CreateCosmosException(HttpStatusCode.BadRequest);
        containerMock
            .Setup(x => x.UpsertItemAsync(car, new PartitionKey(car.Id), null, default))
            .ThrowsAsync(cosmosEx);

        var act = async () => await sut.AddCarAsync(car);

        await act.Should().ThrowAsync<BadRequestException>();
    }

    [Fact]
    public async Task AddCarAsync_RethrowsCosmosException_WhenNotBadRequest()
    {
        var car = new Car("Toyota", "Camry", 2024);
        var cosmosEx = CreateCosmosException(HttpStatusCode.ServiceUnavailable);
        containerMock
            .Setup(x => x.UpsertItemAsync(car, new PartitionKey(car.Id), null, default))
            .ThrowsAsync(cosmosEx);

        var act = async () => await sut.AddCarAsync(car);

        await act.Should().ThrowAsync<CosmosException>();
    }

    [Fact]
    public async Task GetCarAsync_ReturnsCar()
    {
        var car = new Car("Toyota", "Camry", 2024);
        var responseMock = new Mock<ItemResponse<Car>>();
        responseMock.Setup(x => x.Resource).Returns(car);

        containerMock
            .Setup(x => x.ReadItemAsync<Car>(car.Id, new PartitionKey(car.Id), null, default))
            .ReturnsAsync(responseMock.Object);

        var result = await sut.GetCarAsync(car.Id);

        result.Should().BeEquivalentTo(car);
    }

    [Fact]
    public async Task GetCarAsync_ThrowsDataNotFoundException_WhenCosmosReturns404()
    {
        var id = Guid.NewGuid().ToString();
        var cosmosEx = CreateCosmosException(HttpStatusCode.NotFound);
        containerMock
            .Setup(x => x.ReadItemAsync<Car>(id, new PartitionKey(id), null, default))
            .ThrowsAsync(cosmosEx);

        var act = async () => await sut.GetCarAsync(id);

        await act.Should().ThrowAsync<DataNotFoundException>();
    }

    [Fact]
    public async Task GetCarAsync_RethrowsException_WhenUnexpectedErrorOccurs()
    {
        var id = Guid.NewGuid().ToString();
        containerMock
            .Setup(x => x.ReadItemAsync<Car>(id, new PartitionKey(id), null, default))
            .ThrowsAsync(new Exception("Unexpected"));

        var act = async () => await sut.GetCarAsync(id);

        await act.Should().ThrowAsync<Exception>();
    }

    [Fact]
    public async Task GetCarsAsync_ReturnsCars()
    {
        var cars = new List<Car> { new("Toyota", "Camry", 2024), new("BMW", "M3", 2023) };
        SetupQueryIterator(cars);

        var result = await sut.GetCarsAsync();

        result.Should().HaveCount(2);
        result.Should().Contain(c => c.Make == "Toyota");
        result.Should().Contain(c => c.Make == "BMW");
    }

    [Fact]
    public async Task GetCarsAsync_ThrowsDataNotFoundException_WhenNoCarsExist()
    {
        SetupQueryIterator([]);

        var act = async () => await sut.GetCarsAsync();

        await act.Should().ThrowAsync<DataNotFoundException>();
    }

    [Fact]
    public async Task GetCarsAsync_RethrowsCosmosException()
    {
        var cosmosEx = CreateCosmosException(HttpStatusCode.ServiceUnavailable);
        var iteratorMock = new Mock<FeedIterator<Car>>();
        iteratorMock.Setup(x => x.HasMoreResults).Returns(true);
        iteratorMock.Setup(x => x.ReadNextAsync(default)).ThrowsAsync(cosmosEx);

        containerMock
            .Setup(x => x.GetItemQueryIterator<Car>("SELECT * FROM c", null, null))
            .Returns(iteratorMock.Object);

        var act = async () => await sut.GetCarsAsync();

        await act.Should().ThrowAsync<CosmosException>();
    }

    [Fact]
    public async Task RemoveCarAsync_CallsDeleteItemAsync()
    {
        var id = Guid.NewGuid().ToString();
        containerMock
            .Setup(x => x.DeleteItemAsync<Car>(id, new PartitionKey(id), null, default))
            .ReturnsAsync(Mock.Of<ItemResponse<Car>>());

        await sut.RemoveCarAsync(id);

        containerMock.Verify(x => x.DeleteItemAsync<Car>(id, new PartitionKey(id), null, default), Times.Once);
    }

    [Fact]
    public async Task RemoveCarAsync_RethrowsCosmosException()
    {
        var id = Guid.NewGuid().ToString();
        var cosmosEx = CreateCosmosException(HttpStatusCode.ServiceUnavailable);
        containerMock
            .Setup(x => x.DeleteItemAsync<Car>(id, new PartitionKey(id), null, default))
            .ThrowsAsync(cosmosEx);

        var act = async () => await sut.RemoveCarAsync(id);

        await act.Should().ThrowAsync<CosmosException>();
    }

    [Fact]
    public async Task UpdateCarAsync_ExecutesBatchSuccessfully()
    {
        var id = Guid.NewGuid().ToString();
        var updatePayload = new CarUpdatePayload(make: "Honda");
        var (batchMock, _) = SetupBatch(id, HttpStatusCode.OK, isSuccess: true);

        await sut.UpdateCarAsync(id, updatePayload);

        batchMock.Verify(x => x.ExecuteAsync(default), Times.Once);
    }

    [Fact]
    public async Task UpdateCarAsync_ThrowsDataNotFoundException_WhenBatchReturns404()
    {
        var id = Guid.NewGuid().ToString();
        var updatePayload = new CarUpdatePayload(make: "Honda");
        SetupBatch(id, HttpStatusCode.NotFound, isSuccess: false);

        var act = async () => await sut.UpdateCarAsync(id, updatePayload);

        await act.Should().ThrowAsync<DataNotFoundException>();
    }

    [Fact]
    public async Task UpdateCarAsync_ThrowsInternalServerErrorException_WhenBatchReturnsOtherFailure()
    {
        var id = Guid.NewGuid().ToString();
        var updatePayload = new CarUpdatePayload(make: "Honda");
        SetupBatch(id, HttpStatusCode.ServiceUnavailable, isSuccess: false);

        var act = async () => await sut.UpdateCarAsync(id, updatePayload);

        await act.Should().ThrowAsync<InternalServerErrorException>();
    }

    [Fact]
    public async Task UpdateCarAsync_RethrowsCosmosException()
    {
        var id = Guid.NewGuid().ToString();
        var updatePayload = new CarUpdatePayload(make: "Honda");
        var batchMock = new Mock<TransactionalBatch>();
        batchMock.Setup(x => x.PatchItem(It.IsAny<string>(), It.IsAny<IReadOnlyList<PatchOperation>>(), null))
            .Returns(batchMock.Object);
        batchMock.Setup(x => x.ExecuteAsync(default))
            .ThrowsAsync(CreateCosmosException(HttpStatusCode.ServiceUnavailable));

        containerMock
            .Setup(x => x.CreateTransactionalBatch(new PartitionKey(id)))
            .Returns(batchMock.Object);

        var act = async () => await sut.UpdateCarAsync(id, updatePayload);

        await act.Should().ThrowAsync<CosmosException>();
    }

    private static CosmosException CreateCosmosException(HttpStatusCode statusCode)
        => new(statusCode.ToString(), statusCode, 0, string.Empty, 0);

    private (Mock<TransactionalBatch> batch, Mock<TransactionalBatchResponse> response) SetupBatch(
        string id, HttpStatusCode statusCode, bool isSuccess)
    {
        var responseMock = new Mock<TransactionalBatchResponse>();
        responseMock.Setup(x => x.IsSuccessStatusCode).Returns(isSuccess);
        responseMock.Setup(x => x.StatusCode).Returns(statusCode);
        responseMock.Setup(x => x.ErrorMessage).Returns(string.Empty);

        var batchMock = new Mock<TransactionalBatch>();
        batchMock.Setup(x => x.PatchItem(It.IsAny<string>(), It.IsAny<IReadOnlyList<PatchOperation>>(), null))
            .Returns(batchMock.Object);
        batchMock.Setup(x => x.ExecuteAsync(default))
            .ReturnsAsync(responseMock.Object);

        containerMock
            .Setup(x => x.CreateTransactionalBatch(new PartitionKey(id)))
            .Returns(batchMock.Object);

        return (batchMock, responseMock);
    }

    private void SetupQueryIterator(List<Car> cars)
    {
        var feedResponseMock = new Mock<FeedResponse<Car>>();
        feedResponseMock
            .Setup(x => x.GetEnumerator())
            .Returns(cars.GetEnumerator());

        var iteratorMock = new Mock<FeedIterator<Car>>();
        iteratorMock
            .SetupSequence(x => x.HasMoreResults)
            .Returns(cars.Count > 0)
            .Returns(false);
        iteratorMock
            .Setup(x => x.ReadNextAsync(default))
            .ReturnsAsync(feedResponseMock.Object);

        containerMock
            .Setup(x => x.GetItemQueryIterator<Car>("SELECT * FROM c", null, null))
            .Returns(iteratorMock.Object);
    }
}
