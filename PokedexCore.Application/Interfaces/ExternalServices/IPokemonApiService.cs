using PokedexCore.Application.DTOs;
using PokedexCore.Application.DTOs.PokemonDtos.ResponsePokemon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokedexCore.Application.Interfaces.ExternalServices
{
    public interface IPokemonApiService
    {
        Task<bool> PokemonExistAsync(string pokemonName);
        Task<PokemonApiData> GetPokemonDataAsync(string PokemonName);
        Task<bool> IsValidEvolutionAsync(string currentName, string evolvedFormName);
        Task<List<PokemonListResponse>> GetAllPokemonsAsync(int limit , int offset);
        Task<int> GetPokemonTotalCountAsync();
        Task<List<PokemonListResponse>> GetPokemonsByTypeAsync(string type, int limit, int offset);
        Task<PokemonDetailResponse> GetPokemonByNameAsync(string name);
    }
}
