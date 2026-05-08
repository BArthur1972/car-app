using Cars.ApiCommon.Exceptions;
using Cars.DataAccess.Entities;
using Cars.Models;
using Microsoft.Azure.Cosmos;

namespace Cars.DataAccess;

public class CarDataProvider(Container container, ILogger<CarDataProvider> logger)
    : ICarDataProvider
{
    public async Task AddCarAsync(Car car)
    {
        try
        {
            await container.UpsertItemAsync<Car>(car, new PartitionKey(car.Id));
            logger.LogInformation("Added car: {Car}", car);
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.BadRequest)
        {
            logger.LogError(
                ex,
                "Invalid car payload: Id={Id}, Make={Make}, Model={Model}, Year={Year}",
                car.Id,
                car.Make,
                car.Model,
                car.Year);

            throw new BadRequestException(
                message: $"The car is invalid: {ex.Message}", innerException: ex);
        }
        catch (CosmosException e)
        {
            logger.LogError(e, "Failed to add car");
            throw;
        }
    }

    public async Task<Car> GetCarAsync(string id)
    {
        try
        {
            ItemResponse<Car> response =
                await container.ReadItemAsync<Car>(id, new PartitionKey(id));

            logger.LogInformation("Car obtained: {Car}", response.Resource);
            return response.Resource;
        }
        catch (CosmosException e) when (e.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            throw new DataNotFoundException(message: "Car not found");
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to get car");
            throw;
        }
    }

    public async Task<IEnumerable<Car>> GetCarsAsync()
    {
        List<Car> cars = [];
        try
        {
            var query = container.GetItemQueryIterator<Car>("SELECT * FROM c");
            while (query.HasMoreResults)
            {
                var response = await query.ReadNextAsync();
                cars.AddRange(response);
            }

            logger.LogDebug("Cars obtained: {Count} cars", cars.Count);
            return cars;
        }
        catch (CosmosException e) when (e.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            logger.LogInformation("Container or database not found, returning empty list");
            return cars;
        }
        catch (CosmosException e)
        {
            logger.LogError(e, "Failed to get cars");
            throw;
        }
    }

    public async Task RemoveCarAsync(string id)
    {
        try
        {
            await container.DeleteItemAsync<Car>(id, new PartitionKey(id));
            logger.LogInformation("Deleted car with Id: {Id}", id);
        }
        catch (CosmosException e)
        {
            logger.LogError(e, "Failed to delete car");
            throw;
        }
    }

    public async Task UpdateCarAsync(string id, CarUpdatePayload updatePayload)
    {
        try
        {
            var patchOperations = CreatePatchOperations(updatePayload);

            var batch = container.CreateTransactionalBatch(new PartitionKey(id));
            batch.PatchItem(id, [.. patchOperations]);

            var response = await batch.ExecuteAsync();

            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    logger.LogError("Car with Id: {Id} not found", id);
                    throw new DataNotFoundException(message: $"Car with Id {id} not found");
                }

                logger.LogError(
                    "Failed to update car with Id: {Id}, Status: {Status}, Message: {Message}",
                    id,
                    response.StatusCode,
                    response.ErrorMessage);
                throw new InternalServerErrorException(
                    message: $"Failed to update car with Id: {id}");
            }

            logger.LogInformation("Successfully updated car with Id: {Id}", id);
        }
        catch (CosmosException e)
        {
            logger.LogError(e, "Failed to update car with Id: {Id}", id);
            throw;
        }
    }

    private static List<PatchOperation> CreatePatchOperations(CarUpdatePayload updatePayload)
    {
        var patchOperations = new List<PatchOperation>();

        if (updatePayload.Make != null)
            patchOperations.Add(PatchOperation.Set("/make", updatePayload.Make));

        if (updatePayload.Model != null)
            patchOperations.Add(PatchOperation.Set("/model", updatePayload.Model));

        if (updatePayload.Year != null)
            patchOperations.Add(PatchOperation.Set("/year", updatePayload.Year));

        if (!string.IsNullOrWhiteSpace(updatePayload.ImageUrl))
            patchOperations.Add(PatchOperation.Set("/imageUrl", updatePayload.ImageUrl));

        return patchOperations;
    }
}
