using DAL.Entities;

namespace DAL.Interfaces;

public interface IEventRepository : IRepository<Event>
{
    // Trả IQueryable chưa execute để BLL tự chain thêm Where() theo từng điều kiện tìm kiếm
    IQueryable<Event> BuildSearchQuery();
}
