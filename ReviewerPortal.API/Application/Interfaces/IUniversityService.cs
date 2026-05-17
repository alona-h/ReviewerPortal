using ReviewerPortal.API.Domain.Entities;

namespace ReviewerPortal.API.Application.Interfaces;

public interface IUniversityService
{
    Task<University> GetOrCreateUniversityAsync(string universityName);
}
