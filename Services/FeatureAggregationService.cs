using BurnoutMonitoring.Data;
using Microsoft.EntityFrameworkCore;

namespace BurnoutMonitoring.Services;

public interface IFeatureAggregationService
{
    Task<(float AvgOvertime, float NightRatio, float Experience)> GetFeaturesForDepartmentAsync(Guid departmentId);
}

public class FeatureAggregationService : IFeatureAggregationService
{
    private readonly AppDbContext _db;

    public FeatureAggregationService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<(float AvgOvertime, float NightRatio, float Experience)> GetFeaturesForDepartmentAsync(Guid departmentId)
    {
        var since = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-30));

        var schedules = await _db.WorkSchedules
            .Where(ws => ws.ShiftDate >= since)
            .Join(_db.Employees, ws => ws.EmployeeId, e => e.EmployeeId, (ws, e) => new { ws, e })
            .Where(x => x.e.DepartmentId == departmentId)
            .ToListAsync();

        if (schedules.Count == 0)
            return (0f, 0f, 0f);

        var avgOvertime = (float)schedules.Average(x => (double)x.ws.OvertimeHours);
        var nightRatio = (float)schedules.Count(x => x.ws.ShiftType == Models.ShiftType.Night) / schedules.Count;
        var avgExperience = (float)schedules
            .Select(x => (DateTime.UtcNow - x.e.HireDate.ToDateTime(TimeOnly.MinValue)).TotalDays / 365.25)
            .Average();

        return (avgOvertime, nightRatio, avgExperience);
    }
}