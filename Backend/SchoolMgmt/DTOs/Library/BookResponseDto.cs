namespace SchoolMgmt.DTOs.Library;

public class BookResponseDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string ISBN { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Publisher { get; set; } = string.Empty;
    public int PublishedYear { get; set; }
    public int TotalCopies { get; set; }
    public int AvailableCopies { get; set; }    
    public string? Description { get; set; }
    public string? ShelfLocation { get; set; }
    public bool IsAvailable => AvailableCopies > 0;
}