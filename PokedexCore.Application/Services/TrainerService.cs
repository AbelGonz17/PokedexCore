using Microsoft.AspNetCore.Mvc.TagHelpers.Cache;
using Microsoft.Extensions.Logging;
using PokedexCore.Application.DTOs;
using PokedexCore.Application.DTOs.PokemonDtos.ResponsePokemon;
using PokedexCore.Application.DTOs.TrainerDtos.RequestTrainer;
using PokedexCore.Application.DTOs.TrainerDtos.ResponseTrainer;
using PokedexCore.Application.Interfaces;
using PokedexCore.Application.Interfaces.ExternalServices;
using PokedexCore.Data.Securtiry;
using PokedexCore.Data.UnitWork;
using PokedexCore.Domain.Entities;
using PokedexCore.Domain.Enums;
using PokedexCore.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.InteropServices.Marshalling;
using System.Text;
using System.Threading.Tasks;

namespace PokedexCore.Application.Services
{
    public class TrainerService : ITrainerService
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly ICurrentUserHelper currentUser;
        private readonly IPokemonApiService pokemonApiService;
        private readonly ILogger<TrainerService> logger;
        private readonly IConsoleService consoleService;

        public TrainerService(IUnitOfWork unitOfWork,ICurrentUserHelper currentUser,IPokemonApiService pokemonApiService,
            ILogger<TrainerService> logger, IConsoleService consoleService)
        {
            this.unitOfWork = unitOfWork;
            this.currentUser = currentUser;
            this.pokemonApiService = pokemonApiService;
            this.logger = logger;
            this.consoleService = consoleService;
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
            var trainer = await unitOfWork.Trainer
                .GetByAsyncId(id, "Pokemons.Pokemon"); 

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
                Pokemons = trainer.Pokemons.Select(tp => new PokemonListResponse
                {
                    Id = tp.Pokemon.Id,
                    Name = tp.Pokemon.Name,
                    MainType = tp.Pokemon.MainType,
                    Level = tp.Level,
                    Quantity = tp.Quantity
                }).ToList()
            };

