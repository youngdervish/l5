using AutoMapper;
using l5.Core.Models;
using l5.Application.DTOs;

namespace l5.Application.Mappings
{
    public class UserMappingProfile : Profile
    {
        public UserMappingProfile() 
        {
            CreateMap<User, UserDTO>();
            CreateMap<CreateUserDTO, User>();
            CreateMap<UpdateUserDTO, User>();
        }
    }
}
