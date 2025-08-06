using PokedexCore.Application.DTOs;
using PokedexCore.Application.DTOs.TrainerDtos.ResponseTrainer;

namespace PokedexCore.Application.Services
{
    public interface ITrainerService
    {
        Task<ApiResponse<List<TrainerBasicResponse>>> GetAllAsync();
        Task<ApiResponse<TrainerDetailResponse>> GetByIdAsync(int id);
    }
}