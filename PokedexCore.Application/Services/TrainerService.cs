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
using System.Text.Json;
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
                    Quantity = tp.Quantity,
                    Exp = tp.Experience,
                    SpriteUrl = tp.Pokemon.SpriteURL,
                    Region = tp.Pokemon.Region     
                }).ToList()
            };

            return ApiResponse<TrainerDetailResponse>.Ok(response);
        }

        public async Task<ApiResponse<string>> CatchPokemonAsync(CatchPokemonRequest request)
        {
            try
            {
                // 1. Validaciones iniciales
                var validationResult = await ValidateRequestAsync(request);
                if (!validationResult.IsValid)
                    return ApiResponse<string>.Fail(validationResult.ErrorMessage);

                var trainer = validationResult.Trainer;

                // 2. Simulación de captura
                var captureResult = await SimulatePokemonCaptureAsync(request.Name, trainer.Rank);
                if (!captureResult.Success)
                    return ApiResponse<string>.Fail(captureResult.Message);

                // 3. Determinar nivel inicial según evolución
                int initialLevel = await GetInitialLevelForPokemonAsync(request.Name);

                // 4. Procesar captura en base de datos
                return await ProcessSuccessfulCaptureAsync(request.Name, trainer, initialLevel);
            }
            catch (Exception ex)
            {
                consoleService.WriteLine($"General error: {ex.Message}");
                return ApiResponse<string>.Fail("An error occurred while trying to catch the Pokemon. Please try again.");
            }
        }

        public async Task<ApiResponse<string>> ReleasePokemonAsync(ReleasePokemonRequest request, int trainerId)
        {
            // Validación básica
            if (request == null || string.IsNullOrWhiteSpace(request.Name))
                return ApiResponse<string>.Fail("Pokemon name is required");

            if (request.Quantity <= 0)
                return ApiResponse<string>.Fail("Quantity must be at least 1");

            // Inicio transacción
            await unitOfWork.BeginTransactionAsync();

            try
            {
                var trainer = await unitOfWork.Trainer.GetByAsyncId(trainerId);
                if (trainer == null)
                {
                    await unitOfWork.RollbackTransactionAsync();
                    return ApiResponse<string>.Fail("Trainer not found");
                }

                var basePokemon = await unitOfWork.Pokemon.GetByConditionAsync(p => p.Name == request.Name);
                if (basePokemon == null)
                {
                    await unitOfWork.RollbackTransactionAsync();
                    return ApiResponse<string>.Fail("This Pokémon species does not exist.");
                }

                var trainerPokemon = await unitOfWork.TrainerPokemons
                    .GetByTrainerAndPokemonAsync(trainerId, basePokemon.Id);

                if (trainerPokemon == null)
                {
                    await unitOfWork.RollbackTransactionAsync();
                    return ApiResponse<string>.Fail("You don't have that Pokémon.");
                }

                if (request.Quantity > trainerPokemon.Quantity)
                {
                    await unitOfWork.RollbackTransactionAsync();
                    return ApiResponse<string>.Fail($"You only have {trainerPokemon.Quantity} {request.Name}(s). You cannot release {request.Quantity}.");
                }

                // Lógica para liberar
                if (trainerPokemon.Quantity > request.Quantity)
                {
                    trainerPokemon.Quantity -= request.Quantity;
                    await unitOfWork.TrainerPokemons.UpdateAsync(trainerPokemon);
                }
                else
                {
                    await unitOfWork.TrainerPokemons.DeleteAsync(trainerPokemon);
                }

                // Actualizar contador en trainer
                trainer.PokemonCount -= Math.Min(trainerPokemon.Quantity, request.Quantity);

                await unitOfWork.SaveChangesAsync();
                await unitOfWork.CommitTransactionAsync();

                return ApiResponse<string>.Ok($"Released {request.Quantity} {request.Name}(s) successfully.");
            }
            catch (Exception ex)
            {
                await unitOfWork.RollbackTransactionAsync();
                return ApiResponse<string>.Fail("An error occurred while releasing the Pokémon.");
            }
        }

        public async Task<ApiResponse<string>> BattlePokemonAsync(BattlePokemonRequest request)
        {
            try
            {
                // Validaciones iniciales
                var validationResult = await ValidateBattleRequestAsync(request);
                if (!validationResult.IsValid)
                    return ApiResponse<string>.Fail(validationResult.ErrorMessage);

                var trainer = validationResult.Trainer;
                var myPokemon = validationResult.MyPokemon;

                // Obtener un Pokémon random como oponente
                var opponent = await GetRandomOpponentAsync();
                if (opponent == null)
                    return ApiResponse<string>.Fail("No opponents available for battle");

                // Simulación de batalla
                var battleResult = await SimulateBattleAsync(myPokemon, opponent);

                // Procesar resultado de batalla
                return await ProcessBattleResultAsync(myPokemon, opponent, battleResult);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error during Pokemon battle simulation");
                return ApiResponse<string>.Fail("An error occurred during the battle. Please try again.");
            }
        }

        private async Task<BattleValidationResult> ValidateBattleRequestAsync(BattlePokemonRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.PokemonName))
                return BattleValidationResult.Invalid("Pokemon name is required");

            var trainerId = currentUser.GetUserId();
            if (trainerId == 0)
                return BattleValidationResult.Invalid("Unauthenticated Trainer");

            var trainer = await unitOfWork.Trainer.GetByAsyncId(trainerId);
            if (trainer == null)
                return BattleValidationResult.Invalid("Trainer not found");

            var basePokemon = await pokemonApiService.GetPokemonDataAsync(request.PokemonName);
            if (basePokemon == null)
                return BattleValidationResult.Invalid("This Pokémon species does not exist");

            var myPokemon = await unitOfWork.TrainerPokemons
                .GetByTrainerAndPokemonAsync(trainerId, basePokemon.Id);

            if (myPokemon == null)
                return BattleValidationResult.Invalid("You don't have this Pokémon in your collection");

            return BattleValidationResult.Valid(trainer, myPokemon);
        }

        private async Task<Pokemon> GetRandomOpponentAsync()
        {
            try
            {
                // Obtener todos los Pokémon de la base de datos
                var allPokemons = await unitOfWork.Pokemon.GetAllAsync();
                if (allPokemons == null || !allPokemons.Any())
                    return null;

                // Seleccionar uno random
                var randomIndex = Random.Shared.Next(0, allPokemons.Count());
                return allPokemons.ElementAt(randomIndex);
            }
            catch
            {
                return null;
            }
        }

        private async Task<BattleResult> SimulateBattleAsync(TrainerPokemons myPokemon, Pokemon opponent)
        {
            var battleMessage = $"🥊 {myPokemon.Pokemon.Name} vs {opponent.Name}";
            using var cts = new CancellationTokenSource();

            var spinnerTask = consoleService.ShowSpinnerAsync(battleMessage, cts.Token);

            // Simular tiempo de batalla
            await Task.Delay(1000);
            await Task.Delay(Random.Shared.Next(2000, 4000));

            // Calcular probabilidad de victoria basada en level
            double winChance = CalculateWinChance(myPokemon.Level);
            double roll = Random.Shared.NextDouble();

            cts.Cancel();
            await spinnerTask;

            bool victory = roll <= winChance;
            int experienceGained = victory ? Random.Shared.Next(50, 101) : Random.Shared.Next(10, 31);

            return new BattleResult
            {
                Victory = victory,
                ExperienceGained = experienceGained,
                OpponentName = opponent.Name
            };
        }

        private double CalculateWinChance(int pokemonLevel)
        {
            // Base chance of 50%, modified by level
            double baseChance = 0.5;
            double levelBonus = pokemonLevel * 0.02; // 2% bonus per level
            double finalChance = Math.Min(0.85, baseChance + levelBonus); // Max 85% chance

            return finalChance;
        }

        private async Task<ApiResponse<string>> ProcessBattleResultAsync(TrainerPokemons myPokemon, Pokemon opponent, BattleResult battleResult)
        {
            await unitOfWork.BeginTransactionAsync();
            try
            {
                // Guardar level anterior para comparar
                int previousLevel = myPokemon.Level;

                // Actualizar experiencia del Pokémon
                myPokemon.Experience += battleResult.ExperienceGained;

                // Verificar si debe subir de nivel
                int newLevel = CalculateLevelFromExperience(myPokemon.Experience);
                bool leveledUp = newLevel > previousLevel;

                if (leveledUp)
                {
                    myPokemon.Level = newLevel;
                }

                await unitOfWork.TrainerPokemons.UpdateAsync(myPokemon);
                await unitOfWork.SaveChangesAsync();
                await unitOfWork.CommitTransactionAsync();

                // Construir mensaje con información de subida de nivel
                string resultMessage = battleResult.Victory
                    ? $"🎉 Victory! {myPokemon.Pokemon.Name} defeated {battleResult.OpponentName}!"
                    : $"😞 Defeat! {battleResult.OpponentName} defeated {myPokemon.Pokemon.Name}.";

                resultMessage += $"\n💫 {myPokemon.Pokemon.Name} gained {battleResult.ExperienceGained} experience points!";

                if (leveledUp)
                {
                    int levelsGained = newLevel - previousLevel;
                    resultMessage += $"\n🆙 LEVEL UP! {myPokemon.Pokemon.Name} reached level {newLevel}!";
                    if (levelsGained > 1)
                    {
                        resultMessage += $" (+{levelsGained} levels!)";
                    }
                }

                resultMessage += $"\n📊 Total experience: {myPokemon.Experience} | Level: {myPokemon.Level}";

                // Mostrar experiencia necesaria para el próximo nivel
                int expForNextLevel = GetExperienceForLevel(myPokemon.Level + 1);
                int expNeeded = expForNextLevel - myPokemon.Experience;
                if (expNeeded > 0)
                {
                    resultMessage += $"\n📈 Next level in: {expNeeded} exp";
                }

                return ApiResponse<string>.Ok(resultMessage);
            }
            catch (Exception ex)
            {
                await unitOfWork.RollbackTransactionAsync();
                logger.LogError(ex, "Error processing battle result");
                return ApiResponse<string>.Fail("An error occurred while processing the battle result");
            }
        }

        private async Task<ApiResponse<string>> ProcessSuccessfulCaptureAsync(string pokemonName, Trainer trainer, int initialLevel)
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
                        pokemonApiResult.Data.CaptureDate,
                        pokemonApiResult.Data.IsShiny,
                        trainer,
                        pokemonApiResult.Data.Level,
                        pokemonApiResult.Data.SpriteURL                   
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
                        Level = initialLevel,
                        Quantity = 1,
                        IsShiny = pokemonApiResult.Data.IsShiny,
                        LastCaptureDate = basePokemon.CaptureDate,
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

            var basePokemon = await pokemonApiService.GetPokemonDataAsync(request.Name.ToLower());
            if (basePokemon == null)
            {                
                return ValidationResult.Invalid("This Pokémon species does not exist.");
            }

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

        private int GetExperienceForLevel(int level)
        {
            if (level <= 1) return 0;
            if (level == 2) return 100;
            if (level == 3) return 250;
            if (level == 4) return 450;
            if (level == 5) return 700;
            if (level == 6) return 1000;
            if (level == 7) return 1350;
            if (level == 8) return 1750;
            if (level == 9) return 2200;
            if (level == 10) return 2700;
            if (level == 11) return 3250;

            // Fórmula para niveles superiores: más experiencia requerida por nivel
            return (level * level * 50) - 50;
        }

        private async Task<int> GetInitialLevelForPokemonAsync(string pokemonName)
        {
            // Usa el método que ya existe para determinar la etapa y asignar nivel
            int initialLevel = await pokemonApiService.GetEvolutionLevelRequirementAsync(pokemonName, null);

            // Por si acaso devuelve algo inválido, aseguramos mínimo 0
            return initialLevel < 0 ? 0 : initialLevel;
        }

        private int CalculateLevelFromExperience(int experience)
        {
            // Sistema de niveles: cada nivel requiere más experiencia
            // Nivel 1: 0 exp
            // Nivel 2: 100 exp
            // Nivel 3: 250 exp (150 adicionales)
            // Nivel 4: 450 exp (200 adicionales)
            // Nivel 5: 700 exp (250 adicionales)
            // Y así sucesivamente...

            if (experience < 100) return 1;
            if (experience < 250) return 2;
            if (experience < 450) return 3;
            if (experience < 700) return 4;
            if (experience < 1000) return 5;
            if (experience < 1350) return 6;
            if (experience < 1750) return 7;
            if (experience < 2200) return 8;
            if (experience < 2700) return 9;
            if (experience < 3250) return 10;
        
            int level = 10;
            while (GetExperienceForLevel(level + 1) <= experience)
            {
                level++;
                if (level > 100) break;
            }

            return level;
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

        private class BattleValidationResult
        {
            public bool IsValid { get; init; }
            public string ErrorMessage { get; init; }
            public Trainer Trainer { get; init; }
            public TrainerPokemons MyPokemon { get; init; }

            public static BattleValidationResult Valid(Trainer trainer, TrainerPokemons myPokemon) =>
                new() { IsValid = true, Trainer = trainer, MyPokemon = myPokemon };

            public static BattleValidationResult Invalid(string errorMessage) =>
                new() { IsValid = false, ErrorMessage = errorMessage };
        }

        private class BattleResult
        {
            public bool Victory { get; init; }
            public int ExperienceGained { get; init; }
            public string OpponentName { get; init; }
        }
    }
}
