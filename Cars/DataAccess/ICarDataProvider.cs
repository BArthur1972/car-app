using Cars.DataAccess.Entities;
using Cars.DataAccess.Entities.Resources;

namespace Cars.DataAccess;

public interface ICarDataProvider
{
    Task<IEnumerable<CarResponsePayload>> GetCarsAsync();
    Task<CarResponsePayload> GetCarAsync(string id);
    Task AddCarAsync(Car car);
    Task RemoveCarAsync(string id);
    Task UpdateCarAsync(string id, CarUpdatePayload updatePayload);
}
