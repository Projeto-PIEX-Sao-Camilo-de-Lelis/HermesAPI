using Hermes.Core.Dtos.Requests;
using Hermes.Core.Dtos.Responses;
using Hermes.Core.Models;
using Microsoft.Extensions.Hosting;

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
                Slug = post.Slug,
                Title = post.Title,
                Content = post.Content,
                ContentPreview = post.ContentPreview,
                Author = post.Author,
                PublishedAt = post.PublishedAt,
                UpdatedAt = post.UpdatedAt,
                IsPublished = post.IsPublished
            };
        }

        public static BlogPostSimplifiedResponseDto ToSimplifiedResponseDto(this BlogPost post)
        {
            return new BlogPostSimplifiedResponseDto
            {
                Id = post.Id,
                Slug = post.Slug,
                Title = post.Title,
                ContentPreview = post.ContentPreview,
                Author = post.Author,
                PublishedAt = post.PublishedAt,
                UpdatedAt = post.UpdatedAt
            };
        }

        public static IEnumerable<BlogPostResponseDto> ToResponseDto(this IEnumerable<BlogPost> posts)
        {
            return posts.Select(post => post.ToResponseDto());
        }

        public static IEnumerable<BlogPostSimplifiedResponseDto> ToSimplifiedResponseDto(this IEnumerable<BlogPost> posts)
        {
            return posts.Select(post => post.ToSimplifiedResponseDto());
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