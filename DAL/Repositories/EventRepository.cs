using DAL.Data;
using DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repositories;

public class EventRepository : BaseRepository<Event>, IEventRepository
{
    public EventRepository(AppDbContext context) : base(context) { }

    public IQueryable<Event> BuildSearchQuery()
    {
        // Include Venue và Tags ngay từ đây để tránh N+1 query khi BLL map sang DTO
        return _dbSet
            .AsNoTracking()
            .Include(e => e.Venue)
            .Include(e => e.EventTags)
                .ThenInclude(et => et.Tag)
            .Include(e => e.EventCategories);
    }
}
