using BusinessObjects.DTOs;
using DAL.Entities;
using System.ComponentModel.DataAnnotations;

namespace RazorPages.ViewModels;

public class EventSearchViewModel
{
    [MaxLength(200)]
    public string? Keyword { get; set; }

    public int? CategoryId { get; set; }

    public List<int> TagIds { get; set; } = new();

    public string? TimeFilter { get; set; }

    [DataType(DataType.Date)]
    public DateTime? StartDate { get; set; }

    [DataType(DataType.Date)]
    public DateTime? EndDate { get; set; }

    public int PageNumber { get; set; } = 1;

    public string? SortBy { get; set; } = "DateAsc";

    public string ViewType { get; set; } = "Grid";

    // Kết quả trả về từ service sau khi map
    public List<EventCardDTO> Results { get; set; } = new();

    public int TotalCount { get; set; }

    public int TotalPages => (int)Math.Ceiling((double)TotalCount / EventSearchDTO.PageSize);

    // Dùng để populate dropdown và checkbox trên form
    public List<Category> Categories { get; set; } = new();
    public List<Tag> Tags { get; set; } = new();
}
