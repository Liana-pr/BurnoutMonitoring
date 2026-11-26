namespace BurnoutMonitoring.Dtos;

public record DashboardResponse(
    DateTime RefreshedAt,
    List<DepartmentAggDto> Departments);

public record DepartmentAggDto(
    Guid DepartmentId,
    string DepartmentName,
    decimal AvgOvertime,
    decimal AvgHours,
    int HighRiskSessions,
    int MediumRiskSessions,
    int LowRiskSessions, 
    int NotPassed);
public class DepartmentAggRaw
{
    public Guid DepartmentId { get; set; }
    public string DepartmentName { get; set; } = string.Empty;
    public decimal AvgOvertime { get; set; }
    public decimal AvgHours { get; set; }
    public int HighRiskSessions { get; set; }
    public int MediumRiskSessions { get; set; }
    public int LowRiskSessions { get; set; }
    public DateTime RefreshedAt { get; set; }
}