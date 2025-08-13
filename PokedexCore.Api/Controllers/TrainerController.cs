using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PokedexCore.Application.DTOs.TrainerDtos.RequestTrainer;
using PokedexCore.Application.Interfaces;
using PokedexCore.Data.Securtiry;

namespace PokedexCore.Api.Controllers
{
    [ApiController]
    [Route("api/Trainer")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class TrainerController : ControllerBase
    {
        private readonly ITrainerService trainerService;
        private readonly ICurrentUserHelper currentUser;

        public TrainerController(ITrainerService trainerService,ICurrentUserHelper currentUser)
        {
            this.trainerService = trainerService;
            this.currentUser = currentUser;
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

        [HttpPost("Catch")]
        public async Task<IActionResult> CatchPokemon(CatchPokemonRequest request)
        {
            var result = await trainerService.CatchPokemonAsync(request);
            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        [HttpPost("release")]
        public async Task<IActionResult> ReleasePokemon([FromBody] ReleasePokemonRequest request)
        {
            int trainerId;
            try
            {
                trainerId = currentUser.GetUserId();
            }
            catch (Exception)
            {
                return Unauthorized("User not authenticated.");
            }

            var result = await trainerService.ReleasePokemonAsync(request, trainerId);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpPost("battle")]
        public async Task<IActionResult> BattlePokemon([FromBody] BattlePokemonRequest request)
        {
            var result = await trainerService.BattlePokemonAsync(request);

            if (result.Success)
                return Ok(result);

            return BadRequest(result);
        }
    }
}
