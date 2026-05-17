using System.ComponentModel.DataAnnotations;

namespace ReviewerPortal.API.Controllers.Models;

public class RegisterUserRequest
{
    [Required]
    [MinLength(3)]
    public string UserName { get; set; } = string.Empty;

    [Required]
    [MinLength(3)]
    public string UniversityName { get; set; } = string.Empty;

    [Required]
    [Range(0, int.MaxValue)]
    public int? NumberOfPublications { get; set; }
}
