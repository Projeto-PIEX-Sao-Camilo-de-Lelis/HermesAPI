﻿using Hermes.Core.Models;

namespace Hermes.Core.Interfaces.Repository
{
    public interface IUserRepository : IGenericRepository<User>
    {
        Task<User?> GetByEmailAsync(string email);
    }
}