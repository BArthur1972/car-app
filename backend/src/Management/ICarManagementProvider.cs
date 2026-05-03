using Cars.Models;

namespace Cars.Management;

public interface ICarManagementProvider
{
    Task<CarResponsePayload> AddCar(CarRequestPayload car);
    Task<IEnumerable<CarResponsePayload>> GetCars();
    Task<CarResponsePayload> GetCar(string id);
    Task RemoveCar(string id);
    Task<CarResponsePayload> UpdateCar(string id, CarUpdatePayload car);
}
