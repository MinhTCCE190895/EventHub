using AutoMapper;
using BLL.DTOs;
using DAL.Entities;

namespace BLL.Mappings;

public class VenueProfile : Profile
{
    public VenueProfile()
    {
        CreateMap<Venue, VenueDTO>().ReverseMap();
        CreateMap<VenueCreateDTO, Venue>();
        CreateMap<VenueUpdateDTO, Venue>();
    }
}
