using PokedexCore.Application.DTOs;
using PokedexCore.Application.DTOs.PokemonDtos.RequestPokemon;
using PokedexCore.Application.DTOs.PokemonDtos.ResponsePokemon;
using PokedexCore.Application.Interfaces;
using PokedexCore.Application.Interfaces.ExternalServices;
using PokedexCore.Domain.Entities;
using PokedexCore.Domain.Exceptions;
using PokedexCore.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokedexCore.Application.Services
{
    public class PokemonServices : IPokemonServices
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IPokemonApiService pokemonApiService;

        public PokemonServices(IUnitOfWork unitOfWork, IPokemonApiService pokemonApiService)
        {
            this.unitOfWork = unitOfWork;
            pokemonApiService = pokemonApiService;
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
