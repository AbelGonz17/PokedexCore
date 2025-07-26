using Microsoft.EntityFrameworkCore;
using PokedexCore.Data.Contex;
using PokedexCore.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace PokedexCore.Data.Repositories
{
    public class PokemonRepository<T> : IPokemonRepository<T> where T : class, IEntitiesBase
    {
        private readonly PokedexDbContext _context;
        private readonly DbSet<T> _dbSet;

        public PokemonRepository(PokedexDbContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }
   

        public async Task<T> GetByAsyncId(int id, string includeProperties = "")
        {
            IQueryable<T> query = _dbSet;

            if (!string.IsNullOrEmpty(includeProperties))
            {
                foreach (var includeProperty in includeProperties.Split(','))
                {
                    query = query.Include(includeProperty.Trim());
                }
            }

            return await query.FirstOrDefaultAsync(r => r.Id!.Equals(id));
        }

        public async Task<int> ContarAsync(Expression<Func<T, bool>> predicate) //contar entidades
        {
            return await _dbSet.CountAsync(predicate);
        }

        public async Task<T> GetByConditionAsync(Expression<Func<T, bool>> condition, string includeProperties = "") //busca la entidad
        {
            IQueryable<T> query = _dbSet;

            if (!string.IsNullOrEmpty(includeProperties))
            {
                foreach (var includeProperty in includeProperties.Split(','))
                {
                    query = query.Include(includeProperty.Trim());
                }
            }

            return await query.FirstOrDefaultAsync(condition);
        }

        public async Task AddAsync(T entity)
        {
            await _dbSet.AddAsync(entity);
        }

        public void Update(T entity)
        {
            _dbSet.Update(entity);
        }

        public void Delete(T entity)
        {
            _dbSet.Remove(entity);
        }

        public IQueryable<T> Query()
        {
            return _dbSet.AsQueryable();
        }

        public async Task<List<T>> GetAllAsync(string includeProperties = "")
        {
            IQueryable<T> query = _dbSet;

            if (!string.IsNullOrWhiteSpace(includeProperties))
            {
                foreach (var includeProperty in includeProperties.Split(','))
                {
                    query = query.Include(includeProperty.Trim());
                }
            }
            return await query.ToListAsync();
        }
    }

}
 

