using BLL.Services;
using BusinessObjects.DTOs;
using DAL.Entities;
using DAL.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RazorPages.ViewModels;

namespace RazerPages.Pages;

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
        _categoryRepo = categoryRepo;
        _tagRepo = tagRepo;
        _logger = logger;
    }

    [BindProperty(SupportsGet = true)]
    public EventSearchViewModel SearchVm { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return Page();

        try
        {
            SearchVm.Categories = (await _categoryRepo.GetAllAsync(cancellationToken)).ToList();
            SearchVm.Tags = (await _tagRepo.GetAllAsync(cancellationToken)).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Không load được danh sách category/tag trên trang chủ");
        }

        var searchDto = new EventSearchDTO
        {
            Keyword = SearchVm.Keyword,
            CategoryId = SearchVm.CategoryId,
            TagIds = SearchVm.TagIds,
            TimeFilter = SearchVm.TimeFilter,
            StartDate = SearchVm.StartDate,
            EndDate = SearchVm.EndDate,
            PageNumber = SearchVm.PageNumber,
            SortBy = SearchVm.SortBy
        };

        var (items, totalCount) = await _searchService.SearchEventsAsync(searchDto, cancellationToken);

        SearchVm.Results = items;
        SearchVm.TotalCount = totalCount;

        // Clamp page number to valid range to prevent out of bounds
        SearchVm.PageNumber = Math.Clamp(SearchVm.PageNumber, 1, Math.Max(1, SearchVm.TotalPages));

        return Page();
    }
}
