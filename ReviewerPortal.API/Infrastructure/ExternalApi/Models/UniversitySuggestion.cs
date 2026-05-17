using System.Text.Json.Serialization;

namespace ReviewerPortal.API.Infrastructure.ExternalApi.Models;

public class UniversitySuggestion
{
    [JsonPropertyName("organizationName")]
    public string OrganizationName { get; set; } = string.Empty;

    [JsonPropertyName("score")]
    public decimal Score { get; set; }
}
