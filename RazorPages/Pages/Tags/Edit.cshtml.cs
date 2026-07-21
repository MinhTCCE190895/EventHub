using System.Threading.Tasks;
using BLL.Services;
using BLL.Interfaces;
using BLL.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace RazorPages.Pages.Tags;

[Authorize(Roles = "Admin,Organizer")]
public class EditModel : PageModel
{
    private readonly ITagService _tagService;

    public EditModel(ITagService tagService)
    {
        _tagService = tagService;
    }

    [BindProperty]
    public TagUpdateDTO TagUpdateDTO { get; set; } = default!;

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var tag = await _tagService.GetTagByIdAsync(id);
        if (tag == null)
        {
            return NotFound();
        }

        TagUpdateDTO = new TagUpdateDTO
        {
            Id = tag.Id,
            Name = tag.Name,
            Description = tag.Description
        };

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        try
        {
            await _tagService.UpdateTagAsync(TagUpdateDTO);
            TempData["SuccessMessage"] = "Tag updated successfully!";
        }
        catch (System.Collections.Generic.KeyNotFoundException)
        {
            return NotFound();
        }
        catch (System.Exception ex)
        {
            ModelState.AddModelError("TagUpdateDTO.Name", ex.Message);
            return Page();
        }

        return RedirectToPage("./Index");
    }
}
