using AutoMapper;
using Hermes.Core.Models;
using Hermes.Core.Dtos.Requests;
using Hermes.Core.Dtos.Responses;

namespace Hermes.Core.Profiles
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<User, UserCreateRequestDto>().ReverseMap();
            CreateMap<UserUpdateRequestDto, User>().ReverseMap();
            CreateMap<UserResponseDto, User>().ReverseMap();

            CreateMap<PostCreateRequestDto, Post>().ReverseMap();
            CreateMap<PostUpdateRequestDto, Post>().ReverseMap();
        }
    }
}
