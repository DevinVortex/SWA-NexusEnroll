namespace NexusEnroll.Core.Entities;

public class Report
{
    public string ReportType { get; init; }
    public string Title { get; init; }
    public DateTime GeneratedDate { get; init; }
    public List<string> Content { get; init; } = new();

    public Report(string reportType, string title)
    {
        ReportType = reportType;
        Title = title;
        GeneratedDate = DateTime.Now;
    }

    public Report(string reportType, string title, DateTime generatedDate)
    {
        ReportType = reportType;
        Title = title;
        GeneratedDate = generatedDate;
    }
}