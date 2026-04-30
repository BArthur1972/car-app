using Cars.DataAccess.Entities;
using Cars.DataAccess.Entities.Resources;
using Cars.DataAccess;
using Cars.ApiCommon.Extensions;
using Cars.ApiCommon.Exceptions;

namespace Cars.Management
{
    public class CarManagementProvider(
        ICarDataProvider carDataProvider,
        ILogger<CarManagementProvider> logger)
        : ICarManagementProvider
    {
        private readonly ICarDataProvider carDataProvider = carDataProvider;
        private readonly ILogger<CarManagementProvider> logger = logger;

        public async Task<Car> AddCar(CarRequestPayload carRequestPayload)
        {
            try
            {
                Car newCar = carRequestPayload.ToCar();
                await carDataProvider.AddCarAsync(newCar);
                logger.LogInformation("Added car: " + newCar.ToString());
                return newCar;
            }
            catch (Exception e)
            {
                logger.LogError(e, "Failed to add car: " + e.Message);
                throw;
            }
        }

        public async Task<IEnumerable<CarResponsePayload>> GetCars()
        {
            try
            {
                var response = await carDataProvider.GetCarsAsync();

                logger.LogInformation("Cars obtained: " + response.Count() + " cars");
                return response;
            }
            catch (Exception e)
            {
                logger.LogError(e, "Failed to get cars: " + e.Message);
                throw;
            }
        }

        public async Task<CarResponsePayload?> GetCar(string id)
        {
            try
            {
                var car = await carDataProvider.GetCarAsync(id);
                logger.LogInformation("Car obtained: " + car.ToString());
                return car;
            }
            catch (Exception e)
            {
                logger.LogError(e, "Failed to get car: " + e.Message);
                throw;
            }
        }

        public async Task RemoveCar(string id)
        {
            try
            {
                await carDataProvider.RemoveCarAsync(id);
                logger.LogInformation("Removed car: " + id);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Failed to remove car: " + e.Message);
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
                logger.LogInformation($"Updated car with ID: {id}, Changes: {updatePayload}");

                return await carDataProvider.GetCarAsync(id).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                logger.LogError(e, $"Failed to update car with ID: {id}: {e.Message}");
                throw;
            }
        }
    }
}
