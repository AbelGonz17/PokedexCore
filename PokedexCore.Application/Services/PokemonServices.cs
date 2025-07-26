using MediatR;
using PokedexCore.Application.DTOs.Pagination;
using PokedexCore.Application.DTOs.PokemonDtos.RequestPokemon;
using PokedexCore.Application.DTOs.PokemonDtos.ResponsePokemon;
using PokedexCore.Application.Interfaces;
using PokedexCore.Application.Interfaces.ExternalServices;
using PokedexCore.Domain.Entities;
using PokedexCore.Domain.Exceptions;
using PokedexCore.Domain.Interfaces;
using System.ComponentModel.Design;

namespace PokedexCore.Application.Services
{
    public class PokemonServices : IPokemonServices
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IMediator eventBus;
        private readonly IPokemonApiService pokemonApiService;

        public PokemonServices(IUnitOfWork unitOfWork, IPokemonApiService pokemonApiService, IMediator eventBus)
        {
            this.unitOfWork = unitOfWork;
            this.eventBus = eventBus;
            pokemonApiService = pokemonApiService;
        }

        public async Task<PaginatedResponse<PokemonListResponse>> GetAllAsync(GetPokemonsRequest request)
        {
            var query = unitOfWork.Pokemon.Query();

            if (request.TrainerId.HasValue)
                query = query.Where(p => p.TrainerId == request.TrainerId);

            if (!string.IsNullOrEmpty(request.MainType))
                query = query.Where(p => p.MainType == request.MainType);

            if (!string.IsNullOrEmpty(request.Region))
                query = query.Where(p => p.Region == request.Region);

            var projection = query.Select(p => new PokemonListResponse
            {
                Id = p.Id,
                Name = p.Name,
                MainType = p.MainType,
                Level = p.Level.ToString()
            });

            return await projection.PaginateAsync(request.Page, request.recordsPerPage);
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

        public async Task<PokemonDetailResponse> GetByIdAsync(int id)
        {
            var pokemon = await unitOfWork.Pokemon.GetByAsyncId(id);
            if (pokemon == null)
                throw new DomainException("Pokémon not found");

            var trainer = await unitOfWork.Trainer.GetByAsyncId(pokemon.TrainerId);
            if (trainer == null)
                throw new DomainException("Trainer not found");

            return new PokemonDetailResponse
            {
                Id = pokemon.Id,
                Name = pokemon.Name,
                MainType = pokemon.MainType,
                Region = pokemon.Region,
                CaptureDate = pokemon.CaptureDate,
                Level = pokemon.Level,
                IsShiny = pokemon.IsShiny,
                Status = pokemon.Status.ToString(),
                Trainer = new DTOs.TrainerDtos.ResponseTrainer.TrainerBasicResponse
                {
                    Id = trainer.Id,
                    UserName = trainer.UserName,
                    Rank = trainer.Rank.ToString(),
                }
            };
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
