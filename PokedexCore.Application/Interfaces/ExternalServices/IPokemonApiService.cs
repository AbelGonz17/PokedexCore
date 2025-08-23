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
        Task<List<PokemonListResponse>> GetAllPokemonsAsync(int limit , int offset);
        Task<int> GetPokemonTotalCountAsync();
        Task<List<PokemonListResponse>> GetPokemonsByTypeAsync(string type, int limit, int offset);
        Task<ApiResponse<PokemonDetailResponse>> GetPokemonByNameAsync(string name);
        Task<string> GetNextEvolutionAsync(string currentPokemonName);
        Task<int> GetTotalEvolutionsAsync(string currentPokemonName);
        Task<int> GetEvolutionLevelRequirementAsync(string currentPokemonName, string nextEvolutionName);
    }
}
