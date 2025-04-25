using AutoMapper;
using l5.Core.Models;
using l5.Application.DTOs;


namespace l5.Application.Mappings
{
    public class BookMappingProfile : Profile
    {
        public BookMappingProfile() 
        {
            CreateMap<Book, BookDTO>().ReverseMap();
            CreateMap<Book, BookResponseDTO>();
        }
        
    }
}
