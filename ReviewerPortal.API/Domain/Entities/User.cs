namespace ReviewerPortal.API.Domain.Entities;

public class User
{
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public int UniversityId { get; set; }
    public int NumberOfPublications { get; set; }
    public University University { get; set; } = null!;
}
