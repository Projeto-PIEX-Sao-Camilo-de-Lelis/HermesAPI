using Hermes.Core.Dtos.Requests;
using Hermes.Core.Dtos.Responses;
using Hermes.Core.Models;

namespace Hermes.Core.Extensions
{
    public static class UserMapper
    {
        public static User ToEntity(UserCreateRequestDto dto)
        {
            var user = new User
            {
                Name = dto.Name,
                Email = dto.Email,
                Role = dto.Role,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            user.SetPassword(dto.Password);

            return user;
        }

        public static UserResponseDto ToResponseDto(this User user)
        {
            return new UserResponseDto
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                CreatedAt = user.CreatedAt ?? DateTime.UtcNow,
                UpdatedAt = user.UpdatedAt,
                IsActive = user.IsActive
            };
        }
        public static IEnumerable<UserResponseDto> ToResponseDto(this IEnumerable<User> users)
        {
            return users.Select(user => user.ToResponseDto());
        }

        public static void UpdateEntity(User user, UserUpdateRequestDto dto)
        {
            user.Name = dto.Name ?? user.Name;
            user.Email = dto.Email ?? user.Email;
            user.Role = dto.Role ?? user.Role;
            user.IsActive = dto.IsActive ?? user.IsActive;

            if (!string.IsNullOrWhiteSpace(dto.Password))
            {
                user.SetPassword(dto.Password);
            }

            user.UpdatedAt = DateTime.UtcNow;
        }
    }
}
