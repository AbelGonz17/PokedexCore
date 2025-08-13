using PokedexCore.Application.DTOs;
using PokedexCore.Application.DTOs.TrainerDtos.RequestTrainer;
using PokedexCore.Application.DTOs.TrainerDtos.ResponseTrainer;

namespace PokedexCore.Application.Interfaces
{
    public interface ITrainerService
    {
        Task<ApiResponse<string>> BattlePokemonAsync(BattlePokemonRequest request);
        Task<ApiResponse<string>> CatchPokemonAsync(CatchPokemonRequest request);
        Task<ApiResponse<List<TrainerBasicResponse>>> GetAllAsync();
        Task<ApiResponse<TrainerDetailResponse>> GetByIdAsync(int id);
        Task<ApiResponse<string>> ReleasePokemonAsync(ReleasePokemonRequest request, int trainerId);
    }
}