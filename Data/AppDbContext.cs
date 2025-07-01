using Microsoft.EntityFrameworkCore;
using Ankieter.Models;

namespace Ankieter.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<SurveyQuestion> surveyQuestions { get; set; }
        public DbSet<SurveyAnswer> surveyAnswers { get; set; }
        public DbSet<SurveyUser> surveyUsers { get; set; }
    }
}