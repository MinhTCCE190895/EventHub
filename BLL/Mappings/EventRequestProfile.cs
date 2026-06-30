using AutoMapper;
using BusinessObjects.DTOs;
using DAL.Entities;

namespace BLL.Mappings;

public class EventRequestProfile : Profile
{
    public EventRequestProfile()
    {
        CreateMap<EventRequest, EventRequestDTO>()
            .ForMember(d => d.StudentName, opt => opt.MapFrom(s => s.Student != null ? s.Student.FullName : string.Empty))
            .ForMember(d => d.SubmittedAt, opt => opt.MapFrom(s => s.SubmittedAt.AddHours(7)));

        CreateMap<EventRequestCreateDTO, EventRequest>()
            .ForMember(d => d.Id, opt => opt.Ignore())
            .ForMember(d => d.StudentId, opt => opt.Ignore())
            .ForMember(d => d.Status, opt => opt.MapFrom(_ => "Pending"))
            .ForMember(d => d.SubmittedAt, opt => opt.Ignore())
            .ForMember(d => d.ResponseMessage, opt => opt.Ignore())
            .ForMember(d => d.Student, opt => opt.Ignore());
    }
}
