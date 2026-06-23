using System.Threading.Tasks;
using BLL.Services;
using BusinessObjects.DTOs;
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
        catch (Microsoft.EntityFrameworkCore.DbUpdateException)
        {
            TempData["ErrorMessage"] = "Không thể xóa thẻ này vì đang được sử dụng.";
            return RedirectToPage("./Delete", new { id });
        }
        catch (System.Collections.Generic.KeyNotFoundException)
        {
            return NotFound();
        }

        return RedirectToPage("./Index");
    }
}
