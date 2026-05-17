using Microsoft.EntityFrameworkCore;
using ReviewerPortal.API.Application.Exceptions;
using ReviewerPortal.API.Application.Interfaces;
using ReviewerPortal.API.Domain.Entities;
using ReviewerPortal.API.Infrastructure.Persistence;

namespace ReviewerPortal.API.Application.Services;

public class UniversityService(AppDbContext context, IUniversityApiClient universityApiClient) : IUniversityService
{
    public async Task<University> GetOrCreateUniversityAsync(string universityName)
    {
        var cached = await context.Universities
            .FirstOrDefaultAsync(u => u.UniversityName == universityName);
        if (cached is not null)
            return cached;

        var apiResult = await universityApiClient.FindAsync(universityName);
        if (apiResult is null)
            throw new BadRequestException($"No university found for '{universityName}'.");

        var cachedByApiName = await context.Universities
            .FirstOrDefaultAsync(u => u.UniversityName == apiResult.Name);
        if (cachedByApiName is not null)
            return cachedByApiName;

        var university = new University
        {
            UniversityName = apiResult.Name,
            Score = apiResult.Score
        };

        context.Universities.Add(university);
        try
        {
            await context.SaveChangesAsync();
            return university;
        }
        catch (DbUpdateException)
        {
            context.Entry(university).State = EntityState.Detached;
            return await context.Universities
                .FirstAsync(u => u.UniversityName == apiResult.Name);
        }
    }
}
