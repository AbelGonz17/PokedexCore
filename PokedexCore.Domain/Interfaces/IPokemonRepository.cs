namespace PokedexCore.Domain.Interfaces
{
    public interface IPokemonRepository<T> where T: class, IEntitiesBase
    {
        Task AddAsync(T entity);
        Task<int> ContarAsync(System.Linq.Expressions.Expression<Func<T, bool>> predicate);
        void Delete(T entity);
        Task<List<T>> GetAllAsync(string includeProperties = "");
        Task<T> GetByAsyncId(int id, string includeProperties = "");
        Task<T> GetByConditionAsync(System.Linq.Expressions.Expression<Func<T, bool>> condition, string includeProperties = "");
        void Update(T entity);
        IQueryable<T> Query();
    }
}
