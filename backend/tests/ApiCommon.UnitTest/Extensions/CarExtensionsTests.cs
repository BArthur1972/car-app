using Cars.ApiCommon.Extensions;
using Cars.DataAccess.Entities;
using Cars.Models;
using FluentAssertions;

namespace ApiCommon.UnitTest.Extensions;

public class CarExtensionsTests
{
    // ToCar

    [Fact]
    public void ToCar_MapsAllProperties()
    {
        var request = new CarRequestPayload("Toyota", "Camry", 2024, "https://image.url");

        var car = request.ToCar();

        car.Make.Should().Be("Toyota");
        car.Model.Should().Be("Camry");
        car.Year.Should().Be(2024);
        car.ImageUrl.Should().Be("https://image.url");
    }

    [Fact]
    public void ToCar_MapsNullImageUrl()
    {
        var request = new CarRequestPayload("Toyota", "Camry", 2024, null);

        var car = request.ToCar();

        car.ImageUrl.Should().BeNull();
    }

    [Fact]
    public void ToCar_GeneratesNonEmptyId()
    {
        var request = new CarRequestPayload("Toyota", "Camry", 2024, null);

        var car = request.ToCar();

        car.Id.Should().NotBeNullOrEmpty();
    }

    // ToResponsePayload

    [Fact]
    public void ToResponsePayload_MapsAllProperties()
    {
        var car = new Car("Toyota", "Camry", 2024, "https://image.url");

        var payload = car.ToResponsePayload();

        payload.Id.Should().Be(car.Id);
        payload.Make.Should().Be("Toyota");
        payload.Model.Should().Be("Camry");
        payload.Year.Should().Be(2024);
        payload.ImageUrl.Should().Be("https://image.url");
    }

    [Fact]
    public void ToResponsePayload_MapsNullImageUrl()
    {
        var car = new Car("Toyota", "Camry", 2024);

        var payload = car.ToResponsePayload();

        payload.ImageUrl.Should().BeNull();
    }

    // ToResponsePayloads

    [Fact]
    public void ToResponsePayloads_MapsCollection()
    {
        var cars = new List<Car>
        {
            new("Toyota", "Camry", 2024),
            new("BMW", "M3", 2023)
        };

        var payloads = cars.ToResponsePayloads().ToList();

        payloads.Should().HaveCount(2);
        payloads[0].Make.Should().Be("Toyota");
        payloads[1].Make.Should().Be("BMW");
    }

    [Fact]
    public void ToResponsePayloads_ReturnsEmpty_WhenCollectionIsEmpty()
    {
        var payloads = new List<Car>().ToResponsePayloads();

        payloads.Should().BeEmpty();
    }
}
