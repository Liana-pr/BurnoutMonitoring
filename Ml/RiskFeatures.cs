using Microsoft.ML.Data;

namespace BurnoutMonitoring.Ml;

public class RiskFeatures
{
    [LoadColumn(0)] public float AvgTestScore { get; set; }

    // Название теста (maslach или boyko)
    [LoadColumn(1)] public float TestCode { get; set; } // Теперь это число!
    [LoadColumn(2)] public float OvertimeHours { get; set; } // Новый признак!
    // Целевая переменная (low, medium, high)
    [LoadColumn(3), ColumnName("Label")]

    public string RiskLabel { get; set; } = string.Empty;
}

public class RiskPrediction
{
    [ColumnName("PredictedLabel")] public string PredictedLabel { get; set; } = string.Empty;
    public float[] Score { get; set; } = Array.Empty<float>();
}