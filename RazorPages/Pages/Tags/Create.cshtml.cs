using System.Threading.Tasks;
using BLL.Services;
using BusinessObjects.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace RazorPages.Pages.Tags;

[Authorize(Roles = "Admin,Organizer")]
public class CreateModel : PageModel
{
    private readonly ITagService _tagService;

    public CreateModel(ITagService tagService)
    {
        _tagService = tagService;
    }

    [BindProperty]
    public TagCreateDTO TagCreateDTO { get; set; } = default!;

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

        await _tagService.CreateTagAsync(TagCreateDTO);
        TempData["SuccessMessage"] = "Tag created successfully!";
        
        return RedirectToPage("./Index");
    }
}
