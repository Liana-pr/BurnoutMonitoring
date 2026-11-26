namespace BurnoutMonitoring.Models;

public class Position
{
    public Guid PositionId { get; set; } = Guid.NewGuid();
    public string Title { get; set; } = string.Empty;

    public ICollection<Employee> Employees { get; set; } = new List<Employee>();
}