using Cars.ApiCommon.Cosmos;
using Cars.ApiCommon.Cosmos.Options;
using Cars.ApiCommon.Exceptions;
using Cars.DataAccess.Entities;
using Cars.DataAccess.Entities.Resources;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;

namespace Cars.DataAccess
{
    public class CarDataProvider : ICarDataProvider
    {
        private readonly CosmosAccountOptions cosmosAccountOptions;
        private readonly CosmosContainerOptions cosmosContainerOptions;
        private readonly Container container;
        private readonly ILogger<CarDataProvider> logger;
        
        public CarDataProvider(IOptions<CosmosAccountOptions> cosmosAccountOptions, IOptions<CosmosContainerOptions> cosmosContainerOptions, ILogger<CarDataProvider> logger)
        {
            this.cosmosAccountOptions = cosmosAccountOptions.Value;
            this.cosmosContainerOptions = cosmosContainerOptions.Value;
            this.logger = logger;
            container = new CosmosFacade(this.cosmosAccountOptions, this.cosmosContainerOptions, this.logger).GetContainer();
        }

        public async Task AddCarAsync(Car car)
        {
            try
            {
                await container.UpsertItemAsync<Car>(car, new PartitionKey(car.Id));
                logger.LogInformation("Added car: " + car.ToString());
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {   
                logger.LogError(ex, $"Car causing error: Id={car.Id}, Make={car.Make}, Model={car.Model} Year={car.Year}");
                
                throw new ArgumentException($"The car is invalid: {ex.Message}", ex);
            }
            catch (CosmosException e)
            {
                logger.LogError(e, "Failed to add car: " + e.Message);
                throw;
            }
        }

        public async Task<CarResponsePayload> GetCarAsync(string id)
        {
            try
            {
                ItemResponse<CarResponsePayload> response =
                    await container.ReadItemAsync<CarResponsePayload>(id, new PartitionKey(id));
                
                logger.LogInformation("Car obtained: " + response.Resource.ToString());
                return response.Resource;
            }
            catch (CosmosException e) when (e.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                throw new DataNotFoundException(message: "Car not found");
            }
            catch (Exception e)
            {
                logger.LogError(e, "Failed to get car: " + e.Message);
                throw;
            }
        }

        public async Task<IEnumerable<CarResponsePayload>> GetCarsAsync()
        {
            List<CarResponsePayload>? cars = [];
            try
            {
                var query = container.GetItemQueryIterator<CarResponsePayload>("SELECT * FROM c");
                while (query.HasMoreResults)
                {
                    var response = await query.ReadNextAsync();
                    cars.AddRange(response);
                }

                if (cars.Count == 0)
                {
                    logger.LogError("No cars found");
                    throw new DataNotFoundException(message: "No cars found");
                }

                logger.LogDebug("Cars obtained: " + cars.Count + " cars");
                return cars;
            }
            catch (CosmosException e)
            {
                logger.LogError(e, "Failed to get cars: " + e);
                throw;
            }
        }

        public async Task RemoveCarAsync(string id)
        {
            try
            {
                await container.DeleteItemAsync<Car>(id, new PartitionKey(id));
                logger.LogInformation("Deleted car with ID: " + id);
            }
            catch (CosmosException e)
            {
                logger.LogError(e, "Failed to delete car: " + e.Message);
                throw;
            }
        }
        
        public async Task UpdateCarAsync(string id, CarUpdatePayload updatePayload)
        {
            try
            {
                // Create patch operations
                var patchOperations = CreatePatchOperations(updatePayload);
                
                // If no properties to update, return early
                if (!patchOperations.Any())
                {
                    logger.LogInformation($"No properties to update for car with ID: {id}");
                    return;
                }
                
                // Create a transactional batch with all patch operations
                var batch = container.CreateTransactionalBatch(new PartitionKey(id));
                batch.PatchItem(id, [.. patchOperations]);
                
                // Execute the batch transaction
                var response = await batch.ExecuteAsync();
                
                // Check if the batch operation was successful
                if (!response.IsSuccessStatusCode)
                {
                    if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                    {
                        logger.LogError($"Car with ID: {id} not found.");
                        throw new DataNotFoundException($"Car with ID {id} not found");
                    }

                    logger.LogError($"Failed to update car with ID: {id}, Status: {response.StatusCode}, Message: {response.ErrorMessage}");
                    throw new Exception($"Failed to update car with ID: {id}. Status: {response.StatusCode}, Message: {response.ErrorMessage}");
                }
                
                logger.LogInformation($"Successfully updated car with ID: {id}");
            }
            catch (CosmosException e)
            {
                logger.LogError(e, $"Failed to update car: {e.Message}");
                throw;
            }
        }

        /// <summary>
        /// Creates a list of patch operations based on the properties in the update payload
        /// </summary>
        private static List<PatchOperation> CreatePatchOperations(CarUpdatePayload updatePayload)
        {
            var patchOperations = new List<PatchOperation>();
            
            // Add patch operations for each property that needs to be updated
            if (updatePayload.Make != null)
            {
                patchOperations.Add(PatchOperation.Set("/make", updatePayload.Make));
            }
            
            if (updatePayload.Model != null)
            {
                patchOperations.Add(PatchOperation.Set("/model", updatePayload.Model));
            }
            
            if (updatePayload.Year != null)
            {
                patchOperations.Add(PatchOperation.Set("/year", updatePayload.Year));
            }
            
            if (updatePayload.ImageUrl != null)
            {
                patchOperations.Add(PatchOperation.Set("/imageUrl", updatePayload.ImageUrl));
            }
            
            return patchOperations;
        }
    }
}
