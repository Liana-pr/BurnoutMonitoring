namespace BurnoutMonitoring.Models;

public enum RiskClass { Low, Medium, High }

public class RiskAssessment
{
    public Guid AssessmentId { get; set; } = Guid.NewGuid();
    public string SessionHash { get; set; } = string.Empty;
    public RiskClass RiskClass { get; set; }
    public decimal Probability { get; set; }
    public string FactorsJson { get; set; } = "{}";   // JSONB в БД
    public DateTime AssessedAt { get; set; } = DateTime.UtcNow;

    public TestSession Session { get; set; } = null!;
    public ICollection<Recommendation> Recommendations { get; set; } = new List<Recommendation>();
}