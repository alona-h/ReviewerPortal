using ReviewerPortal.API.Application.Models;

namespace ReviewerPortal.API.Application.Interfaces;

public interface IUniversityApiClient
{
    Task<UniversityApiResult?> FindAsync(string query);
}
