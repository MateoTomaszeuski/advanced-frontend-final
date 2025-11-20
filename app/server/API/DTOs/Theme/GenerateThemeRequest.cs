using System.ComponentModel.DataAnnotations;

namespace API.DTOs.Theme;

public class GenerateThemeRequest {
    [Required]
    [StringLength(1000, MinimumLength = 10)]
    public required string Description { get; set; }
}