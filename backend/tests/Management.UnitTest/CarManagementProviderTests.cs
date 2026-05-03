using Cars.ApiCommon.Exceptions;
using Cars.DataAccess;
using Cars.DataAccess.Entities;
using Cars.Management;
using Cars.Models;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace Management.UnitTest;

public class CarManagementProviderTests
{
    private readonly Mock<ICarDataProvider> _dataProviderMock = new();
    private readonly Mock<ILogger<CarManagementProvider>> _loggerMock = new();
    private readonly CarManagementProvider _sut;

    public CarManagementProviderTests()
    {
        _sut = new CarManagementProvider(_dataProviderMock.Object, _loggerMock.Object);
    }

    // AddCar

    [Fact]
    public async Task AddCar_ReturnsMappedCarResponsePayload()
    {
        var request = new CarRequestPayload("Toyota", "Camry", 2024, null);
        _dataProviderMock
            .Setup(x => x.AddCarAsync(It.IsAny<Car>()))
            .Returns(Task.CompletedTask);

        var result = await _sut.AddCar(request);

        result.Make.Should().Be("Toyota");
        result.Model.Should().Be("Camry");
        result.Year.Should().Be(2024);
    }

    [Fact]
    public async Task AddCar_RethrowsException_WhenDataProviderFails()
    {
        var request = new CarRequestPayload("Toyota", "Camry", 2024, null);
        _dataProviderMock
            .Setup(x => x.AddCarAsync(It.IsAny<Car>()))
            .ThrowsAsync(new Exception("DB error"));

        var act = async () => await _sut.AddCar(request);

        await act.Should().ThrowAsync<Exception>();
    }

    // GetCars

    [Fact]
    public async Task GetCars_ReturnsMappedCarResponsePayloads()
    {
        var cars = new List<Car>
        {
            new("Toyota", "Camry", 2024),
            new("BMW", "M3", 2023)
        };
        _dataProviderMock
            .Setup(x => x.GetCarsAsync())
            .ReturnsAsync(cars);

        var result = (await _sut.GetCars()).ToList();

        result.Should().HaveCount(2);
        result.Should().Contain(c => c.Make == "Toyota");
        result.Should().Contain(c => c.Make == "BMW");
    }

    [Fact]
    public async Task GetCars_RethrowsException_WhenDataProviderFails()
    {
        _dataProviderMock
            .Setup(x => x.GetCarsAsync())
            .ThrowsAsync(new Exception("DB error"));

        var act = async () => await _sut.GetCars();

        await act.Should().ThrowAsync<Exception>();
    }

    // GetCar

    [Fact]
    public async Task GetCar_ReturnsMappedCarResponsePayload()
    {
        var car = new Car("Toyota", "Camry", 2024);
        _dataProviderMock
            .Setup(x => x.GetCarAsync(car.Id))
            .ReturnsAsync(car);

        var result = await _sut.GetCar(car.Id);

        result.Id.Should().Be(car.Id);
        result.Make.Should().Be("Toyota");
        result.Model.Should().Be("Camry");
    }

    [Fact]
    public async Task GetCar_RethrowsDataNotFoundException_WhenCarDoesNotExist()
    {
        _dataProviderMock
            .Setup(x => x.GetCarAsync(It.IsAny<string>()))
            .ThrowsAsync(new DataNotFoundException(message: "Car not found"));

        var act = async () => await _sut.GetCar("nonexistent-id");

        await act.Should().ThrowAsync<DataNotFoundException>();
    }

    // RemoveCar

    [Fact]
    public async Task RemoveCar_CallsDataProviderOnce()
    {
        var id = Guid.NewGuid().ToString();
        _dataProviderMock
            .Setup(x => x.RemoveCarAsync(id))
            .Returns(Task.CompletedTask);

        await _sut.RemoveCar(id);

        _dataProviderMock.Verify(x => x.RemoveCarAsync(id), Times.Once);
    }

    [Fact]
    public async Task RemoveCar_RethrowsException_WhenDataProviderFails()
    {
        _dataProviderMock
            .Setup(x => x.RemoveCarAsync(It.IsAny<string>()))
            .ThrowsAsync(new Exception("DB error"));

        var act = async () => await _sut.RemoveCar("some-id");

        await act.Should().ThrowAsync<Exception>();
    }

    // UpdateCar

    [Fact]
    public async Task UpdateCar_ThrowsBadRequestException_WhenPayloadHasNoUpdates()
    {
        var emptyPayload = new CarUpdatePayload();

        var act = async () => await _sut.UpdateCar("some-id", emptyPayload);

        await act.Should().ThrowAsync<BadRequestException>();
    }

    [Fact]
    public async Task UpdateCar_ReturnsMappedCarResponsePayload_WhenSuccessful()
    {
        var id = Guid.NewGuid().ToString();
        var updatePayload = new CarUpdatePayload(make: "Honda");
        var updatedCar = new Car("Honda", "Civic", 2023);

        _dataProviderMock
            .Setup(x => x.UpdateCarAsync(id, updatePayload))
            .Returns(Task.CompletedTask);
        _dataProviderMock
            .Setup(x => x.GetCarAsync(id))
            .ReturnsAsync(updatedCar);

        var result = await _sut.UpdateCar(id, updatePayload);

        result.Make.Should().Be("Honda");
        result.Model.Should().Be("Civic");
    }

    [Fact]
    public async Task UpdateCar_RethrowsException_WhenDataProviderFails()
    {
        var id = Guid.NewGuid().ToString();
        var updatePayload = new CarUpdatePayload(make: "Honda");

        _dataProviderMock
            .Setup(x => x.UpdateCarAsync(id, updatePayload))
            .ThrowsAsync(new Exception("DB error"));

        var act = async () => await _sut.UpdateCar(id, updatePayload);

        await act.Should().ThrowAsync<Exception>();
    }
}
