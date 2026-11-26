using BurnoutMonitoring.Data;
using BurnoutMonitoring.Dtos;
using BurnoutMonitoring.Ml;
using BurnoutMonitoring.Models;
using BurnoutMonitoring.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BurnoutMonitoring.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TestController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IAnonymizationService _anon;
    private readonly IMlService _ml;
    private readonly IRecommendationService _recommendationService;

    public TestController(AppDbContext db, IAnonymizationService anon, IMlService ml, IRecommendationService recommendationService)
    {
        _db = db;
        _anon = anon;
        _ml = ml;
        _recommendationService = recommendationService;
    }

    // POST /api/test/session — начать тестирование
    [HttpPost("session")]
    public async Task<ActionResult<CreateSessionResponse>> CreateSession([FromBody] CreateSessionRequest request)
    {
        var method = await _db.TestMethods
            .Include(m => m.Questions)
            .FirstOrDefaultAsync(m => m.MethodId == request.MethodId && m.IsActive);

        if (method is null)
            return NotFound(new { message = "Методика не найдена или неактивна" });

        // Генерируем случайный хеш сессии (НЕ от employee_id!)
        var sessionHash = _anon.GenerateSessionHash();

        var session = new TestSession
        {
            SessionHash = sessionHash,
            MethodId = method.MethodId,
            IsComplete = false
        };
        _db.TestSessions.Add(session);
        await _db.SaveChangesAsync();

        var questions = method.Questions
            .OrderBy(q => q.Ordinal)
            .Select(q => new QuestionDto(q.QuestionId, q.Ordinal, q.Text, q.ScaleCode))
            .ToList();

        return Ok(new CreateSessionResponse(sessionHash, questions));
    }

    // POST /api/test/submit — отправить ответы
    [HttpPost("submit")]
    public async Task<ActionResult<SubmitTestResponse>> Submit([FromBody] SubmitTestRequest request)
    {

        // 1. Находим сессию
        var session = await _db.TestSessions
            .Include(s => s.Method)
            .FirstOrDefaultAsync(s => s.SessionHash == request.SessionHash);

        if (session is null)
            return NotFound(new { message = "Сессия не найдена" });

        if (session.IsComplete)
            return BadRequest(new { message = "Сессия уже завершена" });
        // 2. Сохраняем ответы
        foreach (var ans in request.Answers)
        {
            _db.TestAnswers.Add(new TestAnswer
            {
                SessionHash = request.SessionHash,
                QuestionId = ans.QuestionId,
                Score = ans.Score
            });
        }

        session.IsComplete = true;
        session.CompletedAt = DateTime.UtcNow;

        // Сохраняем ответы в базу ПЕРЕД вызовом ML
        await _db.SaveChangesAsync();

        // Считаем сумму баллов
        float totalScore = request.Answers.Any() ? (float)request.Answers.Sum(a => a.Score) : 0f;

        // ПЕРЕДАЕМ СВЕРХУРОЧНЫЕ В ML
        var result = await _ml.AssessRiskAsync(session.SessionHash, totalScore, session.MethodId, request.OvertimeHours);


        // 5. Сохраняем оценку риска (RiskAssessment)
        var assessment = new RiskAssessment
        {
            SessionHash = session.SessionHash,
            RiskClass = result.RiskClass switch
            {
                "high" => RiskClass.High,
                "medium" => RiskClass.Medium,
                _ => RiskClass.Low
            },
            Probability = result.Probability,
            FactorsJson = System.Text.Json.JsonSerializer.Serialize(result.Factors)
        };
        _db.RiskAssessments.Add(assessment);
        await _db.SaveChangesAsync();

        // 6. Генерируем рекомендации
        await _recommendationService.GenerateAsync(assessment.AssessmentId, assessment.RiskClass, result.Factors);

        // 7. Возвращаем результат
        return Ok(new SubmitTestResponse(
            session.SessionHash,
            result.RiskClass,
            result.Probability,
            result.Factors));
    }
    // GET /api/test/risks — получить список всех выявленных рисков (для психолога)
    [Authorize(Roles = "Psychologist")]
    [HttpGet("risks")]
    public async Task<IActionResult> GetAllRisks()
    {
        var risks = await _db.RiskAssessments
            .OrderByDescending(r => r.AssessedAt)
            .Take(50) // Берем последние 50 записей, чтобы не грузить всё сразу
            .Select(r => new
            {
                r.AssessmentId,
                r.SessionHash,
                RiskClass = r.RiskClass.ToString(),
                r.Probability,
                r.AssessedAt
            })
            .ToListAsync();

        return Ok(risks);
    }
}