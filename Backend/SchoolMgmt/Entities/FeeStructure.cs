namespace SchoolMgmt.Entities;

public class FeeStructure
{
    public Guid Id { get; set; }

    public Guid ClassId { get; set; }
    public Class Class { get; set; } = null!;

    public string TermName { get; set; } = string.Empty;   // Term1, Term2, Term3
    public decimal Amount { get; set; }
    public DateOnly DueDate { get; set; }
    public string AcademicYear { get; set; } = string.Empty;
    public bool DiscountAllowed { get; set; } = false;

    public ICollection<FeePayment> FeePayments { get; set; } = new List<FeePayment>();
    public string Description { get; set; } = string.Empty;
}