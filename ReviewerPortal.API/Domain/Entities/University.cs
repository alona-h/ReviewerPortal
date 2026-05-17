namespace ReviewerPortal.API.Domain.Entities;

public class University
{
    public int UniversityId { get; set; }
    public string UniversityName { get; set; } = string.Empty;
    public decimal Score { get; set; }
}
