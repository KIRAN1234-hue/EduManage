using Microsoft.AspNetCore.Identity;
using SchoolMgmt.Enums;

namespace SchoolMgmt.Entities;

public class ApplicationUser : IdentityUser<Guid>
{
    public string FullName { get; set; } = string.Empty;
    public RoleType RoleType { get; set; }
    public bool IsActive { get; set; } = true;
    public string? ProfilePhotoUrl { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Only one of these will ever have a value per user
    public Student? Student { get; set; }
    public Teacher? Teacher { get; set; }
    public Parent? Parent { get; set; }
}