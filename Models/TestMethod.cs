namespace BurnoutMonitoring.Models;

public class TestMethod
{
    public Guid MethodId { get; set; } = Guid.NewGuid();
    public string Code { get; set; } = string.Empty;        // 'maslach', 'boyko'
    public string Title { get; set; } = string.Empty;
    public int QuestionCount { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<Question> Questions { get; set; } = new List<Question>();
    public ICollection<TestSession> Sessions { get; set; } = new List<TestSession>();
}