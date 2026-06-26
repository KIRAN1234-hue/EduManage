namespace SchoolMgmt.Services.Implementations;

// Pure static utility — no DI needed, no state
// Called directly wherever grade calculation is needed
public static class GradeService
{
    public static string CalculateGrade(decimal percentage)
    {
        return percentage switch
        {
            >= 90 => "A+",
            >= 80 => "A",
            >= 70 => "B",
            >= 60 => "C",
            >= 50 => "D",
            _ => "F"
        };
    }

    public static decimal CalculatePercentage(decimal marksObtained, int maxMarks)
    {
        if (maxMarks == 0) return 0;
        return Math.Round(marksObtained / maxMarks * 100, 2);
    }

    // Used for overall report card grade
    public static string GetPassFailStatus(decimal percentage)
        => percentage >= 50 ? "PASS" : "FAIL";
}