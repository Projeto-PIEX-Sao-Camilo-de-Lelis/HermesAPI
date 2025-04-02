﻿using Hermes.Core.Interfaces.Repository;
using Hermes.Core.Interfaces.Service;
using Hermes.Core.Models;

namespace Hermes.Core.Services
{
    public class PostService : IPostService
    {
        private readonly IPostRepository _postRepository;

        public PostService(IPostRepository postRepository)
        {
            _postRepository = postRepository;
        }

        public async Task<IEnumerable<Post>> GetAllPostsAsync()
        {
            return await _postRepository.GetAllAsync();
        }

        public async Task<Post> GetPostByIdAsync(Guid id)
        {
            return await _postRepository.GetByIdAsync(id);
        }

        public async Task<Post> GetPostByAuthor(string author)
        {
            return await _postRepository.GetByAuthorAsync(author);
        }

        public async Task<Post> CreatePostAsync(Post post)
        {
            if (post is null)
            {
                throw new ArgumentNullException(nameof(post), "O post não pode ser nulo!");
            }

            return await _postRepository.CreateAsync(post);
        }

        public async Task<Post> UpdatePostAsync(Guid id, Post post)
        {
            if (post is null)
            {
                throw new ArgumentNullException(nameof(post), "O post não pode ser nulo!");
            }

            return await _postRepository.UpdateAsync(id, post);
        }

        public async Task DeletePostAsync(Guid id)
        {
            await _postRepository.DeleteAsync(id);
        }
    }
}