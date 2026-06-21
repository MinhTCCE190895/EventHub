using System.Threading.Tasks;
using BLL.Services;
using BusinessObjects.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace RazorPages.Pages.Categories;

[Authorize(Roles = "Admin,Organizer")]
public class DetailsModel : PageModel
{
    private readonly ICategoryService _categoryService;

    public DetailsModel(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

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
}
