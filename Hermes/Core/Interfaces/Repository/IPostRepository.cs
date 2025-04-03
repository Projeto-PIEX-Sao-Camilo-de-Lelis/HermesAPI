using Hermes.Core.Models;

namespace Hermes.Core.Interfaces.Repository
{
    public interface IPostRepository : IGenericRepository<Post>
    {
        Task<IEnumerable<Post?>> GetByAuthorAsync(string authorName);
    }
}