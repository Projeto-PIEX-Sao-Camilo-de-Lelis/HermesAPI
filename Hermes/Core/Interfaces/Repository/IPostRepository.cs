using Hermes.Core.Models;

namespace Hermes.Core.Interfaces.Repository
{
    public interface IPostRepository : IGenericRepository<Post>
    {
        Task<(IEnumerable<Post> Posts, int TotalCount)> GetPagedAsync(int pageNumber, int pageSize);
        Task<IEnumerable<Post?>> GetByAuthorAsync(string authorName);
    }
}