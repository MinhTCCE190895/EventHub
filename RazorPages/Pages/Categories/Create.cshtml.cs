using System.Threading.Tasks;
using BLL.Services;
using BLL.Interfaces;
using BusinessObjects.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace RazorPages.Pages.Categories;

[Authorize(Roles = "Admin,Organizer")]
public class CreateModel : PageModel
{
    private readonly ICategoryService _categoryService;

    public CreateModel(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    [BindProperty]
    public CategoryCreateDTO CategoryCreateDTO { get; set; } = default!;

    public IActionResult OnGet()
    {
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
            await _categoryService.CreateCategoryAsync(CategoryCreateDTO);
            TempData["SuccessMessage"] = "Category created successfully!";
            return RedirectToPage("./Index");
        }
        catch (System.InvalidOperationException ex)
        {
            ModelState.AddModelError("CategoryCreateDTO.Name", ex.Message);
            return Page();
        }
    }
}
