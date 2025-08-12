using PokedexCore.Domain.Entities;
using PokedexCore.Domain.Interfaces;

namespace PokedexCore.Data.UnitWork
{
    public interface IUnitOfWork:IDisposable
    {
        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();

        IPokemonRepository<Pokemon> Pokemon { get; }
        ITrainerRepository<Trainer> Trainer { get; }
        ITrainerPokemonRepository TrainerPokemons { get; }

        Task<int> SaveChangesAsync();
    }
}
