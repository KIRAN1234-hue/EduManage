namespace SchoolMgmt.Entities;

public class RefreshToken
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }
    public ApplicationUser User { get; set; } = null!;

    // The actual token value — random 64-byte base64 string
    public string Token { get; set; } = string.Empty;

    public DateTime ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Set to true on logout or when replaced by a new refresh token
    public bool IsRevoked { get; set; } = false;

    public string? CreatedByIp { get; set; }
}