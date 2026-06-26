namespace SchoolMgmt.DTOs.Fee;

public class StudentBalanceDto
{
    public decimal TotalAmount { get; set; }
    public decimal Paid { get; set; }
    public decimal Remaining { get; set; }
    public bool IsFullyPaid { get; set; }
}