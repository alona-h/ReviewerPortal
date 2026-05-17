using Microsoft.AspNetCore.Mvc;
using ReviewerPortal.API.Application.Interfaces;
using ReviewerPortal.API.Controllers.Models;

namespace ReviewerPortal.API.Controllers;

[ApiController]
[Route("api/users")]
public class UsersController(IUserService userService) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Register([FromBody] RegisterUserRequest request)
    {
        var result = await userService.RegisterUserAsync(
            request.UserName,
            request.UniversityName,
            request.NumberOfPublications!.Value);
        return Created(string.Empty, result);
    }

    [HttpPost("{userId:int}/invitations")]
    public async Task<IActionResult> Invite(int userId)
    {
        var result = await userService.InviteReviewerAsync(userId);
        return Ok(result);
    }
}
