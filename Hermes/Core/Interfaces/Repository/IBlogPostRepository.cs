using Hermes.Core.Models;

namespace Hermes.Core.Interfaces.Repository
{
    public interface IBlogPostRepository : IGenericRepository<BlogPost>
    {
        Task<(IEnumerable<BlogPost> Posts, int TotalCount)> GetPagedAsync(int pageNumber, int pageSize);
        Task<IEnumerable<BlogPost?>> GetByAuthorAsync(string authorName);
        Task<BlogPost> GetBySlugAsync(string slug);
    }
}