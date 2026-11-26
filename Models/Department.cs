namespace BurnoutMonitoring.Models;

public class Department
{
    public Guid DepartmentId { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Навигационное свойство: у отделения много сотрудников
    public ICollection<Employee> Employees { get; set; } = new List<Employee>();
}