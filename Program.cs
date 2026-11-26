using BurnoutMonitoring.Data;
using BurnoutMonitoring.Ml;
using BurnoutMonitoring.Models;
using BurnoutMonitoring.Services;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Npgsql.EntityFrameworkCore.PostgreSQL;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// 1. Создаем конструктор источника данных и настраиваем маппинг enum-ов для PostgreSQL
var dataSourceBuilder = new NpgsqlDataSourceBuilder(
    builder.Configuration.GetConnectionString("DefaultConnection")
);

dataSourceBuilder.MapEnum<ShiftType>("ref.shift_type_enum");
dataSourceBuilder.MapEnum<RiskClass>("anonymous.risk_class_enum");
dataSourceBuilder.MapEnum<RecommendationType>("ref.recommendation_type_enum");
dataSourceBuilder.MapEnum<RecommendationStatus>("ref.recommendation_status_enum");

// 2. Собираем источник данных
var dataSource = dataSourceBuilder.Build();

// 3. Регистрируем DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(dataSource, o =>
    {
        o.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
    })
    .UseSnakeCaseNamingConvention() // Автоматический перевод PascalCase -> snake_case
);

// 4. Регистрируем наши собственные сервисы (Dependency Injection)
builder.Services.AddSingleton<IAnonymizationService, AnonymizationService>();
builder.Services.AddScoped<IMlService, MlService>();
builder.Services.AddScoped<IFeatureAggregationService, FeatureAggregationService>();
builder.Services.AddScoped<IRecommendationService, RecommendationService>();

// 5. Регистрируем стандартные сервисы ASP.NET Core
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder => builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});
builder.Services.AddControllers(); // <--- ВОТ ОНА, ТА САМАЯ ПРОПАВШАЯ СТРОКА
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var jwtKey = builder.Configuration["Jwt:Key"] ?? "SuperSecretKeyForBurnoutMonitoringApp2026!";
var key = Encoding.ASCII.GetBytes(jwtKey);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = false,
        ValidateAudience = false
    };
});

builder.Services.AddAuthorization(); builder.Services.AddAuthorization();

var app = builder.Build();
app.UseCors("AllowAll");

// 6. Заполняем базу начальными данными (сидер)

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await DbSeeder.SeedAsync(db);

    // ВОТ ЭТА СТРОКА ЗАСТАВИТ МОДЕЛЬ ОБУЧИТЬСЯ ПРИ ЗАПУСКЕ
    var ml = scope.ServiceProvider.GetRequiredService<IMlService>();
    await ml.TrainModelAsync();
}

// 7. Настройка конвейера обработки запросов (Middleware)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseDefaultFiles(); // Позволяет открывать index.html по умолчанию
app.UseStaticFiles();  // Разрешает отдавать картинки, css и js
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();