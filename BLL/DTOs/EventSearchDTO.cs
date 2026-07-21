namespace BLL.DTOs;

// DTO này đi từ PL xuống BLL — BLL không biết gì về ViewModel của web
public class EventSearchDTO
{
    public string? Keyword { get; set; }
    public int? CategoryId { get; set; }
    public List<int> TagIds { get; set; } = new();
    public string? TimeFilter { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int PageNumber { get; set; } = 1;
    public string? SortBy { get; set; }
    public const int PageSize = 9;
}