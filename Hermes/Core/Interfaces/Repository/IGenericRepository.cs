namespace Hermes.Core.Interfaces.Repository
{
    public interface IGenericRepository<T> where T : class
    {
        Task<IEnumerable<T?>> GetAllAsync();
        Task<T?> GetByIdAsync(Guid id);
        Task<T?> CreateAsync(T entity);
        Task<T?> UpdateAsync(Guid id, T entity);
        Task DeleteAsync(Guid id);
    }
};