using Cars.DataAccess.Entities;

namespace Cars.DataAccess;

public interface IUserDataProvider
{
    Task<User> GetUserByIdAsync(string userId);
    Task<User> GetUserByEmailAsync(string email);
    Task CreateUserAsync(User user);
    Task UpdateUserAsync(User user);
    Task DeleteUserAsync(string userId);
}
