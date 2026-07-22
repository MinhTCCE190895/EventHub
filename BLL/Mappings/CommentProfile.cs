using AutoMapper;
using BLL.DTOs;
using DAL.Entities;

namespace BLL.Mappings;

public class CommentProfile : Profile
{
    public CommentProfile()
    {
        CreateMap<EventComment, CommentDTO>()
            .ForMember(d => d.UserFullName, opt => opt.MapFrom(s => s.User != null ? s.User.FullName : string.Empty))
            .ForMember(d => d.UserAvatarUrl, opt => opt.MapFrom(s => s.User != null ? s.User.AvatarUrl : string.Empty))
            .ForMember(d => d.UserRole, opt => opt.MapFrom(s => s.User != null ? s.User.Role : string.Empty))
            .ForMember(d => d.Replies, opt => opt.MapFrom(s => s.Replies));
    }
}
