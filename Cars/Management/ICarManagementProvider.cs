using Cars.DataAccess.Entities;
using Cars.DataAccess.Entities.Resources;

namespace Cars.Management;

public interface ICarManagementProvider
{
    Task<Car> AddCar(CarRequestPayload car);
    Task<IEnumerable<CarResponsePayload>> GetCars();
    Task<CarResponsePayload> GetCar(string id);
    Task RemoveCar(string id);
    Task<CarResponsePayload> UpdateCar(string id, CarUpdatePayload car);
}
