using Cars.Management;
using Cars.Models;
using Microsoft.AspNetCore.Mvc;

namespace Cars.Controllers;

[ApiController]
[Route("auth")]
public class AuthController(IAuthManagementProvider authProvider, ILogger<AuthController> logger)
    : ControllerBase
{
    private readonly IAuthManagementProvider authProvider = authProvider;
    private readonly ILogger<AuthController> logger = logger;

    [HttpPost("register")]
    public async Task<ActionResult> Register([FromBody] RegisterRequest request)
    {
        await authProvider.RegisterAsync(request).ConfigureAwait(false);

        logger.LogInformation("User registered: {Email}", request.Email);
        return StatusCode(StatusCodes.Status201Created);
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request)
    {
        AuthResponse response = await authProvider.LoginAsync(request.Email, request.Password)
            .ConfigureAwait(false);
        
        logger.LogInformation("User logged in: {Email}", request.Email);
        return Ok(response);
    }
}
