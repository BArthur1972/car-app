using Cars.DataAccess.Entities;

namespace Cars.ApiCommon.Auth;

public interface IJwtTokenService
{
    string GenerateToken(User user);
}
