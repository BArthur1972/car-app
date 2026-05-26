using Cars.Management;
using Cars.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Cars.Controllers;

[Authorize]
[ApiController]
[Route("cars")]
public class CarController(ICarManagementProvider carProvider)
    : ControllerBase
{
    private readonly ICarManagementProvider carManagementProvider = carProvider;

    [HttpGet("getCars")]
    public async Task<ActionResult> GetCars()
    {
        IEnumerable<CarResponsePayload> cars = await carManagementProvider.GetCars().ConfigureAwait(false);
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
