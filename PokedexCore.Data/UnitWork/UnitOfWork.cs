using PokedexCore.Data.Contex;
using PokedexCore.Data.Repositories;
using PokedexCore.Domain.Entities;
using PokedexCore.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokedexCore.Data.UnitWork
{
    public class UnitOfWork:IUnitOfWork
    {
        private readonly PokedexDbContext context;

        public IPokemonRepository<Pokemon> Pokemon { get; }
        public ITrainerRepository<Trainer> Trainer { get; }

        public UnitOfWork(PokedexDbContext context)
        {
            this.context = context;
            Pokemon = new PokemonRepository<Pokemon>(context);
            Trainer = new TrainerRepository<Trainer>(context);
        }

        public async Task<int> SaveChangesAsync()
        {
            return await context.SaveChangesAsync();
        }
    }
}

   
