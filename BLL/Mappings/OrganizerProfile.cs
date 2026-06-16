using AutoMapper;
using BusinessObjects.DTOs;
using DAL.Entities;

namespace BLL.Mappings;

public class OrganizerProfile : Profile
{
    public OrganizerProfile()
    {
        CreateMap<User, OrganizerDTO>().ReverseMap();
        CreateMap<OrganizerCreateDTO, User>();
        CreateMap<OrganizerUpdateDTO, User>();
    }
}
