using Cars.DataAccess.Entities;
using Cars.Models;

namespace Cars.ApiCommon.Extensions;

public static class CarExtensions
{
    public static Car ToCar(this CarRequestPayload request)
    {
        return new Car(
            request.Make,
            request.Model,
            request.Year,
            request.ImageUrl
        );
    }

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

    public static IEnumerable<CarResponsePayload> ToResponsePayloads(this IEnumerable<Car> cars)
    {
        return cars.Select(car => car.ToResponsePayload());
    }
}
