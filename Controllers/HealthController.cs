using BurnoutMonitoring.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BurnoutMonitoring.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    private readonly AppDbContext _db;

    public HealthController(AppDbContext db)
    {
        _db = db;
    }

    // GET /api/health — проверка, что приложение живо и БД доступна
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        try
        {
            // Простейший запрос: посчитать количество отделений
            var departmentsCount = await _db.Departments.CountAsync();
            return Ok(new
            {
                status = "ok",
                database = "connected",
                departmentsCount,
                timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                status = "error",
                message = ex.Message
            });
        }
    }
}