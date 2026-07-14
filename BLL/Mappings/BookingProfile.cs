using AutoMapper;
using BusinessObjects.DTOs;
using DAL.Entities;

namespace BLL.Mappings;

public class BookingProfile : Profile
{
    public BookingProfile()
    {
        CreateMap<Booking, BookingDTO>()
            .ForMember(d => d.EventTitle, opt => opt.MapFrom(s => s.Event != null ? s.Event.Title : string.Empty))
            .ForMember(d => d.StudentName, opt => opt.MapFrom(s => s.Student != null ? s.Student.FullName : string.Empty));
    }
}
