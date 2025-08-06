using PokedexCore.Application.DTOs;
using PokedexCore.Application.DTOs.Paginacion;
using PokedexCore.Application.DTOs.PokemonDtos.RequestPokemon;
using PokedexCore.Application.DTOs.PokemonDtos.ResponsePokemon;

namespace PokedexCore.Application.Interfaces
{
    public interface IPokemonServices
    {
        Task CheckIfCanBattleAsync(int pokemonId);
        Task<PokemonResponse> CreateAsync(CreatePokemonRequest request);
        Task<string> Delete(int id);
        Task EvolveAsync(int pokemonId, string evolvedForm);  
        Task<ApiResponse<PagedResponse<PokemonListResponse>>> GetAllAsync(int page , int pageSize, string? type = null);      
        Task<ApiResponse<PokemonDetailResponse>> GetByNameFromExternalAsync(string name);
        Task LevelUpAsync(int pokemonId);
    }
}