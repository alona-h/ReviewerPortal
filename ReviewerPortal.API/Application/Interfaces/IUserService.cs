using ReviewerPortal.API.Application.DTOs;
using ReviewerPortal.API.Application.Models;

namespace ReviewerPortal.API.Application.Interfaces;

public interface IUserService
{
    Task<RegisterUserResponseDto> RegisterUserAsync(string userName, string universityName, int numberOfPublications);
    Task<InvitationResult> InviteReviewerAsync(int userId);
}