            return ApiResponse<TrainerDetailResponse>.Ok(response);
        }


        public async Task<ApiResponse<string>> CatchPokemonAsync(CatchPokemonRequest request)
        {
            try
            {
                // Validaciones iniciales
                var validationResult = await ValidateRequestAsync(request);
                if (!validationResult.IsValid)
                    return ApiResponse<string>.Fail(validationResult.ErrorMessage);

                var trainer = validationResult.Trainer;

                // Simulación de captura
                var captureResult = await SimulatePokemonCaptureAsync(request.Name, trainer.Rank);
                if (!captureResult.Success)
                    return ApiResponse<string>.Fail(captureResult.Message);

                // Captura exitosa - procesar en base de datos
                return await ProcessSuccessfulCaptureAsync(request.Name, trainer);
            }
            catch (Exception ex)
            {
                consoleService.WriteLine($"General error: {ex.Message}");
                return ApiResponse<string>.Fail("An error occurred while trying to catch the Pokemon. Please try again.");
            }
        }

        private async Task<ApiResponse<string>> ProcessSuccessfulCaptureAsync(string pokemonName, Trainer trainer)
        {
            await unitOfWork.BeginTransactionAsync();
            try
            {
                // Obtener datos del Pokémon de la API externa
                var pokemonApiResult = await pokemonApiService.GetPokemonByNameAsync(pokemonName);
                if (!pokemonApiResult.Success || pokemonApiResult.Data == null)
                {
                    await unitOfWork.RollbackTransactionAsync();
                    return ApiResponse<string>.Fail("Pokemon not found in external PokeApi");
                }

                var basePokemon = await unitOfWork.Pokemon.GetByConditionAsync(x => x.Name == pokemonName);
                if (basePokemon == null)
                {
                    // Crear el nuevo Pokémon
                    basePokemon = Pokemon.CreatePokemon(
                        pokemonApiResult.Data.Name,
                        pokemonApiResult.Data.MainType ?? "Unknown",
                        pokemonApiResult.Data.Region ?? "Unknown",
                        DateTime.UtcNow,
                        pokemonApiResult.Data.IsShiny,
                        trainer
                    );
                    // Agregar Pokémon a la base de datos
                    await unitOfWork.Pokemon.AddAsync(basePokemon);
                    await unitOfWork.SaveChangesAsync();
                }

                var trainerPokemon = await unitOfWork.TrainerPokemons
                    .GetByTrainerAndPokemonAsync(trainer.Id, basePokemon.Id);

                if (trainerPokemon != null)
                {
                    trainerPokemon.Quantity++;
                    trainerPokemon.LastCaptureDate = DateTime.UtcNow;
                    await unitOfWork.TrainerPokemons.UpdateAsync(trainerPokemon);
                }
                else
                {
                    trainerPokemon = new TrainerPokemons
                    {
                        TrainerId = trainer.Id,
                        PokemonId = basePokemon.Id,
                        Level = 1,
                        Quantity = 1,
                        IsShiny = pokemonApiResult.Data.IsShiny,
                        LastCaptureDate = DateTime.UtcNow,
                    };
                    await unitOfWork.TrainerPokemons.AddAsync(trainerPokemon);
                }

                trainer.PokemonCount++;
                await unitOfWork.CommitTransactionAsync();

                return ApiResponse<string>.Ok(
                       $"Congratulations! You caught {pokemonApiResult.Data.Name}! " +
                       $"You now have {trainerPokemon.Quantity} of this Pokémon."
                 );
            }
            catch (Exception dbEx)
            {
                await unitOfWork.RollbackTransactionAsync();
                consoleService.WriteLine($"Database error: {dbEx.Message}");
                return ApiResponse<string>.Fail("An error occurred while saving the Pokemon. Please try again.");
            }
        }


        

        private async Task<ValidationResult> ValidateRequestAsync(CatchPokemonRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Name))
                return ValidationResult.Invalid("Pokemon name is required");

            var trainerId = currentUser.GetUserId();
            if (trainerId == 0)
                return ValidationResult.Invalid("Unauthenticated Trainer");

            var trainer = await unitOfWork.Trainer.GetByAsyncId(trainerId);
            if (trainer == null)
                return ValidationResult.Invalid("Trainer not found");

            return ValidationResult.Valid(trainer);
        }

        private async Task<CaptureResult> SimulatePokemonCaptureAsync(string pokemonName, TrainerRank rank)
        {
            var captureMessage = $"Capturing... {pokemonName}";
            using var cts = new CancellationTokenSource();

            var spinnerTask = consoleService.ShowSpinnerAsync(captureMessage, cts.Token);

            // Simular tiempo de captura
            await Task.Delay(500);
            await Task.Delay(Random.Shared.Next(2000, 3500));

            // Calcular probabilidad de captura
            double captureChance = GetCaptureChance(rank) / 100.0; // Convertir a decimal
            double roll = Random.Shared.NextDouble();

            cts.Cancel();
            await spinnerTask;

            if (roll > captureChance)
                return CaptureResult.Failed($"{pokemonName} escaped! Better luck next time.");

            return CaptureResult.CaptureSuccess();
        }

        private double GetCaptureChance(TrainerRank rank)
        {
            return rank switch
            {
                TrainerRank.Rookie => 30.0,        // 30% chance
                TrainerRank.Intermediate => 50.0,  // 50% chance
                TrainerRank.Advanced => 70.0,      // 70% chance
                TrainerRank.Expert => 85.0,        // 85% chance
                TrainerRank.Master => 95.0,        // 95% chance
                _ => 0.0
            };
        }

        private class ValidationResult
        {
            public bool IsValid { get; init; }
            public string ErrorMessage { get; init; }
            public Trainer Trainer { get; init; }

            public static ValidationResult Valid(Trainer trainer) =>
                new() { IsValid = true, Trainer = trainer };

            public static ValidationResult Invalid(string errorMessage) =>
                new() { IsValid = false, ErrorMessage = errorMessage };
        }

        private class CaptureResult
        {
            public bool Success { get; init; }
            public string Message { get; init; }

            public static CaptureResult CaptureSuccess() =>
                new() { Success = true };

            public static CaptureResult Failed(string message) =>
                new() { Success = false, Message = message };
        }
    }
}
