using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.AspNetCore.RateLimiting;
using PokedexCore.Application.DTOs.PokemonDtos.RequestPokemon;
using PokedexCore.Application.Interfaces;

namespace PokedexCore.Api.Controllers
{
    [ApiController]
    [Route("api/Pokemon")]
    public class PokemonController : ControllerBase
    {
        private readonly IPokemonServices pokemonServices;

        public PokemonController(IPokemonServices pokemonServices)
        {
            this.pokemonServices = pokemonServices;
        }

        [HttpGet]
        [OutputCache(PolicyName = "PokemonCache")]
        public async Task<IActionResult> GetAllPokemons([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string? type = null)
        {
            var result = await pokemonServices.GetAllAsync(page, pageSize, type);
            if (!result.Success)
            {
                return NotFound(result);
            }
            return Ok(result);
        }

        [HttpGet("{name}", Name = "GetByName")]
        [OutputCache(PolicyName = "PokemonCacheByName")]
        public async Task<IActionResult> GetPokemonsFromName(string name)
        {
            var response = await pokemonServices.GetByNameFromExternalAsync(name);
            if (!response.Success)
                return NotFound(response);

            return Ok(response);
        }

        [HttpGet("evolution", Name = "GetEvolutionPokemon")]    
        public async Task<IActionResult> GetEvolutionPokemon([FromQuery] GetEvolutionRequest getEvolutionRequest)
        {
            var response = await pokemonServices.GetEvolutionInfoAsync(getEvolutionRequest);
            if (!response.Success)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }
    }
}

