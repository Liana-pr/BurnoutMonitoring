using BurnoutMonitoring.Data;
using BurnoutMonitoring.Models;

namespace BurnoutMonitoring.Services;

public interface IRecommendationService
{
    Task<List<Recommendation>> GenerateAsync(Guid assessmentId, RiskClass riskClass, List<string> factors);
}

public class RecommendationService : IRecommendationService
{
    private readonly AppDbContext _db;

    public RecommendationService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<Recommendation>> GenerateAsync(Guid assessmentId, RiskClass riskClass, List<string> factors)
    {
        var recommendations = new List<Recommendation>();

        if (riskClass == RiskClass.High)
        {
            recommendations.Add(new Recommendation
            {
                AssessmentId = assessmentId,
                RecType = RecommendationType.Psychologist,
                Text = "Рекомендуется консультация штатного психолога в течение недели."
            });
        }

        if (factors.Contains("Высокий уровень сверхурочных"))
        {
            recommendations.Add(new Recommendation
            {
                AssessmentId = assessmentId,
                RecType = RecommendationType.ScheduleChange,
                Text = "Пересмотреть график: снизить количество сверхурочных часов на 20% в следующем месяце."
            });
        }

        if (factors.Contains("Частые ночные смены"))
        {
            recommendations.Add(new Recommendation
            {
                AssessmentId = assessmentId,
                RecType = RecommendationType.ScheduleChange,
                Text = "Сократить долю ночных смен до 30% от общего числа."
            });
        }

        if (riskClass == RiskClass.Medium && recommendations.Count == 0)
        {
            recommendations.Add(new Recommendation
            {
                AssessmentId = assessmentId,
                RecType = RecommendationType.Training,
                Text = "Рекомендуется пройти тренинг по управлению стрессом."
            });
        }

        _db.Recommendations.AddRange(recommendations);
        await _db.SaveChangesAsync();
        return recommendations;
    }
}