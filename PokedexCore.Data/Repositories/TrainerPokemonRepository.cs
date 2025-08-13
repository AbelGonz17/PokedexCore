using Microsoft.EntityFrameworkCore;
using PokedexCore.Data.Contex;
using PokedexCore.Domain.Entities;
using PokedexCore.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace PokedexCore.Data.Repositories
{
    public class TrainerPokemonRepository : ITrainerPokemonRepository 
    {
        private readonly PokedexDbContext _context;
        

        public TrainerPokemonRepository(PokedexDbContext context)
        {
            _context = context;
        }

        public async Task<TrainerPokemons?> GetByTrainerAndPokemonAsync(int trainerId, int pokemonId)
        {
            return await _context.TrainerPokemons
                .FirstOrDefaultAsync(tp => tp.TrainerId == trainerId && tp.PokemonId == pokemonId);
        }

        public async Task AddAsync(TrainerPokemons entity)
        {
            await _context.TrainerPokemons.AddAsync(entity);
        }

        public async Task UpdateAsync(TrainerPokemons entity)
        {
            _context.TrainerPokemons.Update(entity);
        }

        public async Task DeleteAsync(TrainerPokemons entity)
        {
            _context.TrainerPokemons.Remove(entity);
            await Task.CompletedTask;
        }
    }
}

