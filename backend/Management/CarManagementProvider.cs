using Cars.DataAccess;
using Cars.ApiCommon.Extensions;
using Cars.ApiCommon.Exceptions;
using Cars.Models;

namespace Cars.Management;

public class CarManagementProvider(
    ICarDataProvider carDataProvider,
    ILogger<CarManagementProvider> logger)
    : ICarManagementProvider
{
    private readonly ICarDataProvider carDataProvider = carDataProvider;
    private readonly ILogger<CarManagementProvider> logger = logger;

    public async Task<CarResponsePayload> AddCar(CarRequestPayload carRequestPayload)
    {
        try
        {
            var newCar = carRequestPayload.ToCar();
            await carDataProvider.AddCarAsync(newCar);
            logger.LogInformation("Added car: {Car}", newCar);
            return newCar.ToResponsePayload();
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to add car");
            throw;
        }
    }

    public async Task<IEnumerable<CarResponsePayload>> GetCars()
    {
        try
        {
            var cars = await carDataProvider.GetCarsAsync();
            logger.LogInformation("Cars obtained: {Count} cars", cars.Count());
            return cars.ToResponsePayloads();
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to get cars");
            throw;
        }
    }

    public async Task<CarResponsePayload> GetCar(string id)
    {
        try
        {
            var car = await carDataProvider.GetCarAsync(id);
            logger.LogInformation("Car obtained: {Car}", car);
            return car.ToResponsePayload();
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to get car");
            throw;
        }
    }

    public async Task RemoveCar(string id)
    {
        try
        {
            await carDataProvider.RemoveCarAsync(id);
            logger.LogInformation("Removed car with id: {Id}", id);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to remove car");
            throw;
        }
    }

    public async Task<CarResponsePayload> UpdateCar(string id, CarUpdatePayload updatePayload)
    {
        if (!updatePayload.HasUpdates())
            throw new BadRequestException(
                message: "Update payload must contain at least one property to update");

        try
        {
            await carDataProvider.UpdateCarAsync(id, updatePayload);
            logger.LogInformation("Updated car with ID: {Id}, Changes: {UpdatePayload}", id, updatePayload);

            var car = await carDataProvider.GetCarAsync(id).ConfigureAwait(false);
            return car.ToResponsePayload();
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to update car with ID: {Id}", id);
            throw;
        }
    }
}
