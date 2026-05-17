using Microsoft.EntityFrameworkCore;
using ReviewerPortal.API.Application.Constants;
using ReviewerPortal.API.Application.DTOs;
using ReviewerPortal.API.Application.Exceptions;
using ReviewerPortal.API.Application.Interfaces;
using ReviewerPortal.API.Application.Models;
using ReviewerPortal.API.Domain.Entities;
using ReviewerPortal.API.Infrastructure.Persistence;

namespace ReviewerPortal.API.Application.Services;

public class UserService(AppDbContext context, IUniversityService universityService) : IUserService
{
    public async Task<RegisterUserResponseDto> RegisterUserAsync(string userName, string universityName, int numberOfPublications)
    {
        if (await context.Users.AnyAsync(u => u.UserName == userName))
            throw new BadRequestException($"Username '{userName}' is already taken.");

        var university = await universityService.GetOrCreateUniversityAsync(universityName);

        var user = new User
        {
            UserName = userName,
            UniversityId = university.UniversityId,
            NumberOfPublications = numberOfPublications
        };

        context.Users.Add(user);
        await context.SaveChangesAsync();

        return new RegisterUserResponseDto
        {
            UserId = user.UserId,
            UserName = user.UserName,
            NumberOfPublications = user.NumberOfPublications,
            University = new UniversityDto
            {
                UniversityId = university.UniversityId,
                UniversityName = university.UniversityName,
                Score = university.Score
            }
        };
    }

    public async Task<InvitationResult> InviteReviewerAsync(int userId)
    {
        var user = await context.Users
            .Include(u => u.University)
            .FirstOrDefaultAsync(u => u.UserId == userId);

        if (user is null)
            throw new NotFoundException($"User with ID {userId} was not found.");

        var eligible = user.NumberOfPublications > EligibilityRules.MinimumPublications
            && user.University.Score >= EligibilityRules.MinimumUniversityScore;

        return eligible
            ? new InvitationResult(true, EligibilityRules.InvitationSuccessMessage)
            : new InvitationResult(false, EligibilityRules.InvitationFailureMessage);
    }
}
