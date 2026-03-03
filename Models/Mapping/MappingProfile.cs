using AutoMapper;
using Sehha360.Models;
using Sehha360.Models.DTOs;
namespace Sehha360.Models.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<UserRegisterDTO,AppUser>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest=>dest.CreatedAt,opt=>opt.MapFrom(src=> DateTime.UtcNow));
            CreateMap<UserResponseDTO,AppUser>().ReverseMap();
            CreateMap<AppUser, UserProfileDTO>();
        }
    }
}
