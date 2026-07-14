using AutoMapper;
using BusinessObjects.DTOs;
using DAL.Entities;

namespace BLL.Mappings;

public class CommentProfile : Profile
{
    public CommentProfile()
    {
        CreateMap<EventComment, CommentDTO>()
            .ForMember(d => d.UserFullName, opt => opt.MapFrom(s => s.User != null ? s.User.FullName : string.Empty))
            .ForMember(d => d.UserAvatarUrl, opt => opt.MapFrom(s => s.User != null ? s.User.AvatarUrl : string.Empty));
    }
}
