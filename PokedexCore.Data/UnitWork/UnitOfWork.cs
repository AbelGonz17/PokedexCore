using Microsoft.EntityFrameworkCore.Storage;
using PokedexCore.Data.Contex;
using PokedexCore.Data.Repositories;
using PokedexCore.Domain.Entities;
using PokedexCore.Domain.Interfaces;

namespace PokedexCore.Data.UnitWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly PokedexDbContext _context;
        private IDbContextTransaction _currentTransaction;
        public IPokemonRepository<Pokemon> Pokemon { get; private set; }
        public ITrainerRepository<Trainer> Trainer { get; private set; }
        public ITrainerPokemonRepository TrainerPokemons{ get; private set; }

        public UnitOfWork(PokedexDbContext contex)
        {
            _context = contex;
            Pokemon = new PokemonRepository<Pokemon>(contex);
            Trainer = new TrainerRepository<Trainer>(contex);
            TrainerPokemons = new TrainerPokemonRepository(contex);
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public async Task BeginTransactionAsync()
        {
            if (_currentTransaction == null)
                _currentTransaction = await _context.Database.BeginTransactionAsync();
        }

        public async Task CommitTransactionAsync()
        {
            try
            {
                await _context.SaveChangesAsync();
                await _currentTransaction?.CommitAsync();
            }
            finally
            {
                if (_currentTransaction != null)
                {
                    await _currentTransaction.DisposeAsync();
                    _currentTransaction = null;
                }
            }
        }

        public async Task RollbackTransactionAsync()
        {
            try
            {
                await _currentTransaction?.RollbackAsync();
            }
            finally
            {
                if (_currentTransaction != null)
                {
                    await _currentTransaction.DisposeAsync();
                    _currentTransaction = null;
                }
            }
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
