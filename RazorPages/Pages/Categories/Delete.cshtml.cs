using System.Threading.Tasks;
using BLL.Services;
using BLL.Interfaces;
using BusinessObjects.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace RazorPages.Pages.Categories;

[Authorize(Roles = "Admin,Organizer")]
public class DeleteModel : PageModel
{
    private readonly ICategoryService _categoryService;

    public DeleteModel(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    [BindProperty]
    public CategoryDTO CategoryDTO { get; set; } = default!;

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var category = await _categoryService.GetCategoryByIdAsync(id);
        if (category == null)
        {
            return NotFound();
        }

        CategoryDTO = category;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        try
        {
            await _categoryService.DeleteCategoryAsync(id);
            TempData["SuccessMessage"] = "Category deleted successfully!";
        }
        catch (System.InvalidOperationException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            return RedirectToPage("./Delete", new { id });
        }
        catch (System.Collections.Generic.KeyNotFoundException)
        {
            return NotFound();
        }

        return RedirectToPage("./Index");
    }
}
