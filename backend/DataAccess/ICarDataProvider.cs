using Cars.DataAccess.Entities;
using Cars.Models;

namespace Cars.DataAccess;

public interface ICarDataProvider
{
    Task<IEnumerable<Car>> GetCarsAsync();
    Task<Car> GetCarAsync(string id);
    Task AddCarAsync(Car car);
    Task RemoveCarAsync(string id);
    Task UpdateCarAsync(string id, CarUpdatePayload updatePayload);
}
