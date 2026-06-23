using AutoMapper;
using BLL.Services;
using BusinessObjects.DTOs;
using DAL.Data;
using DAL.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace RazorPages.Pages.Events;

[Authorize(Roles = "Student")]
public class OrganizerDetailsModel : PageModel
{
    private readonly AppDbContext _context;
    private readonly IFollowService _followService;
    private readonly IMapper _mapper;

    public OrganizerDetailsModel(AppDbContext context, IFollowService followService, IMapper mapper)
    {
        _context = context;
        _followService = followService;
        _mapper = mapper;
    }

    public User Organizer { get; set; } = null!;
    public int FollowerCount { get; set; }
    public bool IsFollowed { get; set; }
    public IEnumerable<EventCardDTO> Events { get; set; } = new List<EventCardDTO>();

    public async Task<IActionResult> OnGetAsync(Guid id)
    {
        var studentIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(studentIdString) || !Guid.TryParse(studentIdString, out var studentId))
        {
            return RedirectToPage("/Account/Login");
        }

        var organizer = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == id && u.Role == "Organizer" && u.IsActive);

        if (organizer == null)
        {
            return NotFound();
        }

        Organizer = organizer;

        FollowerCount = await _context.Follows.CountAsync(f => f.FolloweeId == id);
        IsFollowed = await _followService.IsFollowingAsync(studentId, id);

        var eventList = await _context.Events
            .Include(e => e.Venue)
            .Include(e => e.Organizer)
            .Include(e => e.EventTags)
                .ThenInclude(et => et.Tag)
            .Include(e => e.Bookings)
            .Where(e => e.OrganizerId == id && e.Status == "Published")
            .OrderByDescending(e => e.StartTime)
            .ToListAsync();

        Events = _mapper.Map<List<EventCardDTO>>(eventList);

        return Page();
    }

    public async Task<IActionResult> OnPostFollowAsync(Guid id)
    {
        var studentIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(studentIdString) || !Guid.TryParse(studentIdString, out var studentId))
        {
            return RedirectToPage("/Account/Login");
        }

        try
        {
            await _followService.FollowAsync(studentId, id);
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
        }

        return RedirectToPage(new { id });
    }

    public async Task<IActionResult> OnPostUnfollowAsync(Guid id)
    {
        var studentIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(studentIdString) || !Guid.TryParse(studentIdString, out var studentId))
        {
            return RedirectToPage("/Account/Login");
        }

        try
        {
            await _followService.UnfollowAsync(studentId, id);
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
        }

        return RedirectToPage(new { id });
    }
}
