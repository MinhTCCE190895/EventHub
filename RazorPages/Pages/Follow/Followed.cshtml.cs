using BLL.Services;
using BLL.Interfaces;
using BusinessObjects.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace RazorPages.Pages.Follow;

[Authorize(Roles = "Student")]
public class FollowedModel : PageModel
{
    private readonly IFollowService _followService;

    public FollowedModel(IFollowService followService)
    {
        _followService = followService;
    }

    public IEnumerable<OrganizerDto> FollowedOrganizers { get; set; } = new List<OrganizerDto>();
    public IEnumerable<EventCardDTO> Events { get; set; } = new List<EventCardDTO>();

    public async Task<IActionResult> OnGetAsync()
    {
        var studentIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(studentIdString) || !Guid.TryParse(studentIdString, out var studentId))
        {
            return RedirectToPage("/Account/Login");
        }

        FollowedOrganizers = await _followService.GetFollowedOrganizersAsync(studentId);
        Events = await _followService.GetNewEventsFromFollowedOrganizersAsync(studentId);

        return Page();
    }

    public async Task<IActionResult> OnPostUnfollowAsync(Guid organizerId)
    {
        var studentIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(studentIdString) || !Guid.TryParse(studentIdString, out var studentId))
        {
            return RedirectToPage("/Account/Login");
        }

        try
        {
            await _followService.UnfollowAsync(studentId, organizerId);
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
        }

        return RedirectToPage();
    }
}
