using System.ComponentModel.DataAnnotations;

namespace TaskDb.Models;

public class CreateTaskDto
{
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string Description { get; set; } = string.Empty;

    [Required]
    [MaxLength(20)]
    public string Priority { get; set; } = "Normal";
}

public class UpdateTaskDto
{
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string Description { get; set; } = string.Empty;

    public bool IsCompleted { get; set; }

    [Required]
    [MaxLength(20)]
    public string Priority { get; set; } = "Normal";
}
