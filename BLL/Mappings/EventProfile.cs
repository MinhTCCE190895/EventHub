using AutoMapper;
using BusinessObjects.DTOs;
using DAL.Entities;
using System.Linq;

namespace BLL.Mappings;

public class EventProfile : Profile
{
    public EventProfile()
    {
        CreateMap<Event, EventDTO>()
            .ForMember(d => d.OrganizerName, opt => opt.MapFrom(s => s.Organizer != null ? s.Organizer.FullName : string.Empty))
            .ForMember(d => d.VenueName, opt => opt.MapFrom(s => s.Venue != null ? s.Venue.Name : string.Empty))
            .ForMember(d => d.VenueMaxCapacity, opt => opt.MapFrom(s => s.Venue != null ? s.Venue.MaxCapacity : 0))
            .ForMember(d => d.CategoryIds, opt => opt.MapFrom(s => s.EventCategories.Select(ec => ec.CategoryId)))
            .ForMember(d => d.TagIds, opt => opt.MapFrom(s => s.EventTags.Select(et => et.TagId)));

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
