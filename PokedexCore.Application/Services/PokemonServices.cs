using MediatR;
using PokedexCore.Application.DTOs;
using PokedexCore.Application.DTOs.Paginacion;
using PokedexCore.Application.DTOs.PokemonDtos.RequestPokemon;
using PokedexCore.Application.DTOs.PokemonDtos.ResponsePokemon;
using PokedexCore.Application.Interfaces;
using PokedexCore.Application.Interfaces.ExternalServices;
using PokedexCore.Data.UnitWork;
using PokedexCore.Domain.Entities;
using PokedexCore.Domain.Enums;
using PokedexCore.Domain.Exceptions;
using PokedexCore.Domain.Interfaces;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;

namespace PokedexCore.Application.Services
{
    public class PokemonServices : IPokemonServices
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IMediator eventBus;
        private readonly HttpClient httpClient;
        private readonly IPokemonApiService pokemonApiService;

        public PokemonServices(IUnitOfWork unitOfWork, IPokemonApiService pokemonApiService, IMediator eventBus,HttpClient httpClient)
        {
            this.unitOfWork = unitOfWork;
            this.eventBus = eventBus;
            this.httpClient = httpClient;
            this.pokemonApiService = pokemonApiService;
        }

        public async Task<ApiResponse<PagedResponse<PokemonListResponse>>> GetAllAsync(int page , int pageSize, string? type = null)
        {
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 50) pageSize = 10;

            var offset = (page - 1) * pageSize;

            List<PokemonListResponse> pokemons;
            int totalCount;

            if (!string.IsNullOrWhiteSpace(type))
            {
                // Usar el nuevo método para obtener TODOS los de ese tipo
                pokemons = await pokemonApiService.GetPokemonsByTypeAsync(type, pageSize, offset);

                // Obtener total de pokémon de ese tipo
                var typeListResponse = await httpClient.GetAsync($"https://pokeapi.co/api/v2/type/{type.ToLower()}");
                var json = await typeListResponse.Content.ReadAsStringAsync();
                var typeData = JsonSerializer.Deserialize<PokemonTypeApiResponse>(json, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
                });
                totalCount = typeData.Pokemon.Count;
            }
            else
            {
                pokemons = await pokemonApiService.GetAllPokemonsAsync(pageSize, offset);
                totalCount = await pokemonApiService.GetPokemonTotalCountAsync();
            }

            if (pokemons == null || !pokemons.Any())
                return ApiResponse<PagedResponse<PokemonListResponse>>.Fail("No pokemons found from PokeAPI");

            var paged = new PagedResponse<PokemonListResponse>
            {
                Data = pokemons,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };

            return ApiResponse<PagedResponse<PokemonListResponse>>.Ok(paged);
        }

        public async Task<ApiResponse<PokemonDetailResponse>> GetByNameFromExternalAsync(string name)
        {
            try
            {
                var data = await pokemonApiService.GetPokemonByNameAsync(name);
           
                if (data == null || data.Data == null)
                {
                    return ApiResponse<PokemonDetailResponse>.Fail($"No Pokémon found with the name '{name}'.");
                }
           
                if (!string.Equals(data.Data.Name, name, StringComparison.OrdinalIgnoreCase))
                {
                    return ApiResponse<PokemonDetailResponse>.Fail($"'{name}' is not a valid Pokémon name.");
                }

                var level = await pokemonApiService.GetEvolutionLevelRequirementAsync(data.Data.Name, data.Data.Name);

                var result = new PokemonDetailResponse
                {
                    Id = data.Data.Id,
                    Name = data.Data.Name,
                    MainType = data.Data.MainType,
                    Region = "Unknown",
                    CaptureDate = data.Data.CaptureDate,
                    Level = level,
                    IsShiny = false,
                    Status = PokemonStatus.Active,
                    Trainer = null
                };

                return ApiResponse<PokemonDetailResponse>.Ok(result);
            }
            catch (DomainException ex)
            {
                return ApiResponse<PokemonDetailResponse>.Fail(ex.Message);
            }
            catch (HttpRequestException httpEx) when (httpEx.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                // Si la API externa responde 404
                return ApiResponse<PokemonDetailResponse>.Fail($"No Pokémon found with the name '{name}'.");
            }
            catch (Exception)
            {
                return ApiResponse<PokemonDetailResponse>.Fail("An unexpected error occurred while fetching data from PokeAPI.");
            }
        }

        public async Task<ApiResponse<string>> GetEvolutionInfoAsync(GetEvolutionRequest getEvolutionRequest)
        {
            if (string.IsNullOrWhiteSpace(getEvolutionRequest.PokemonName))
                return ApiResponse<string>.Fail("Pokemon name is required.");

            var nextEvolution = await pokemonApiService.GetNextEvolutionAsync(getEvolutionRequest.PokemonName);
            var totalEvolutions = await pokemonApiService.GetTotalEvolutionsAsync(getEvolutionRequest.PokemonName);

            if (totalEvolutions == 0)
            {
                // Significa que el Pokémon no existe en la PokeAPI
                return ApiResponse<string>.Fail($"The Pokémon '{getEvolutionRequest.PokemonName}' does not exist.");
            }

            if (string.IsNullOrEmpty(nextEvolution))
            {
                // Existe pero no tiene más evoluciones
                return ApiResponse<string>.Ok(
                    $"{getEvolutionRequest.PokemonName} has no further evolutions. Total evolutions in its chain: {totalEvolutions}."
                );
            }

            // Existe y sí tiene siguiente evolución
            return ApiResponse<string>.Ok(
                $"The next evolution of {getEvolutionRequest.PokemonName} is {nextEvolution}. This Pokémon has {totalEvolutions} evolution(s) in total."
            );
        }

        public async Task<string> Delete(int id)
        {
            var pokemon = await unitOfWork.Pokemon.GetByAsyncId(id);
            if (pokemon == null)
                throw new DomainException("Pokémon not found");

            unitOfWork.Pokemon.Delete(pokemon);
            await unitOfWork.SaveChangesAsync();

            return $"Pokémon '{pokemon.Name}' deleted successfully.";
        }
    }
}
