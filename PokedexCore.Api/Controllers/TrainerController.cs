using Microsoft.AspNetCore.Mvc;
using PokedexCore.Application.Services;

namespace PokedexCore.Api.Controllers
{
    [ApiController]
    [Route("api/Trainer")]
    public class TrainerController : ControllerBase
    {
        private readonly ITrainerService trainerService;

        public TrainerController(ITrainerService trainerService)
        {
            this.trainerService = trainerService;
        }

        [HttpGet("Get all trainers")]
        public async Task<IActionResult> GetAllTraines()
        {
            var result = await trainerService.GetAllAsync();

            if (!result.Success)
            {
                return NotFound(result);
            }
            return Ok(result);
        }

        [HttpGet("{id:int}", Name = "GetTrainerById")]
        public async Task<IActionResult> GetTrainerByAsync(int id)
        {
            var result = await trainerService.GetByIdAsync(id);

            if (!result.Success)
            {
                return NotFound(result);
            }
            return Ok(result);
        }

    }
}
