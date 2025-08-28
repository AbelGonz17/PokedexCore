using PokedexCore.Application.DTOs;
using PokedexCore.Application.DTOs.Paginacion;
using PokedexCore.Application.DTOs.PokemonDtos.RequestPokemon;
using PokedexCore.Application.DTOs.PokemonDtos.ResponsePokemon;

namespace PokedexCore.Application.Interfaces
{
    public interface IPokemonServices
    {
        Task<string> Delete(int id);     
        Task<ApiResponse<PagedResponse<PokemonSummaryResponse>>> GetAllAsync(int page , int pageSize, string? type = null);      
        Task<ApiResponse<PokemonDetailResponse>> GetByNameFromExternalAsync(string name);
        Task<ApiResponse<string>> GetEvolutionInfoAsync(GetEvolutionRequest getEvolutionRequest);    
    }
}