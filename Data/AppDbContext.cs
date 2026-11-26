using BurnoutMonitoring.Models;
using Microsoft.EntityFrameworkCore;
using Npgsql.EntityFrameworkCore.PostgreSQL; // Обязательно добавьте этот using в самом верху, если его нет

namespace BurnoutMonitoring.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    // Справочники
    public DbSet<Department> Departments => Set<Department>();
    public DbSet<Position> Positions => Set<Position>();
    public DbSet<TestMethod> TestMethods => Set<TestMethod>();
    public DbSet<Question> Questions => Set<Question>();
    public DbSet<AppUser> AppUsers => Set<AppUser>();

    // Идентифицирующие данные
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<WorkSchedule> WorkSchedules => Set<WorkSchedule>();

    // Анонимные данные
    public DbSet<TestSession> TestSessions => Set<TestSession>();
    public DbSet<TestAnswer> TestAnswers => Set<TestAnswer>();
    public DbSet<RiskAssessment> RiskAssessments => Set<RiskAssessment>();
    public DbSet<Recommendation> Recommendations => Set<Recommendation>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // 1. Явно указываем схемы и таблицы PostgreSQL
        modelBuilder.Entity<Department>().ToTable("department", "ref");
        modelBuilder.Entity<Position>().ToTable("position", "ref");
        modelBuilder.Entity<TestMethod>().ToTable("test_method", "ref");
        modelBuilder.Entity<Question>().ToTable("question", "ref");
        modelBuilder.Entity<AppUser>().ToTable("app_user", "identity");

        modelBuilder.Entity<Employee>().ToTable("employee", "identity");

        modelBuilder.Entity<WorkSchedule>().ToTable("work_schedule", "identity");

        modelBuilder.Entity<TestSession>().ToTable("test_session", "anonymous");
        modelBuilder.Entity<TestAnswer>().ToTable("test_answer", "anonymous");
        modelBuilder.Entity<RiskAssessment>().ToTable("risk_assessment", "anonymous");
        modelBuilder.Entity<Recommendation>().ToTable("recommendation", "anonymous");

        // 2. Явное указание первичных ключей (для надежности)
        modelBuilder.Entity<Department>().HasKey(d => d.DepartmentId);
        modelBuilder.Entity<Position>().HasKey(p => p.PositionId);
        modelBuilder.Entity<TestMethod>().HasKey(m => m.MethodId);
        modelBuilder.Entity<Question>().HasKey(q => q.QuestionId);
        modelBuilder.Entity<Employee>().HasKey(e => e.EmployeeId);
        modelBuilder.Entity<WorkSchedule>().HasKey(w => w.ScheduleId);
        modelBuilder.Entity<TestSession>().HasKey(t => t.SessionHash);
        modelBuilder.Entity<TestAnswer>().HasKey(t => t.AnswerId);
        modelBuilder.Entity<RiskAssessment>().HasKey(r => r.AssessmentId);
        modelBuilder.Entity<Recommendation>().HasKey(r => r.RecommendationId);

        // 3. Связи и индексы
        // Составной уникальный ключ для ответов (сессия + вопрос)
        modelBuilder.Entity<TestAnswer>()
            .HasIndex(a => new { a.SessionHash, a.QuestionId })
            .IsUnique();
        modelBuilder.Entity<Employee>()
    .HasOne(e => e.User)
    .WithMany(u => u.Employees)
    .HasForeignKey(e => e.UserId);

        // Связь TestAnswer -> TestSession (Один ко многим)
        modelBuilder.Entity<TestAnswer>()
            .HasOne(a => a.Session)
            .WithMany(s => s.Answers)
            .HasForeignKey(a => a.SessionHash);

        // Связь один-к-одному: сессия → оценка риска
        modelBuilder.Entity<RiskAssessment>()
            .HasOne(r => r.Session)
            .WithOne(s => s.RiskAssessment)
            .HasForeignKey<RiskAssessment>(r => r.SessionHash);

        // 4. Специфичные типы данных PostgreSQL
        // Указываем EF Core, что FactorsJson — это JSONB
        modelBuilder.Entity<RiskAssessment>()
            .Property(r => r.FactorsJson)
            .HasColumnType("jsonb");

        // Указываем EF Core, что перечисления — это кастомные ENUM-типы в БД
        modelBuilder.HasPostgresEnum<ShiftType>("ref", "shift_type_enum");
        modelBuilder.HasPostgresEnum<RiskClass>("anonymous", "risk_class_enum");
        modelBuilder.HasPostgresEnum<RecommendationType>("ref", "recommendation_type_enum");
        modelBuilder.HasPostgresEnum<RecommendationStatus>("ref", "recommendation_status_enum");
        modelBuilder.Entity<AppUser>().ToTable("app_user", "identity");
        modelBuilder.Entity<AppUser>().HasKey(u => u.UserId); // Указываем первичный ключ

    }
}