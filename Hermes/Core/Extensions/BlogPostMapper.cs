using Hermes.Core.Dtos.Requests;
using Hermes.Core.Dtos.Responses;
using Hermes.Core.Models;

namespace Hermes.Core.Extensions
{
    public static class BlogPostMapper
    {
        public static BlogPost ToEntity(this BlogPostCreateRequestDto dto)
        {
            return new BlogPost
            {
                Title = dto.Title,
                Content = dto.Content
            };
        }

        public static BlogPostResponseDto ToResponseDto(this BlogPost post)
        {
            return new BlogPostResponseDto
            {
                Id = post.Id,
                Title = post.Title,
                Content = post.Content,
                ContentPreview = post.ContentPreview,
                Author = post.Author,
                PublishedAt = post.PublishedAt,
                UpdatedAt = post.UpdatedAt,
                Slug = post.Slug,
                IsPublished = post.IsPublished
            };
        }

        public static IEnumerable<BlogPostResponseDto> ToResponseDto(this IEnumerable<BlogPost> posts)
        {
            return posts.Select(post => post.ToResponseDto());
        }

        public static void UpdateEntity(this BlogPost existingPost, BlogPostUpdateRequestDto dto)
        {
            if (dto.Title != null)
            {
                existingPost.Title = dto.Title;
            }

            if (dto.Content != null)
            {
                existingPost.Content = dto.Content;
            }
        }
    }
}