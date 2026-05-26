using System.Net;
using Cars.ApiCommon.Cosmos;
using Cars.ApiCommon.Exceptions;
using Microsoft.Azure.Cosmos;
using User = Cars.DataAccess.Entities.User;

namespace Cars.DataAccess;

public class UserDataProvider(
    [FromKeyedServices(CosmosContainerConstants.UsersContainer)] Container container,
    ILogger<UserDataProvider> logger)
    : IUserDataProvider
{
    public async Task<User> GetUserByIdAsync(string userId)
    {
        try
        {
            ItemResponse<User> response =
                await container.ReadItemAsync<User>(userId, new PartitionKey(userId));

            return response.Resource;
        }
        catch (CosmosException e) when (e.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            throw new DataNotFoundException(message: "User not found");
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to get user with Id: {Id}", userId);
            throw;
        }
    }

    public async Task<User> GetUserByEmailAsync(string email)
    {
        try
        {
            var queryDefinition = new QueryDefinition(
                "SELECT * FROM c WHERE c.email = @email")
                .WithParameter("@email", email);

            var query = container.GetItemQueryIterator<User>(queryDefinition);

            while (query.HasMoreResults)
            {
                var response = await query.ReadNextAsync();
                var user = response.FirstOrDefault();
                if (user is not null)
                    return user;
            }

            throw new DataNotFoundException(message: "User not found");
        }
        catch (DataNotFoundException)
        {
            throw;
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to get user with email: {Email}", email);
            throw;
        }
    }

    public async Task CreateUserAsync(User user)
    {
        try
        {
            await container.CreateItemAsync(user, new PartitionKey(user.Id));
        }
        catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.Conflict)
        {
            throw new ConflictException(message: "A user with this email already exists");
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to create user");
            throw;
        }
    }

    public async Task UpdateUserAsync(User user)
    {
        try
        {
            await container.ReplaceItemAsync(user, user.Id, new PartitionKey(user.Id));
        }
        catch (CosmosException e) when (e.StatusCode == HttpStatusCode.NotFound)
        {
            throw new DataNotFoundException(message: "User not found");
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to update user with Id: {Id}", user.Id);
            throw;
        }
    }

    public async Task DeleteUserAsync(string userId)
    {
        try
        {
            await container.DeleteItemAsync<User>(userId, new PartitionKey(userId));
        }
        catch (CosmosException e) when (e.StatusCode == HttpStatusCode.NotFound)
        {
            throw new DataNotFoundException(message: "User not found");
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to delete user with Id: {Id}", userId);
            throw;
        }
    }
}
