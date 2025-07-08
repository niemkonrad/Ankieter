using Ankieter.Models;
using Ankieter.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using System.Diagnostics;
using System.Globalization;
using System.Xml.Linq;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Data.SqlClient;
using Microsoft.AspNetCore.Components;
using System.Security.AccessControl;

// Do zrobienia :
// - przed pierwszym pytaniem wywietl pytania dotyczace danych demograficznych wraz z walidacja
// - utworz usera który wykonuje ankiete i zapisz go w bazie danych
// - ekran logowania dla admina
// - wyswietlaj odpowiedni sposob odpowiedzi dla danego typu pytania
// - u¿ywaj tylko pytañ "isActive" w ankiecie
// - przygotuj gotowe szablony pytañ - automatyczne wybór odpowiedniego zestawu pytañ (admin)
// - przygotuj ekran wyswietlania odpowiedzi na pytania wraz z filtrem (admin)
// - przygotuj ekran ukazujacy dane demograficzne respondentów (admin)
// - eksport wyników do csv (admin)
// - wyswietl postep w ankiecie
// - mozliwosc powrotu do poprzedniego pytania
// - Tryb testowy (demo) dla administratora
// - Modu³ A/B testów – testuj ró¿ne wersje pytañ
// - Obsluga wielu jezykow



