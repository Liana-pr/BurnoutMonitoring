using BurnoutMonitoring.Data;
using BurnoutMonitoring.Dtos;
using BurnoutMonitoring.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization; // не забудьте


namespace BurnoutMonitoring.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DashboardController : ControllerBase
{
    private readonly AppDbContext _db;

    public DashboardController(AppDbContext db)
    {
        _db = db;
    }
    [Authorize(Roles = "ChiefPhysician")] // Только главврач
    [HttpGet]
    public async Task<ActionResult<DashboardResponse>> Get()
    {
        try
        {
            var departments = await _db.Departments.ToListAsync();
            var resultList = new List<DepartmentAggDto>();

            // Считаем общее количество тестов (2 методики)
            int totalTestsPerEmployee = 2;

            foreach (var dept in departments)
            {
                // 1. Считаем сотрудников в отделении
                var employeeCount = await _db.Employees.CountAsync(e => e.DepartmentId == dept.DepartmentId);
                var totalTestsExpected = employeeCount * totalTestsPerEmployee;

                // 2. Считаем риски по этому отделению (все тесты за последние 30 дней)
                // В реальности тут нужна привязка к отделению, но для курсовой считаем общую статистику
                var recentRisks = await _db.RiskAssessments
                    .Where(r => r.AssessedAt >= DateTime.UtcNow.AddDays(-30))
                    .ToListAsync();

                int high = recentRisks.Count(r => r.RiskClass == RiskClass.High);
                int medium = recentRisks.Count(r => r.RiskClass == RiskClass.Medium);
                int low = recentRisks.Count(r => r.RiskClass == RiskClass.Low);

                // 3. Считаем "Нет данных" (те, кто не прошел)
                int passedTests = high + medium + low;
                int notPassed = Math.Max(0, totalTestsExpected - passedTests); // Не меньше 0

                // 4. Считаем нагрузку по отделению
                var employeeIds = await _db.Employees
                    .Where(e => e.DepartmentId == dept.DepartmentId)
                    .Select(e => e.EmployeeId)
                    .ToListAsync();

                var since = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-30));
                var schedules = await _db.WorkSchedules
                    .Where(ws => employeeIds.Contains(ws.EmployeeId) && ws.ShiftDate >= since)
                    .ToListAsync();

                decimal avgOvertime = schedules.Any() ? schedules.Average(s => s.OvertimeHours) : 0;
                decimal avgHours = schedules.Any() ? schedules.Average(s => s.Hours) : 0;

                resultList.Add(new DepartmentAggDto(
                    dept.DepartmentId, dept.Name, avgOvertime, avgHours,
                    high, medium, low, notPassed)); // Добавили notPassed
            }

            return Ok(new DashboardResponse(DateTime.UtcNow, resultList));
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Ошибка сервера: " + ex.Message });
        }
    }
}

// Вспомогательный класс для маппинга строк materialized view
public class DepartmentAggRaw
{
    public Guid DepartmentId { get; set; }
    public string DepartmentName { get; set; } = string.Empty;
    public decimal AvgOvertime { get; set; }
    public decimal AvgHours { get; set; }
    public int HighRiskSessions { get; set; }
    public int MediumRiskSessions { get; set; }
    public int LowRiskSessions { get; set; }
    public DateTime RefreshedAt { get; set; }
}