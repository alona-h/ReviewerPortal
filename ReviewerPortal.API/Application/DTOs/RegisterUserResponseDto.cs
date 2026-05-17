namespace ReviewerPortal.API.Application.DTOs;

public class RegisterUserResponseDto
{
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public int NumberOfPublications { get; set; }
    public UniversityDto University { get; set; } = null!;
}
