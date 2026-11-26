using BurnoutMonitoring.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.ML;

namespace BurnoutMonitoring.Ml;

public interface IMlService
{
    Task<MlAssessmentResult> AssessRiskAsync(string sessionHash, float totalScore, Guid methodId, float overtimeHours);
    Task TrainModelAsync();
}
public record MlAssessmentResult(string RiskClass, decimal Probability, List<string> Factors);

public class MlService : IMlService
{
    private readonly AppDbContext _db;
    private readonly ILogger<MlService> _logger;
    private readonly MLContext _mlContext = new(seed: 42);
    private ITransformer? _model;
    private PredictionEngine<RiskFeatures, RiskPrediction>? _engine;

    public MlService(AppDbContext db, ILogger<MlService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task TrainModelAsync()
    {
        try
        {
            _logger.LogInformation("Начинаю обучение ML-модели на числовых данных...");
            string dataPath = Path.Combine(AppContext.BaseDirectory, "training_data.csv");
            var dataView = _mlContext.Data.LoadFromTextFile<RiskFeatures>(dataPath, hasHeader: true, separatorChar: ',');

            var pipeline = _mlContext.Transforms.Conversion.MapValueToKey("Label")
                // Просто объединяем два числовых признака
                .Append(_mlContext.Transforms.Concatenate("Features",
                    nameof(RiskFeatures.AvgTestScore),
                    nameof(RiskFeatures.TestCode)))
                // Используем быстрый алгоритм
                .Append(_mlContext.MulticlassClassification.Trainers.LbfgsMaximumEntropy())
                .Append(_mlContext.Transforms.Conversion.MapKeyToValue("PredictedLabel"));

            _model = pipeline.Fit(dataView);
            _engine = _mlContext.Model.CreatePredictionEngine<RiskFeatures, RiskPrediction>(_model);
            _logger.LogInformation("ML-модель успешно обучена!");
        }
        catch (Exception ex) { _logger.LogError($"Ошибка ML: {ex.Message}"); }
        await Task.CompletedTask;
    }

    public async Task<MlAssessmentResult> AssessRiskAsync(string sessionHash, float totalScore, Guid methodId, float overtimeHours)
    {
        if (_engine == null) await TrainModelAsync();

        var method = await _db.TestMethods.FindAsync(methodId);
        float testCode = (method?.Code == "boyko") ? 1f : 0f;

        if (_engine != null)
        {
            try
            {
                var features = new RiskFeatures { AvgTestScore = totalScore, TestCode = testCode, OvertimeHours = overtimeHours };
                var prediction = _engine.Predict(features);

                _logger.LogInformation($"[ML] Баллы: {totalScore}, Тест: {testCode}, Сверхурочные: {overtimeHours}, Предсказание: {prediction.PredictedLabel}");

                // 1. Получаем исходный прогноз от ML
                string resultLabel = prediction.PredictedLabel ?? "low_none";

                // 2. ПРИМЕНЯЕМ УМНОЕ ПРАВИЛО (меняем resultLabel ДО определения riskClass)
                if (resultLabel.Contains("low") && overtimeHours >= 10)
                {
                    resultLabel = "medium_schedule"; // Принудительно меняем
                    _logger.LogInformation($"[ML+Правило] Сверхурочные ({overtimeHours}) повысили риск до medium_schedule");
                }
                else if ((resultLabel.Contains("medium") || resultLabel.Contains("high")) && overtimeHours >= 20 && !resultLabel.Contains("schedule") && !resultLabel.Contains("both"))
                {
                    resultLabel = resultLabel.Contains("high") ? "high_both" : "medium_schedule";
                }

                // 3. ТЕПЕРЬ определяем riskClass на основе (возможно измененного) resultLabel
                string riskClass = "low";
                if (resultLabel.Contains("high")) riskClass = "high";
                else if (resultLabel.Contains("medium")) riskClass = "medium";

                // 4. Формируем рекомендации
                var recommendations = new List<string>();

                if (resultLabel.Contains("psy")) recommendations.Add("Рекомендуется консультация психолога");
                if (resultLabel.Contains("schedule")) recommendations.Add("Рекомендуется пересмотреть график (снизить сверхурочные)");

                if (resultLabel == "high_both")
                {
                    recommendations.Clear();
                    recommendations.Add("Настоятельно рекомендуется консультация психолога");
                    if (overtimeHours > 0)
                    {
                        recommendations.Add("Необходимо снизить количество сверхурочных часов");
                    }
                }

                if (resultLabel == "low_none") recommendations.Add("Уровень выгорания в норме. Отдыхайте больше!");

                decimal probability = riskClass == "high" ? 0.85m : (riskClass == "medium" ? 0.55m : 0.2m);

                return new MlAssessmentResult(riskClass, probability, recommendations);
            }
            catch (Exception ex) { _logger.LogError($"Ошибка ML: {ex.Message}"); }
        }

        return new MlAssessmentResult("low", 0.2m, new List<string> { "ML-модель недоступна" });
    }
}