using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BusinessObjects.DTOs;

namespace BLL.Interfaces;

public interface IFollowService
{
    // Lấy danh sách tất cả các đơn vị tổ chức kèm số lượng người theo dõi.
    // Nhận vào mã người dùng hiện tại để đánh dấu trạng thái đã theo dõi (IsFollowed) tương ứng trong danh sách trả về.
    Task<IEnumerable<OrganizerDto>> GetOrganizersWithFollowCountAsync(Guid currentUserId);

    // Kiểm tra xem một người dùng có đang theo dõi một đơn vị tổ chức hay không.
    // Đối chiếu cặp mã người theo dõi (Follower) và người được theo dõi (Followee) xem có tồn tại mối quan hệ trong hệ thống.
    Task<bool> IsFollowingAsync(Guid followerId, Guid followeeId);

    // Thực hiện thiết lập quan hệ theo dõi giữa người dùng và đơn vị tổ chức.
    // Nhận vào mã người dùng và mã đơn vị tổ chức để tạo mới một bản ghi theo dõi nếu chưa tồn tại.
    Task FollowAsync(Guid followerId, Guid followeeId);

    // Hủy bỏ quan hệ theo dõi giữa người dùng và đơn vị tổ chức.
    // Nhận vào mã người dùng và mã đơn vị tổ chức để xóa bản ghi theo dõi tương ứng ra khỏi hệ thống.
    Task UnfollowAsync(Guid followerId, Guid followeeId);

    // Lấy danh sách các đơn vị tổ chức mà một người dùng cụ thể đang theo dõi.
    // Lọc và trả về thông tin các đơn vị tổ chức dựa theo mã của người theo dõi (FollowerId).
    Task<IEnumerable<OrganizerDto>> GetFollowedOrganizersAsync(Guid followerId);

    // Lấy danh sách các sự kiện mới nhất từ các đơn vị tổ chức mà người dùng đã theo dõi.
    // Tìm các đơn vị tổ chức được người dùng quan tâm, sau đó lọc ra các sự kiện ở trạng thái hiển thị của các đơn vị đó.
    Task<IEnumerable<EventCardDTO>> GetNewEventsFromFollowedOrganizersAsync(Guid followerId);
}