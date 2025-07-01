using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ankieter.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "surveyAnswers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<int>(type: "INTEGER", nullable: false),
                    QuestionId = table.Column<int>(type: "INTEGER", nullable: false),
                    Answer = table.Column<string>(type: "TEXT", nullable: true),
                    TimeStamp = table.Column<DateTime>(type: "TEXT", nullable: false),
                    SurveyCompleted = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_surveyAnswers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "surveyQuestions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Type = table.Column<int>(type: "INTEGER", nullable: false),
                    TimeStamp = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_surveyQuestions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "surveyUsers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Localization = table.Column<string>(type: "TEXT", nullable: false),
                    Age = table.Column<int>(type: "INTEGER", nullable: false),
                    Sex = table.Column<char>(type: "TEXT", nullable: false),
                    Education = table.Column<int>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_surveyUsers", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "surveyAnswers");

            migrationBuilder.DropTable(
                name: "surveyQuestions");

            migrationBuilder.DropTable(
                name: "surveyUsers");
        }
    }
}
