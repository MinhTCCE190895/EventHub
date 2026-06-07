using BLL.Services;
using BusinessObjects.DTOs;
using DAL.Entities;
using DAL.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RazorPages.ViewModels;

namespace RazerPages.Pages.Explore;

public class IndexModel : PageModel
{
    private readonly ISearchService _searchService;
    private readonly IRepository<Category> _categoryRepo;
    private readonly IRepository<Tag> _tagRepo;
    private readonly ILogger<IndexModel> _logger;

    public IndexModel(
        ISearchService searchService,
        IRepository<Category> categoryRepo,
        IRepository<Tag> tagRepo,
        ILogger<IndexModel> logger)
    {
        _searchService = searchService;
        _categoryRepo  = categoryRepo;
        _tagRepo       = tagRepo;
        _logger        = logger;
    }

    [BindProperty(SupportsGet = true)]
    public EventSearchViewModel SearchVm { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return Page();

        // Load category và tag song song vì hai cái này không phụ thuộc nhau
        var categoriesTask = _categoryRepo.GetAllAsync(cancellationToken);
        var tagsTask       = _tagRepo.GetAllAsync(cancellationToken);

        try
        {
            await Task.WhenAll(categoriesTask, tagsTask);
            SearchVm.Categories = (await categoriesTask).ToList();
            SearchVm.Tags       = (await tagsTask).ToList();
        }
        catch (Exception ex)
        {
            // Không crash trang nếu load filter options lỗi — form vẫn hiện, chỉ thiếu dropdown
            _logger.LogError(ex, "Không load được danh sách category/tag");
        }

        var searchDto = new EventSearchDTO
        {
            Keyword    = SearchVm.Keyword,
            CategoryId = SearchVm.CategoryId,
            TagIds     = SearchVm.TagIds,
            TimeFilter = SearchVm.TimeFilter,
            PageNumber = SearchVm.PageNumber
        };

        var (items, totalCount) = await _searchService.SearchEventsAsync(searchDto, cancellationToken);

        SearchVm.Results    = items;
        SearchVm.TotalCount = totalCount;

        // Clamp về trang hợp lệ sau khi đã biết TotalPages
        SearchVm.PageNumber = Math.Clamp(SearchVm.PageNumber, 1, Math.Max(1, SearchVm.TotalPages));

        return Page();
    }
}
