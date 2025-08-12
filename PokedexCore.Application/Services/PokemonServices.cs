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

                var result = new PokemonDetailResponse
                {
                    Id = data.Data.Id,
                    Name = data.Data.Name,
                    MainType = data.Data.MainType,
                    Region = "Unknown",
                    CaptureDate = DateTime.UtcNow,
                    Level = 1,
                    IsShiny = false,
                    Status = PokemonStatus.Active,
                    Trainer = null
                };

                return ApiResponse<PokemonDetailResponse>.Ok(result);
            }
            catch(DomainException ex)
            {
                return ApiResponse<PokemonDetailResponse>.Fail(ex.Message);
            }
            catch (Exception )
            {
                return ApiResponse<PokemonDetailResponse>.Fail("An unexpected error ocurred while fetching data from PokeAPI.");
            }

        }

        public async Task<PokemonResponse> CreateAsync(CreatePokemonRequest request)
        {
            var pokemonExist = await pokemonApiService.PokemonExistAsync(request.Name);
            if (!pokemonExist)
                throw new DomainException($"Pokemon '{request.Name}' does not exist");

            var trainer = await unitOfWork.Trainer.GetByAsyncId(request.TrainerId);
            if (trainer == null)
                throw new DomainException("Trainer not found");

            // Resto de la lógica...
            var pokemon = new Pokemon(request.Name, request.MainType, request.Region, request.TrainerId);
            await unitOfWork.Pokemon.AddAsync(pokemon);
            await unitOfWork.SaveChangesAsync();

            return MapToResponse(pokemon);
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

        public async Task LevelUpAsync(int pokemonId)
        {
            var pokemon = await unitOfWork.Pokemon.GetByAsyncId(pokemonId);
            if (pokemon == null)
                throw new DomainException("Pokemon not found");

            pokemon.levelUP();

            await unitOfWork.SaveChangesAsync();

            foreach (var domainEvent in pokemon.DomainEvents)
            {
                await eventBus.Publish(domainEvent);
            }

            pokemon.ClearDomainEvents();
        }

        public async Task EvolveAsync(int pokemonId, string evolvedForm)
        {
            var pokemon = await unitOfWork.Pokemon.GetByAsyncId(pokemonId);
            if (pokemon == null)
                throw new DomainException("Pokémon not found");

            var exists = await pokemonApiService.PokemonExistAsync(evolvedForm);
            if (!exists)
                throw new DomainException($"The Pokémon '{evolvedForm}' does not exist in PokéAPI.");

            var isValid = await pokemonApiService.IsValidEvolutionAsync(pokemon.Name, evolvedForm);
            if (!isValid)
                throw new DomainException($"'{evolvedForm}' is not a valid evolution of '{pokemon.Name}'.");

            pokemon.Evolve(evolvedForm, pokemon.MainType);

            await unitOfWork.SaveChangesAsync();

            foreach (var domainEvent in pokemon.DomainEvents)
                await eventBus.Publish(domainEvent);

            pokemon.ClearDomainEvents();
        }

        public async Task CheckIfCanBattleAsync(int pokemonId)
        {
            var pokemon = await unitOfWork.Pokemon.GetByAsyncId(pokemonId);
            if (pokemon == null)
                throw new DomainException("Pokémon not found");

            pokemon.CanBattle();

            await unitOfWork.SaveChangesAsync(); // por si quieres dejar un registro

            foreach (var domainEvent in pokemon.DomainEvents)
                await eventBus.Publish(domainEvent);

            pokemon.ClearDomainEvents();
        }

        private PokemonResponse MapToResponse(Pokemon pokemon)
        {
            return new PokemonResponse
            {
                Id = pokemon.Id,
                Name = pokemon.Name,
                MainType = pokemon.MainType,
                Region = pokemon.Region,
                CaptureDate = pokemon.CaptureDate,
                Level = pokemon.Level,
                IsShiny = pokemon.IsShiny,
                Status = pokemon.Status.ToString()
            };
        }

    }
}
