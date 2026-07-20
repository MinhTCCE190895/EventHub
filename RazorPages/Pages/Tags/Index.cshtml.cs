using System.Collections.Generic;
using System.Threading.Tasks;
using BLL.Services;
using BLL.Interfaces;
using BLL.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace RazorPages.Pages.Tags;

[Authorize(Roles = "Admin,Organizer")]
public class IndexModel : PageModel
{
    private readonly ITagService _tagService;

    public IndexModel(ITagService tagService)
    {
        _tagService = tagService;
    }

    public IEnumerable<TagDTO> Tags { get; set; } = new List<TagDTO>();

    public async Task OnGetAsync()
    {
        Tags = await _tagService.GetAllTagsAsync();
    }
}
