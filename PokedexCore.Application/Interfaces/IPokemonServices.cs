using PokedexCore.Application.DTOs.Pagination;
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
        Task<PaginatedResponse<PokemonListResponse>> GetAllAsync(GetPokemonsRequest request);
        Task<PokemonDetailResponse> GetByIdAsync(int id);
        Task LevelUpAsync(int pokemonId);
    }
}