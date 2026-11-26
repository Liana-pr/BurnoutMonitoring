namespace BurnoutMonitoring.Models;

public class Question
{
    public Guid QuestionId { get; set; } = Guid.NewGuid();
    public Guid MethodId { get; set; }
    public int Ordinal { get; set; }                         // порядковый номер
    public string Text { get; set; } = string.Empty;
    public string ScaleCode { get; set; } = string.Empty;   // 'exhaustion', 'depersonalization', ...

    public TestMethod Method { get; set; } = null!;
}