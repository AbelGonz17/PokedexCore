using PokedexCore.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokedexCore.Domain.Interfaces
{
    public interface IUnitOfWork
    {
        IPokemonRepository<Pokemon> Pokemon { get; }

        ITrainerRepository<Trainer> Trainer { get; }

        Task<int> SaveChangesAsync();
    }
}
