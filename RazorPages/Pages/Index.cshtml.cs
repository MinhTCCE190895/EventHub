using BLL.Services;
using BLL.Interfaces;
using BusinessObjects.DTOs;
using DAL.Entities;
using DAL.Repositories;
using DAL.Interfaces;
using DAL.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RazorPages.ViewModels;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace RazerPages.Pages;

public class IndexModel : PageModel
{
    private readonly ISearchService _searchService;
    private readonly IRepository<Category> _categoryRepo;
    private readonly IRepository<Tag> _tagRepo;
    private readonly IRepository<User> _userRepo;
    private readonly AppDbContext _context;
    private readonly ILogger<IndexModel> _logger;

    // Get actual ID instead of hardcoding
    public Guid? CurrentStudentId => User.Identity?.IsAuthenticated == true 
        ? Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!) 
        : null;

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

    public List<Guid> BookmarkedEventIds { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return Page();

        // Retrieve bookmarks if user is authenticated and is a student
        if (CurrentStudentId.HasValue && User.IsInRole("Student"))
        {
            BookmarkedEventIds = await _context.Bookmarks
                .Where(b => b.StudentId == CurrentStudentId.Value)
                .Select(b => b.EventId)
                .ToListAsync(cancellationToken);
        }

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
        if (!CurrentStudentId.HasValue) return;
        var exists = await _userRepo.ExistsAsync(u => u.Id == CurrentStudentId.Value, cancellationToken);
        if (!exists)
        {
            // Create a mock student if not exists to ensure db constraints are satisfied
            var student = new User
            {
                Id = CurrentStudentId.Value,
                FullName = "Demo Student (QuiNC)",
                Role = "Student",
                Email = "student.demo@unieventhub.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456"),
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };
            await _userRepo.AddAsync(student, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
