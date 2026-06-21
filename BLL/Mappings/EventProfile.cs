using AutoMapper;
using BusinessObjects.DTOs;
using DAL.Entities;

namespace BLL.Mappings;

public class EventProfile : Profile
{
    public EventProfile()
    {
        CreateMap<Event, EventDTO>()
            .ForMember(d => d.OrganizerName, opt => opt.MapFrom(s => s.Organizer != null ? s.Organizer.FullName : string.Empty))
            .ForMember(d => d.VenueName, opt => opt.MapFrom(s => s.Venue != null ? s.Venue.Name : string.Empty))
            .ForMember(d => d.VenueMaxCapacity, opt => opt.MapFrom(s => s.Venue != null ? s.Venue.MaxCapacity : 0));

        CreateMap<EventCreateDTO, Event>()
            .ForMember(d => d.Id, opt => opt.Ignore())
            .ForMember(d => d.CreatedAt, opt => opt.Ignore())
            .ForMember(d => d.RegisteredCount, opt => opt.Ignore())
            .ForMember(d => d.RowVersion, opt => opt.Ignore());

        CreateMap<EventUpdateDTO, Event>()
            .ForMember(d => d.CreatedAt, opt => opt.Ignore())
            .ForMember(d => d.RegisteredCount, opt => opt.Ignore())
            .ForMember(d => d.RowVersion, opt => opt.Ignore());
    }
}
