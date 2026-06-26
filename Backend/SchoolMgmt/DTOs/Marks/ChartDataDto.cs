namespace SchoolMgmt.DTOs.Marks;

// Chart.js compatible format for Angular bar charts
public class ChartDataDto
{
    public List<string> Labels { get; set; } = new();  // Subject names
    public List<ChartDatasetDto> Datasets { get; set; } = new();
}

public class ChartDatasetDto
{
    public string Label { get; set; } = string.Empty;  // Exam type e.g. "Unit Test"
    public List<decimal> Data { get; set; } = new();   // Marks per subject
    public string BackgroundColor { get; set; } = string.Empty;
}