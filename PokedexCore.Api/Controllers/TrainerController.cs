using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PokedexCore.Application.DTOs.TrainerDtos.RequestTrainer;
using PokedexCore.Application.Services;

namespace PokedexCore.Api.Controllers
{
    [ApiController]
    [Route("api/Trainer")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
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

        [HttpPost]
        public async Task<IActionResult> CatchPokemon(CatchPokemonRequest request)
        {
            var result = await trainerService.CatchPokemonAsync(request);
            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }
    }
}
