using System.ComponentModel.DataAnnotations;

namespace API.DTOs.Theme;

public class SaveThemeRequest
{
    [Required]
    public required ThemeDataDto ThemeData { get; set; }
    
    [Required]
    [StringLength(1000)]
    public required string Description { get; set; }
}
