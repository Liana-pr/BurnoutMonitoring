namespace BurnoutMonitoring.Models;

public class TestAnswer
{
    public Guid AnswerId { get; set; } = Guid.NewGuid();
    public string SessionHash { get; set; } = string.Empty;
    public Guid QuestionId { get; set; }
    public short Score { get; set; }

    public TestSession Session { get; set; } = null!;
    public Question Question { get; set; } = null!;
}