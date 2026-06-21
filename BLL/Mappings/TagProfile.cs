using AutoMapper;
using BusinessObjects.DTOs;
using DAL.Entities;

namespace BLL.Mappings;

public class TagProfile : Profile
{
    public TagProfile()
    {
        CreateMap<Tag, TagDTO>().ReverseMap();
        CreateMap<TagCreateDTO, Tag>();
        CreateMap<TagUpdateDTO, Tag>();
    }
}
