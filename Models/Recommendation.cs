namespace BurnoutMonitoring.Models;

public enum RecommendationType { ScheduleChange, Psychologist, Training }
public enum RecommendationStatus { Issued, Accepted, Declined, Completed }

public class Recommendation
{
    public Guid RecommendationId { get; set; } = Guid.NewGuid();
    public Guid AssessmentId { get; set; }
    public RecommendationType RecType { get; set; }
    public string Text { get; set; } = string.Empty;
    public RecommendationStatus Status { get; set; } = RecommendationStatus.Issued;
    public DateTime IssuedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public RiskAssessment Assessment { get; set; } = null!;
}