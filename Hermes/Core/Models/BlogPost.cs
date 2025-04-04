using System.ComponentModel.DataAnnotations.Schema;

namespace Hermes.Core.Models
{
    public class BlogPost
    {
        [Column("id")]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Column("title")]
        public string Title { get; set; } = string.Empty;

        [Column("content")]
        public string Content { get; set; } = string.Empty;

        [Column("content_preview")]
        public string ContentPreview { get; set; } = string.Empty;

        [Column("author_id")]
        public Guid AuthorId { get; set; } = Guid.NewGuid();
        public string Author { get; set; } = string.Empty;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime? UpdatedAt { get; set; }

        [Column("slug")]
        public string? Slug { get; set; } = string.Empty;

        [Column("is_published")]
        public bool IsPublished { get; set; } = true;
    }
}