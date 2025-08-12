using PokedexCore.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokedexCore.Domain.Interfaces
{
    public interface ITrainerPokemonRepository
    {
        Task AddAsync(TrainerPokemons entity);
        Task<TrainerPokemons?> GetByTrainerAndPokemonAsync(int trainerId, int pokemonId);
        Task UpdateAsync(TrainerPokemons entity);
    }
}
