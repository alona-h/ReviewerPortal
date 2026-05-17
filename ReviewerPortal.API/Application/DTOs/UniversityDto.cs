namespace ReviewerPortal.API.Application.DTOs;

public class UniversityDto
{
    public int UniversityId { get; set; }
    public string UniversityName { get; set; } = string.Empty;
    public decimal Score { get; set; }
}
