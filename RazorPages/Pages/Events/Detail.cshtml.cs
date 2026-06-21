using DAL.Data;
using DAL.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace RazorPages.Pages.Events
{
    public class DetailModel : PageModel
    {
        private readonly AppDbContext _context;

        public DetailModel(AppDbContext context)
        {
            _context = context;
        }

        public Event EventItem { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(Guid id)
        {
            var eventItem = await _context.Events
                .Include(e => e.Venue)
                .Include(e => e.Organizer)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (eventItem == null)
            {
                return NotFound();
            }

            EventItem = eventItem;
            return Page();
        }
    }
}
