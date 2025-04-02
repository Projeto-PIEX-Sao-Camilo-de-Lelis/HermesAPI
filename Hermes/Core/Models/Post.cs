using System.ComponentModel.DataAnnotations.Schema;

namespace Hermes.Core.Models
{
    public class Post
    {
        [Column("id")]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Column("title")]
        public string Title { get; set; } = string.Empty;

        [Column("content")]
        public string Content { get; set; } = string.Empty;

        [Column("author_id")]
        public Guid AuthorId { get; set; } = Guid.NewGuid();

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime? UpdatedAt { get; set; }

        [Column("is_published")]
        public bool IsPublished { get; set; } = true;
    }
}