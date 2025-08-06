using PokedexCore.Application.DTOs;
using PokedexCore.Application.DTOs.PokemonDtos.ResponsePokemon;
using PokedexCore.Application.DTOs.TrainerDtos.ResponseTrainer;
using PokedexCore.Domain.Entities;
using PokedexCore.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokedexCore.Application.Services
{
    public class TrainerService : ITrainerService
    {
        private readonly IUnitOfWork unitOfWork;

        public TrainerService(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }

        public async Task<ApiResponse<List<TrainerBasicResponse>>> GetAllAsync()
        {
            var trainers = await unitOfWork.Trainer.GetAllAsync();
            if (trainers == null || !trainers.Any())
            {
                return ApiResponse<List<TrainerBasicResponse>>.Fail("Trainers not found");
            }

            var response = trainers.Select(t => new TrainerBasicResponse
            {
                Id = t.Id,
                UserName = t.UserName,
                Rank = t.Rank.ToString(),
                PokemonCount = t.PokemonCount,
                Email = t.Email

            }).ToList();

            return ApiResponse<List<TrainerBasicResponse>>.Ok(response);
        }

        public async Task<ApiResponse<TrainerDetailResponse>> GetByIdAsync(int id)
        {
            var trainer = await unitOfWork.Trainer.GetByAsyncId(id, "Pokemons");
            if (trainer == null)
            {
                return ApiResponse<TrainerDetailResponse>.Fail("Trainer not Found");
            }

            var response = new TrainerDetailResponse
            {
                Id = trainer.Id,
                UserName = trainer.UserName,
                Email = trainer.Email,
                RegistrationDate = trainer.RegistrationDate,
                Rank = trainer.Rank.ToString(),
                PokemonCount = trainer.PokemonCount,
                Pokemons = trainer.Pokemons.Select(p => new DTOs.PokemonDtos.ResponsePokemon.PokemonListResponse
                {
                    Id = p.Id,
                    Name = p.Name,
                    MainType = p.MainType,
                    Level = p.Level
                }).ToList() ?? new List<PokemonListResponse>()
            };

            return ApiResponse<TrainerDetailResponse>.Ok(response);
        }
    }
}
