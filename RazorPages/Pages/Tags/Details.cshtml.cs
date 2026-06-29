using System.Threading.Tasks;
using BLL.Services;
using BLL.Interfaces;
using BusinessObjects.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace RazorPages.Pages.Tags;

[Authorize(Roles = "Admin,Organizer")]
public class DetailsModel : PageModel
{
    private readonly ITagService _tagService;

    public DetailsModel(ITagService tagService)
    {
        _tagService = tagService;
    }

    public TagDTO TagDTO { get; set; } = default!;

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var tag = await _tagService.GetTagByIdAsync(id);
        if (tag == null)
        {
            return NotFound();
        }

        TagDTO = tag;
        return Page();
    }
}
