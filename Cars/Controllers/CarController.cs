using Microsoft.AspNetCore.Mvc;
using Cars.Management;
using Cars.DataAccess.Entities.Resources;
using Cars.DataAccess.Entities;

namespace Cars.Controllers
{
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
            CarResponsePayload? car = await carManagementProvider.GetCar(id).ConfigureAwait(false);
            return Ok(car);
        }

        [HttpPost("addCar")]
        public async Task<ActionResult> AddCar([FromBody] CarRequestPayload car)
        {
            Car newCar = await carManagementProvider.AddCar(car).ConfigureAwait(false);
            return Ok("Successfully added car: " + newCar.ToString());
        }

        [HttpDelete("removeCar/{id}")]
        public async Task<ActionResult> DeleteCar(string id)
        {
            await carManagementProvider.RemoveCar(id).ConfigureAwait(false);
            return Ok("Successfully removed car with id: " + id);
        }

        [HttpPatch("updateCar/{id}")]
        public async Task<ActionResult> UpdateCar(string id, [FromBody] CarUpdatePayload updatePayload)
        {            
            CarResponsePayload updatedCar = await carManagementProvider.UpdateCar(id, updatePayload)
                .ConfigureAwait(false);
            return Ok("Successfully updated car: " + updatedCar.ToString());
        }
    }
}
