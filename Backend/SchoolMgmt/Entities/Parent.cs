namespace SchoolMgmt.Entities;

public class Parent
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }
    public ApplicationUser User { get; set; } = null!;

    public string? Occupation { get; set; }
    public string EmergencyContact { get; set; } = string.Empty;
    public decimal? AnnualIncome { get; set; }

    public ICollection<Student> Students { get; set; } = new List<Student>();
}