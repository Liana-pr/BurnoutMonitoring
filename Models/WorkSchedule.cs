namespace BurnoutMonitoring.Models;

public enum ShiftType { Day, Night, Mixed }

public class WorkSchedule
{
    public Guid ScheduleId { get; set; } = Guid.NewGuid();
    public Guid EmployeeId { get; set; }
    public DateOnly ShiftDate { get; set; }
    public ShiftType ShiftType { get; set; }
    public decimal Hours { get; set; }
    public decimal OvertimeHours { get; set; }
    public int PatientsCount { get; set; }

    public Employee Employee { get; set; } = null!;
}