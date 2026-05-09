using Cars.Models;

namespace Cars.Management;

public interface IAuthManagementProvider
{
    Task RegisterAsync(RegisterRequest request);
    Task<AuthResponse> LoginAsync(string email, string password);
}
