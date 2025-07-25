using PokedexCore.Application.DTOs.PokemonDtos.RequestPokemon;
using PokedexCore.Application.DTOs.PokemonDtos.ResponsePokemon;

namespace PokedexCore.Application.Services
{
    public interface IPokemonServices
    {
        Task<PokemonResponse> CreateAsync(CreatePokemonRequest request);
        Task<string> Delete(int id);
        Task<PokemonDetailResponse> GetByIdAsync(int id);
    }
}