using System.ComponentModel.DataAnnotations;

namespace ShoppingApp.Models;

public class ApplicationUser
{
    public int Id { get; set; }

    [Required, StringLength(100)]
    public string FullName { get; set; } = string.Empty;

    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string PasswordHash { get; set; } = string.Empty;

    /// <summary>"Client" or "Admin"</summary>
    [Required, StringLength(20)]
    public string Role { get; set; } = "Client";
}
