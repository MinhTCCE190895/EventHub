using BLL.DTOs;
using System.ComponentModel.DataAnnotations;

namespace RazorPages.ViewModels;

public class EventSearchViewModel : IValidatableObject
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

    // Results returned from service after mapping
    public List<EventCardDTO> Results { get; set; } = new();

    public int TotalCount { get; set; }

    public int TotalPages => (int)Math.Ceiling((double)TotalCount / EventSearchDTO.PageSize);

    // Used to populate dropdowns and checkboxes on the form — use DTOs, not Entities
    public List<CategoryDTO> Categories { get; set; } = new();
    public List<TagDTO> Tags { get; set; } = new();

    // Custom Validation (B.E) - Check if EndDate is not earlier than StartDate
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (StartDate.HasValue && EndDate.HasValue && EndDate.Value < StartDate.Value)
        {
            yield return new ValidationResult(
                "Ngày kết thúc phải lớn hơn hoặc bằng ngày bắt đầu.",
                new[] { nameof(EndDate) });
        }
    }
}

