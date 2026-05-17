using System.Net.Http.Json;
using ReviewerPortal.API.Application.Exceptions;
using ReviewerPortal.API.Application.Interfaces;
using ReviewerPortal.API.Application.Models;
using ReviewerPortal.API.Infrastructure.ExternalApi.Models;

namespace ReviewerPortal.API.Infrastructure.ExternalApi;

public class UniversityApiClient(IHttpClientFactory httpClientFactory) : IUniversityApiClient
{
    public async Task<UniversityApiResult?> FindAsync(string query)
    {
        var client = httpClientFactory.CreateClient("UniversityApi");
        var url = $"/v1/organizations/elasticSuggestions?query={Uri.EscapeDataString(query)}&maxcount=1";

        HttpResponseMessage response;
        try
        {
            response = await client.GetAsync(url);
        }
        catch (HttpRequestException ex)
        {
            throw new BadRequestException($"University API is unreachable: {ex.Message}");
        }

        using (response)
        {
            if (!response.IsSuccessStatusCode)
                throw new BadRequestException($"University API returned an error: {(int)response.StatusCode}.");

            var suggestions = await response.Content.ReadFromJsonAsync<List<UniversitySuggestion>>() ?? [];
            var first = suggestions.FirstOrDefault();
            return first is null ? null : new UniversityApiResult(first.OrganizationName, first.Score);
        }
    }
}
