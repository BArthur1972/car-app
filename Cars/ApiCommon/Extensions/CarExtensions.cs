using Cars.DataAccess.Entities;
using Cars.DataAccess.Entities.Resources;

namespace Cars.ApiCommon.Extensions;

/// <summary>
/// Extension methods for car-related models
/// </summary>
public static class CarExtensions
{
    /// <summary>
    /// Converts a CarRequestPayload to a Car
    /// </summary>
    public static Car ToCar(this CarRequestPayload request)
    {
        return new Car(
            request.Make,
            request.Model,
            request.Year,
            request.ImageUrl
        );
    }

    /// <summary>
    /// Converts a domain Car object to a CarResponsePayload
    /// </summary>
    public static CarResponsePayload ToResponsePayload(this Car car)
    {
        return new CarResponsePayload(
            car.Id,
            car.Make,
            car.Model,
            car.Year,
            car.ImageUrl
        );
    }

    /// <summary>
    /// Converts a collection of domain Car objects to CarResponsePayload objects
    /// </summary>
    public static IEnumerable<CarResponsePayload> ToResponsePayloads(this IEnumerable<Car> cars)
    {
        return cars.Select(car => car.ToResponsePayload());
    }
}
