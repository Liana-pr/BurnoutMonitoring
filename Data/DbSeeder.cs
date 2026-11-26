using BurnoutMonitoring.Models;
using Microsoft.EntityFrameworkCore;

namespace BurnoutMonitoring.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext db)
    {
        if (!await db.Departments.AnyAsync())
        {
            db.Departments.AddRange(
                new Department { Name = "Терапевтическое отделение" },
                new Department { Name = "Хирургическое отделение" },
                new Department { Name = "Отделение реанимации" }
            );
        }

        if (!await db.Positions.AnyAsync())
        {
            db.Positions.AddRange(
                new Position { Title = "Врач" },
                new Position { Title = "Медицинская сестра" },
                new Position { Title = "Заведующий отделением" }
            );
        }

        if (!await db.TestMethods.AnyAsync())
        {
            var maslach = new TestMethod
            {
                Code = "maslach",
                Title = "Опросник выгорания Маслач",
                QuestionCount = 3 // Для теста сделаем 3 вопроса вместо 22
            };

            // Добавляем вопросы к методике
            maslach.Questions.Add(new Question { Ordinal = 1, Text = "Я чувствую эмоциональное истощение", ScaleCode = "exhaustion" });
            maslach.Questions.Add(new Question { Ordinal = 2, Text = "Я стал более равнодушен к пациентам", ScaleCode = "depersonalization" });
            maslach.Questions.Add(new Question { Ordinal = 3, Text = "Я чувствую снижение профессиональной эффективности", ScaleCode = "efficacy" });

            db.TestMethods.Add(maslach);
        }
        await db.SaveChangesAsync();
    }
}