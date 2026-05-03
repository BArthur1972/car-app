using Microsoft.AspNetCore.Mvc;
using Cars.Management;
using Cars.Models;

namespace Cars.Controllers;

[ApiController]
[Route("cars")]
public class CarController(ILogger<CarController> logger, ICarManagementProvider carProvider)
    : ControllerBase
{
    private readonly ILogger<CarController> logger = logger;
    private readonly ICarManagementProvider carManagementProvider = carProvider;

    [HttpGet("getCars")]
    public async Task<ActionResult> GetCars()
    {
        IEnumerable<CarResponsePayload> cars = await carManagementProvider.GetCars().ConfigureAwait(false);
        logger.LogInformation("Cars obtained: {Count} cars", cars.Count());
        return Ok(cars);
    }

    [HttpGet("getCar/{id}")]
    public async Task<ActionResult> GetCar(string id)
    {
        CarResponsePayload car = await carManagementProvider.GetCar(id).ConfigureAwait(false);
        return Ok(car);
    }

    [HttpPost("addCar")]
    public async Task<ActionResult> AddCar([FromBody] CarRequestPayload car)
    {
        CarResponsePayload newCar = await carManagementProvider.AddCar(car).ConfigureAwait(false);
        return CreatedAtAction(nameof(GetCar), new { id = newCar.Id }, newCar);
    }

    [HttpDelete("removeCar/{id}")]
    public async Task<ActionResult> DeleteCar(string id)
    {
        await carManagementProvider.RemoveCar(id).ConfigureAwait(false);
        return NoContent();
    }

    [HttpPatch("updateCar/{id}")]
    public async Task<ActionResult> UpdateCar(string id, [FromBody] CarUpdatePayload updatePayload)
    {
        CarResponsePayload updatedCar = await carManagementProvider.UpdateCar(id, updatePayload)
            .ConfigureAwait(false);
        return Ok(updatedCar);
    }
}
