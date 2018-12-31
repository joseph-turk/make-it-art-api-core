using AutoMapper;
using MakeItArtApi.Models;
using MakeItArtApi.Dtos;

namespace MakeItArtApi.Helpers
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<User, UserDto>();
            CreateMap<UserDto, User>();
        }
    }
}