using System.Threading.Tasks;
using BLL.Services;
using BLL.Interfaces;
using BLL.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace RazorPages.Pages.Tags;

[Authorize(Roles = "Admin,Organizer")]
public class DeleteModel : PageModel
{
    private readonly ITagService _tagService;

    public DeleteModel(ITagService tagService)
    {
        _tagService = tagService;
    }

    [BindProperty]
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

    public async Task<IActionResult> OnPostAsync(int id)
    {
        try
        {
            await _tagService.DeleteTagAsync(id);
            TempData["SuccessMessage"] = "Tag deleted successfully!";
        }
        catch (System.Collections.Generic.KeyNotFoundException)
        {
            return NotFound();
        }
        catch (System.Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            return RedirectToPage("./Delete", new { id });
        }

        return RedirectToPage("./Index");
    }
}
