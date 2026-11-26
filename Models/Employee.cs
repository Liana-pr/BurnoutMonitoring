namespace BurnoutMonitoring.Models;

public class Employee
{
    public Guid EmployeeId { get; set; } = Guid.NewGuid();
    public Guid DepartmentId { get; set; }
    public Guid PositionId { get; set; }
    public Guid? UserId { get; set; } // Может быть null, если аккаунт не привязан
    public AppUser? User { get; set; }

    public DateOnly HireDate { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public Department Department { get; set; } = null!;
    public Position Position { get; set; } = null!;
    public ICollection<WorkSchedule> Schedules { get; set; } = new List<WorkSchedule>();
}