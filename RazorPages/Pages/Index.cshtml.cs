using BLL.Services;
using BusinessObjects.DTOs;
using DAL.Entities;
using DAL.Repositories;
using DAL.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RazorPages.ViewModels;

namespace RazerPages.Pages;

public class IndexModel : PageModel
{
    private readonly ISearchService _searchService;
    private readonly IRepository<Category> _categoryRepo;
    private readonly IRepository<Tag> _tagRepo;
    private readonly IRepository<User> _userRepo;
    private readonly AppDbContext _context;
    private readonly ILogger<IndexModel> _logger;

    public static readonly Guid CurrentStudentId = new("77777777-7777-7777-7777-777777777777");

    public IndexModel(
        ISearchService searchService,
        IRepository<Category> categoryRepo,
        IRepository<Tag> tagRepo,
        IRepository<User> userRepo,
        AppDbContext context,
        ILogger<IndexModel> logger)
    {
        _searchService = searchService;
        _categoryRepo = categoryRepo;
        _tagRepo = tagRepo;
        _userRepo = userRepo;
        _context = context;
        _logger = logger;
    }

    [BindProperty(SupportsGet = true)]
    public EventSearchViewModel SearchVm { get; set; } = new();



    public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return Page();

        await EnsureStudentExistsAsync(cancellationToken);



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

    private async Task EnsureStudentExistsAsync(CancellationToken cancellationToken)
    {
        var exists = await _userRepo.ExistsAsync(u => u.Id == CurrentStudentId, cancellationToken);
        if (!exists)
        {
            // Create a mock student if not exists to ensure db constraints are satisfied
            var student = new User
            {
                Id = CurrentStudentId,
                FullName = "Demo Student (QuiNC)",
                Role = "Student",
                Email = "student.demo@unieventhub.com",
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };
            await _userRepo.AddAsync(student, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
