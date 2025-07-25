using PokedexCore.Application.DTOs;
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
    }
}
