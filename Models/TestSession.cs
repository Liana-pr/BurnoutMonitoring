namespace BurnoutMonitoring.Models;

public class TestSession
{
    public string SessionHash { get; set; } = string.Empty;  // SHA-256, не Guid!
    public Guid MethodId { get; set; }
    public DateTime CompletedAt { get; set; } = DateTime.UtcNow;
    public bool IsComplete { get; set; }

    public TestMethod Method { get; set; } = null!;
    public ICollection<TestAnswer> Answers { get; set; } = new List<TestAnswer>();
    public RiskAssessment? RiskAssessment { get; set; }
}