namespace Ankieter.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        var surveyQuestionsViewModel = GetAllQuestions();
        return View(surveyQuestionsViewModel);
    }

    public IActionResult Privacy()
    {
        return View();
    }
    public IActionResult Admin()
    {
        var surveyQuestionsViewModel = GetAllQuestions();
       
        return View(surveyQuestionsViewModel);
    }
    public IActionResult Admin2()
    {
        
        var surveyAnswersWithUsers = GetFiltredAnswers("", "");
        return View("Admin2",surveyAnswersWithUsers);
    }

    private static List<SurveyAnswer> userAnswers = new();
    public IActionResult User(int index =0)
    {
        var questions = GetAllQuestions();

        if (index >= questions.QuestionsList.Count)
        {
            MarkAllAnswersAsCompleted(userAnswers);
            // Zapisz wszystkie odpowiedzi do bazy
            SaveAnswersToDatabase(userAnswers);
            userAnswers.Clear(); // Czyœæ po zapisie
            ViewBag.SurveyFinished = true;
            return View("UserEnd");
            
        }

        var question = questions.QuestionsList[index];

        var viewModel = new SurveyAnswerViewModel
        {
            Answer = new SurveyAnswer
            {
                QuestionId = question.Id,
                TimeStamp = DateTime.Now,
                UserId = 1 // tymczasowo, docelowo pobieraj z sesji/logowania
            }
        };
        ViewBag.QuestionText = question.Name;
        ViewBag.Index = index;
        return View("User", viewModel);


       
    }

    [HttpPost]
    public IActionResult User(SurveyAnswerViewModel model, int index)
    {
        userAnswers.Add(model.Answer);
        return RedirectToAction("User", new { index = index + 1 });
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    public IActionResult goToAdminPage()
    {
        return RedirectToAction("Admin", "Ankieter");
    }
    public IActionResult goToUserPage()
    {
        return RedirectToAction("User", "Ankieter");
    }

    private void MarkAllAnswersAsCompleted(List<SurveyAnswer> _userAnswers)
    {   foreach (var answer in _userAnswers)
        {
            if (answer.Answer != null)
                answer.SurveyCompleted = true;
        }
    }
  


    private void SaveAnswersToDatabase(List<SurveyAnswer> answers)
    {
        using var con = new SqliteConnection("Data Source=Survey.sqlite");
        using var cmd = con.CreateCommand();
        con.Open();
        foreach (var answer in answers)
        {

            Console.WriteLine(answer.UserId);
            Console.WriteLine(answer.QuestionId);
            Console.WriteLine(answer.Answer);
            Console.WriteLine(answer.TimeStamp);
            Console.WriteLine(answer.SurveyCompleted);
            cmd.Parameters.Clear();
            cmd.CommandText = @"INSERT INTO surveyAnswers (UserId, QuestionId, Answer, TimeStamp, SurveyCompleted) 
                        VALUES (@UserId, @QuestionId, @Answer, @TimeStamp, @SurveyCompleted)";

            cmd.Parameters.AddWithValue("@UserId", answer.UserId);
            cmd.Parameters.AddWithValue("@QuestionId", answer.QuestionId);
            cmd.Parameters.AddWithValue("@Answer", answer.Answer ?? "");
            cmd.Parameters.AddWithValue("@TimeStamp", answer.TimeStamp);
            cmd.Parameters.AddWithValue("@SurveyCompleted", answer.SurveyCompleted);

            try
            {
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"B³¹d zapisu odpowiedzi: {ex.Message}");
            }
        }
    }


    internal SurveyQuestionsViewModel GetAllQuestions()
    {
        
        List<SurveyQuestion> questionsList = new();
        using (SqliteConnection con =
                new SqliteConnection("Data Source=Survey.sqlite"))
        {

            using (var tableCmd = con.CreateCommand())
            {

                con.Open();
                tableCmd.CommandText = "SELECT * FROM surveyQuestions";

                using (var reader = tableCmd.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {

                            SurveyQuestion surveyQuestion = new SurveyQuestion
                            {
                                Id = reader.GetInt32(0),
                                Name = reader.GetString(1),
                                Type = reader.GetInt32(2),
                                TimeStamp = reader.GetDateTime(3),
                                IsActive = reader.GetBoolean(4)
                            };
                            questionsList.Add(surveyQuestion);
                        }

                    }

                    // return new SurveyQuestionsViewModel { QuestionsList = questionsList };
                }

            }

        }
        return new SurveyQuestionsViewModel { QuestionsList = questionsList };

    }


    internal SurveyAnswerWithUserViewModel GetFiltredAnswers(string filtr, string value)
    {
        int prevUserId = -1;
        SurveyAnswerWithUser existingAnswerWithUser = new SurveyAnswerWithUser();
        List<SurveyAnswerWithUser> allAnswersWithUser = new();
        
        using (SqliteConnection con =
                new SqliteConnection("Data Source=Survey.sqlite"))
        {

            using (var tableCmd = con.CreateCommand())
            {

                con.Open();
                string whereClause = (string.IsNullOrEmpty(filtr) || string.IsNullOrEmpty(value))
      ? ""
      : filtr.ToLower() switch
      {
          "questionid" => "WHERE sa.QuestionId = @value",
          "userid" => "WHERE sa.UserId = @value",
          "age" => "WHERE su.Age = @value",
          "sex" => "WHERE su.Sex = @value",
          "localization" => "WHERE su.Localization = @value",
          _ => ""
      };
                tableCmd.CommandText = $@"
                SELECT sa.Id, sa.UserId, sa.QuestionId, sa.Answer, sa.TimeStamp, sa.SurveyCompleted,
                       su.Age, su.Sex, su.Localization, su.Name, su.Education
                FROM surveyAnswers sa
                JOIN surveyUsers su ON sa.UserId = su.Id
                {whereClause}";
                if (!string.IsNullOrEmpty(whereClause))
                {
                    tableCmd.Parameters.AddWithValue("@value", value);
                }
               

                using (var reader = tableCmd.ExecuteReader())
                {
                    Console.WriteLine("SQL: " + tableCmd.CommandText);
                    Console.WriteLine("Value: " + value);
                    while (reader.Read())
                    {
                        var answerWithUser = new SurveyAnswerWithUser
                        {
                            Answer = new SurveyAnswer
                            {
                                Id = reader.GetInt32(0),
                                
                                Answer = reader.IsDBNull(3) ? null : reader.GetString(3),
                                SurveyCompleted = reader.GetBoolean(5),
                                TimeStamp = reader.GetDateTime(4),
                                QuestionId = reader.GetInt32(2)
                            },
                            User = new SurveyUser
                            { 

                                Id = reader.GetInt32(1),
                                Education = reader.GetInt32(10),
                                Name = reader.GetString(9),
                                Age = reader.GetInt32(6),
                                Sex = reader.GetChar(7),
                                Localization = reader.GetString(8)
                            }
                            
                        };
                        Console.WriteLine(answerWithUser.Answer.Id);
                        if (prevUserId == answerWithUser.User.Id)
                        {
                            // Jeœli u¿ytkownik ju¿ istnieje, dodaj tylko odpowiedŸ
                            existingAnswerWithUser.AnswersList.Add(answerWithUser.Answer);
                        }
                        else
                        {
                            if (existingAnswerWithUser.User != null && !allAnswersWithUser.Contains(existingAnswerWithUser))
                            {
                                allAnswersWithUser.Add(existingAnswerWithUser);
                            }

                            existingAnswerWithUser = new SurveyAnswerWithUser
                            {
                                User = answerWithUser.User,
                                AnswersList = new List<SurveyAnswer> { answerWithUser.Answer }

                            };
                           

                            // Jeœli to nowy u¿ytkownik, dodaj go do listy
                            prevUserId = answerWithUser.User.Id;
                            
                        }
                            
                    }

                    if (existingAnswerWithUser.User != null)
                    {
                        allAnswersWithUser.Add(existingAnswerWithUser);
                    }

                }

            }
        }

        return new SurveyAnswerWithUserViewModel { AnswersWithUserList = allAnswersWithUser };

    }

  

    internal List<SurveyUser> GetFilteredUsers(string filtr, string wartosc)
    {
        List<SurveyUser> users = new();

        using (var con = new SqliteConnection("Data Source=Survey.sqlite"))
        {
            using (var tableCmd = con.CreateCommand())
            {
                con.Open();

                string whereClause = filtr.ToLower() switch
                {
                    "Age" => "WHERE Age = @value",
                    "Sex" => "WHERE Sex = @value",
                    "Localization" => "WHERE Localization = @value",
                    _ => ""
                };

                if (string.IsNullOrEmpty(whereClause))
                    return users;

                tableCmd.CommandText = $"SELECT Id, Localization, Age, Sex, Education, Name FROM surveyUsers {whereClause}";
                tableCmd.Parameters.AddWithValue("@value", wartosc);

                using (var reader = tableCmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        users.Add(new SurveyUser
                        {
                            Id = reader.GetInt32(0),
                            Localization = reader.GetString(1),
                            Age = reader.GetInt32(2),
                            Sex = reader.GetChar(3),
                            Education = reader.GetInt32(4),
                            Name = reader.GetString(5)
                        });
                    }
                }
            }
        }

        return users;
    }


    public RedirectResult Insert(SurveyQuestionsViewModel surveyQuestionModel)
    {

        using (SqliteConnection con =
                new SqliteConnection("Data Source=Survey.sqlite"))
        {

            using (var tableCmd = con.CreateCommand())
            {

                con.Open();
                tableCmd.CommandText = @"INSERT INTO surveyQuestions (name, type, timestamp, isactive) Values (@name,@type,@timestamp,@isActive)";
                tableCmd.Parameters.AddWithValue("@name", surveyQuestionModel.Question.Name);
                tableCmd.Parameters.AddWithValue("@type", surveyQuestionModel.Question.Type);
                tableCmd.Parameters.AddWithValue("@timestamp", surveyQuestionModel.Question.TimeStamp);
                tableCmd.Parameters.AddWithValue("@isActive", surveyQuestionModel.Question.IsActive);

                try
                {
                    tableCmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error inserting data: {ex.Message}");
                }

            }

        }

        return Redirect("/home/admin"); // check ?
    }

    public RedirectResult Update(SurveyQuestionsViewModel surveyQuestionModel)
    {

        using (SqliteConnection con =
                new SqliteConnection("Data Source=Survey.sqlite"))
        {

            using (var tableCmd = con.CreateCommand())
            {

                con.Open();
                tableCmd.CommandText = @"UPDATE surveyQuestions SET name = @name, isActive = @isActive, timestamp = @timeStamp WHERE Id = @id";
                tableCmd.Parameters.AddWithValue("@name", surveyQuestionModel.Question.Name);
                tableCmd.Parameters.AddWithValue("@isActive", surveyQuestionModel.Question.IsActive);
                tableCmd.Parameters.AddWithValue("@timeStamp", DateTime.Now);
                tableCmd.Parameters.AddWithValue("@id", surveyQuestionModel.Question.Id);
                try
                {

                    tableCmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error updating data: {ex.Message}");
                }

            }

        }

        return Redirect("/home/admin");
    }
    [HttpPost]
    public JsonResult Delete(int id)
    {
        using (SqliteConnection con =
               new SqliteConnection("Data Source=Survey.sqlite"))
        {
            using (var tableCmd = con.CreateCommand())
            {
                con.Open();
                tableCmd.CommandText = @"Delete from surveyQuestions WHERE Id = @id";
                tableCmd.Parameters.AddWithValue("@id", id);
                tableCmd.ExecuteNonQuery();

            }
        }

        return Json(new { });
    }
    internal SurveyQuestion GetById(int id)
    {
        SurveyQuestion surveyQuestion = new SurveyQuestion();

        using (var connection =
                new SqliteConnection("Data Source=Survey.sqlite"))
        {
            using (var tableCmd = connection.CreateCommand())
            {
                connection.Open();
                tableCmd.CommandText = $"SELECT * FROM surveyQuestions WHERE Id = '{id}'";

                using (var reader = tableCmd.ExecuteReader())
                {

                    if (reader.Read())
                    {

                        surveyQuestion.Id = reader.GetInt32(0);
                        surveyQuestion.Name = reader.GetString(1);
                        surveyQuestion.Type = reader.GetInt32(2);
                        surveyQuestion.TimeStamp = reader.GetDateTime(3);
                        surveyQuestion.IsActive = reader.GetInt32(4) == 1;

                    }

                    return surveyQuestion;
                }
                ;
            }
        }

    }
    [HttpGet]
    public JsonResult PopulateForm(int id)
    {
        Console.WriteLine("populate");
        var surveyQuestion = GetById(id);
        return Json(surveyQuestion);
    }

    [HttpGet]
    public IActionResult GetQuestion(int index)
    {
        var surveyQuestionModel = GetAllQuestions();

        if (index < 0 || index >= surveyQuestionModel.QuestionsList.Count)
            return Json(null);

        var question = surveyQuestionModel.QuestionsList[index];

        // Dostosuj zwracany JSON, jeœli pytanie ma np. inne pola
        return Json(new
        {
            text = question.Name,   // u¿yj w³aœciwoœci, która jest pytaniem
            
        });
    }
    [HttpGet]
    public IActionResult FiltredAnswers(string filtr, string value)
    {
        var model = GetFiltredAnswers(filtr, value);
        return View();
    }

}