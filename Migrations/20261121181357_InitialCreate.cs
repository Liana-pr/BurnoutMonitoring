using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BurnoutMonitoring.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "ref");

            migrationBuilder.EnsureSchema(
                name: "identity");

            migrationBuilder.EnsureSchema(
                name: "anonymous");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:anonymous.risk_class_enum", "low,medium,high")
                .Annotation("Npgsql:Enum:ref.recommendation_status_enum", "issued,accepted,declined,completed")
                .Annotation("Npgsql:Enum:ref.recommendation_type_enum", "schedule_change,psychologist,training")
                .Annotation("Npgsql:Enum:ref.shift_type_enum", "day,night,mixed");

            migrationBuilder.CreateTable(
                name: "department",
                schema: "ref",
                columns: table => new
                {
                    DepartmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_department", x => x.DepartmentId);
                });

            migrationBuilder.CreateTable(
                name: "position",
                schema: "ref",
                columns: table => new
                {
                    PositionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_position", x => x.PositionId);
                });

            migrationBuilder.CreateTable(
                name: "test_method",
                schema: "ref",
                columns: table => new
                {
                    MethodId = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "text", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: false),
                    QuestionCount = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_test_method", x => x.MethodId);
                });

            migrationBuilder.CreateTable(
                name: "employee",
                schema: "identity",
                columns: table => new
                {
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    DepartmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    PositionId = table.Column<Guid>(type: "uuid", nullable: false),
                    HireDate = table.Column<DateOnly>(type: "date", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_employee", x => x.EmployeeId);
                    table.ForeignKey(
                        name: "FK_employee_department_DepartmentId",
                        column: x => x.DepartmentId,
                        principalSchema: "ref",
                        principalTable: "department",
                        principalColumn: "DepartmentId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_employee_position_PositionId",
                        column: x => x.PositionId,
                        principalSchema: "ref",
                        principalTable: "position",
                        principalColumn: "PositionId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "question",
                schema: "ref",
                columns: table => new
                {
                    QuestionId = table.Column<Guid>(type: "uuid", nullable: false),
                    MethodId = table.Column<Guid>(type: "uuid", nullable: false),
                    Ordinal = table.Column<int>(type: "integer", nullable: false),
                    Text = table.Column<string>(type: "text", nullable: false),
                    ScaleCode = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_question", x => x.QuestionId);
                    table.ForeignKey(
                        name: "FK_question_test_method_MethodId",
                        column: x => x.MethodId,
                        principalSchema: "ref",
                        principalTable: "test_method",
                        principalColumn: "MethodId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "test_session",
                schema: "anonymous",
                columns: table => new
                {
                    SessionHash = table.Column<string>(type: "text", nullable: false),
                    MethodId = table.Column<Guid>(type: "uuid", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsComplete = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_test_session", x => x.SessionHash);
                    table.ForeignKey(
                        name: "FK_test_session_test_method_MethodId",
                        column: x => x.MethodId,
                        principalSchema: "ref",
                        principalTable: "test_method",
                        principalColumn: "MethodId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "work_schedule",
                schema: "identity",
                columns: table => new
                {
                    ScheduleId = table.Column<Guid>(type: "uuid", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    ShiftDate = table.Column<DateOnly>(type: "date", nullable: false),
                    ShiftType = table.Column<int>(type: "integer", nullable: false),
                    Hours = table.Column<decimal>(type: "numeric", nullable: false),
                    OvertimeHours = table.Column<decimal>(type: "numeric", nullable: false),
                    PatientsCount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_work_schedule", x => x.ScheduleId);
                    table.ForeignKey(
                        name: "FK_work_schedule_employee_EmployeeId",
                        column: x => x.EmployeeId,
                        principalSchema: "identity",
                        principalTable: "employee",
                        principalColumn: "EmployeeId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "risk_assessment",
                schema: "anonymous",
                columns: table => new
                {
                    AssessmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    SessionHash = table.Column<string>(type: "text", nullable: false),
                    RiskClass = table.Column<int>(type: "integer", nullable: false),
                    Probability = table.Column<decimal>(type: "numeric", nullable: false),
                    FactorsJson = table.Column<string>(type: "text", nullable: false),
                    AssessedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_risk_assessment", x => x.AssessmentId);
                    table.ForeignKey(
                        name: "FK_risk_assessment_test_session_SessionHash",
                        column: x => x.SessionHash,
                        principalSchema: "anonymous",
                        principalTable: "test_session",
                        principalColumn: "SessionHash",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "test_answer",
                schema: "anonymous",
                columns: table => new
                {
                    AnswerId = table.Column<Guid>(type: "uuid", nullable: false),
                    SessionHash = table.Column<string>(type: "text", nullable: false),
                    QuestionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Score = table.Column<short>(type: "smallint", nullable: false),
                    SessionHash1 = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_test_answer", x => x.AnswerId);
                    table.ForeignKey(
                        name: "FK_test_answer_question_QuestionId",
                        column: x => x.QuestionId,
                        principalSchema: "ref",
                        principalTable: "question",
                        principalColumn: "QuestionId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_test_answer_test_session_SessionHash1",
                        column: x => x.SessionHash1,
                        principalSchema: "anonymous",
                        principalTable: "test_session",
                        principalColumn: "SessionHash",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "recommendation",
                schema: "anonymous",
                columns: table => new
                {
                    RecommendationId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssessmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    RecType = table.Column<int>(type: "integer", nullable: false),
                    Text = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    IssuedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_recommendation", x => x.RecommendationId);
                    table.ForeignKey(
                        name: "FK_recommendation_risk_assessment_AssessmentId",
                        column: x => x.AssessmentId,
                        principalSchema: "anonymous",
                        principalTable: "risk_assessment",
                        principalColumn: "AssessmentId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_employee_DepartmentId",
                schema: "identity",
                table: "employee",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_employee_PositionId",
                schema: "identity",
                table: "employee",
                column: "PositionId");

            migrationBuilder.CreateIndex(
                name: "IX_question_MethodId",
                schema: "ref",
                table: "question",
                column: "MethodId");

            migrationBuilder.CreateIndex(
                name: "IX_recommendation_AssessmentId",
                schema: "anonymous",
                table: "recommendation",
                column: "AssessmentId");

            migrationBuilder.CreateIndex(
                name: "IX_risk_assessment_SessionHash",
                schema: "anonymous",
                table: "risk_assessment",
                column: "SessionHash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_test_answer_QuestionId",
                schema: "anonymous",
                table: "test_answer",
                column: "QuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_test_answer_SessionHash_QuestionId",
                schema: "anonymous",
                table: "test_answer",
                columns: new[] { "SessionHash", "QuestionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_test_answer_SessionHash1",
                schema: "anonymous",
                table: "test_answer",
                column: "SessionHash1");

            migrationBuilder.CreateIndex(
                name: "IX_test_session_MethodId",
                schema: "anonymous",
                table: "test_session",
                column: "MethodId");

            migrationBuilder.CreateIndex(
                name: "IX_work_schedule_EmployeeId",
                schema: "identity",
                table: "work_schedule",
                column: "EmployeeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "recommendation",
                schema: "anonymous");

            migrationBuilder.DropTable(
                name: "test_answer",
                schema: "anonymous");

            migrationBuilder.DropTable(
                name: "work_schedule",
                schema: "identity");

            migrationBuilder.DropTable(
                name: "risk_assessment",
                schema: "anonymous");

            migrationBuilder.DropTable(
                name: "question",
                schema: "ref");

            migrationBuilder.DropTable(
                name: "employee",
                schema: "identity");

            migrationBuilder.DropTable(
                name: "test_session",
                schema: "anonymous");

            migrationBuilder.DropTable(
                name: "department",
                schema: "ref");

            migrationBuilder.DropTable(
                name: "position",
                schema: "ref");

            migrationBuilder.DropTable(
                name: "test_method",
                schema: "ref");
        }
    }
}
