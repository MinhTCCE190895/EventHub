using System.Collections.Generic;
using System.Threading.Tasks;
using BLL.Services;
using BLL.Interfaces;
using BusinessObjects.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace RazorPages.Pages.Categories;

[Authorize(Roles = "Admin,Organizer")]
public class IndexModel : PageModel
{
    private readonly ICategoryService _categoryService;

    public IndexModel(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    public IEnumerable<CategoryDTO> Categories { get; set; } = new List<CategoryDTO>();

    public async Task OnGetAsync()
    {
        Categories = await _categoryService.GetAllCategoriesAsync();
    }
}
