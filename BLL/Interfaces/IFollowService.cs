using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BLL.DTOs;

namespace BLL.Interfaces;

public interface IFollowService
{
    // L?y danh sách t?t c? các don v? t? ch?c kèm s? lu?ng ngu?i theo dõi.
    // Nh?n vào mã ngu?i dùng hi?n t?i d? dánh d?u tr?ng thái dã theo dõi (IsFollowed) tuong ?ng trong danh sách tr? v?.
    Task<IEnumerable<OrganizerDto>> GetOrganizersWithFollowCountAsync(Guid currentUserId);

    // Ki?m tra xem m?t ngu?i dùng có dang theo dõi m?t don v? t? ch?c hay không.
    // Ð?i chi?u c?p mã ngu?i theo dõi (Follower) và ngu?i du?c theo dõi (Followee) xem có t?n t?i m?i quan h? trong h? th?ng.
    Task<bool> IsFollowingAsync(Guid followerId, Guid followeeId);

    // Th?c hi?n thi?t l?p quan h? theo dõi gi?a ngu?i dùng và don v? t? ch?c.
    // Nh?n vào mã ngu?i dùng và mã don v? t? ch?c d? t?o m?i m?t b?n ghi theo dõi n?u chua t?n t?i.
    Task FollowAsync(Guid followerId, Guid followeeId);

    // H?y b? quan h? theo dõi gi?a ngu?i dùng và don v? t? ch?c.
    // Nh?n vào mã ngu?i dùng và mã don v? t? ch?c d? xóa b?n ghi theo dõi tuong ?ng ra kh?i h? th?ng.
    Task UnfollowAsync(Guid followerId, Guid followeeId);

    // L?y danh sách các don v? t? ch?c mà m?t ngu?i dùng c? th? dang theo dõi.
    // L?c và tr? v? thông tin các don v? t? ch?c d?a theo mã c?a ngu?i theo dõi (FollowerId).
    Task<IEnumerable<OrganizerDto>> GetFollowedOrganizersAsync(Guid followerId);

    // L?y danh sách các s? ki?n m?i nh?t t? các don v? t? ch?c mà ngu?i dùng dã theo dõi.
    // Tìm các don v? t? ch?c du?c ngu?i dùng quan tâm, sau dó l?c ra các s? ki?n ? tr?ng thái hi?n th? c?a các don v? dó.
    Task<IEnumerable<EventCardDTO>> GetNewEventsFromFollowedOrganizersAsync(Guid followerId);
}