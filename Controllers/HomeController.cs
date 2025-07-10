using Ankieter.Models;
using Ankieter.Models.ViewModels;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration.UserSecrets;
using System;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Globalization;
using System.Security.AccessControl;
using System.Xml.Linq;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using static System.Runtime.InteropServices.JavaScript.JSType;

// Do zrobienia :
// - przed pierwszym pytaniem wywietl pytania dotyczace danych demograficznych wraz z walidacja
// - utworz usera kt�ry wykonuje ankiete i zapisz go w bazie danych
// - ekran logowania dla admina
// - wyswietlaj odpowiedni sposob odpowiedzi dla danego typu pytania
// - u�ywaj tylko pyta� "isActive" w ankiecie
// - przygotuj gotowe szablony pyta� - automatyczne wyb�r odpowiedniego zestawu pyta� (admin)
// - przygotuj ekran wyswietlania odpowiedzi na pytania wraz z filtrem (admin)
// - przygotuj ekran ukazujacy dane demograficzne respondent�w (admin)
// - eksport wynik�w do csv (admin)
// - wyswietl postep w ankiecie
// - mozliwosc powrotu do poprzedniego pytania
// - Tryb testowy (demo) dla administratora
// - Modu� A/B test�w � testuj r�ne wersje pyta�
// - Obsluga wielu jezykow

// nie dziala filtrownie odpowiedzi
// dodawanie typu pytania



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
    public IActionResult Admin2(string filter, string value)
    {
        
        var surveyAnswersWithUsers = GetFilteredAnswers(filter, value);
        return View("Admin2",surveyAnswersWithUsers);

    }
    public IActionResult Admin3(string filter, string value)
    {
        
        var surveyUsers = GetFilteredUsers(filter, value);
            return View("Admin3", surveyUsers);
      
    }



    [HttpGet]
    public IActionResult StartSurvey()
    {
        return View(new StartSurveyViewModel());
    }

    [HttpPost]
    public IActionResult StartSurvey(StartSurveyViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        // Zapisz u�ytkownika do bazy
        int userId = SaveUserToDatabase(model.User);

        // Przekieruj do pierwszego pytania, przekazuj�c ID u�ytkownika
        return RedirectToAction("User", new { userId = userId, index = 0 });
    }

    private int SaveUserToDatabase(SurveyUser user)
    {
        using var con = new SqliteConnection("Data Source=Survey.sqlite");
        con.Open();

        var cmd = con.CreateCommand();
        cmd.CommandText = @"
        INSERT INTO surveyUsers (Name, Age, Localization, Sex, Education)
        VALUES (@name, @age, @loc, @sex, @edu);
        SELECT last_insert_rowid();
    ";
        cmd.Parameters.AddWithValue("@name", user.Name);
        cmd.Parameters.AddWithValue("@age", user.Age);
        cmd.Parameters.AddWithValue("@loc", user.Localization);
        cmd.Parameters.AddWithValue("@sex", user.Sex);
        cmd.Parameters.AddWithValue("@edu", user.Education);
        var result = cmd.ExecuteScalar();
        int userId = Convert.ToInt32(result);

        return userId;
    }

    private static List<SurveyAnswer> userAnswers = new();


    public IActionResult User(int userId ,int index =0)
    {
        var questions = GetAllQuestions();

        if (index >= questions.QuestionsList.Count)
        {
            MarkAllAnswersAsCompleted(userAnswers);
            // Zapisz wszystkie odpowiedzi do bazy
            SaveAnswersToDatabase(userAnswers);
            userAnswers.Clear(); // Czy�� po zapisie
            ViewBag.SurveyFinished = true;
            return View("UserEnd");
            
        }

        var question = questions.QuestionsList[index];
        Console.WriteLine("userid= ", userId);
        var viewModel = new SurveyAnswerViewModel
        {
            
            Answer = new SurveyAnswer
            {
                QuestionId = question.Id,
                TimeStamp = DateTime.Now,
                
                UserId = userId // tymczasowo, docelowo pobieraj z sesji/logowania
            }
        };
        ViewBag.QuestionText = question.Name;
        ViewBag.Index = index;
        return View("User", viewModel);


       
    }

    [HttpPost]
    public IActionResult User(SurveyAnswerViewModel model, int index)
    {
        if (index == 0)
        {
            TempData["UserId"] = model.Answer.UserId;
        }
        else
        {
            // Przy kolejnych pytaniach � pobierz userId z TempData
            if (TempData["UserId"] != null)
            {
                model.Answer.UserId = (int)TempData["UserId"];
            }
            else
            {
                // Awaryjnie � brak userId, mo�esz przekierowa� do b��du albo logowania
                return RedirectToAction("Error");
            }
        }

        // Potrzebne, �eby TempData przetrwa�o na kolejn� akcj�
        TempData.Keep("UserId");

        userAnswers.Add(model.Answer);
        Console.WriteLine("index= ",model.Answer.UserId);
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
                Console.WriteLine($"B��d zapisu odpowiedzi: {ex.Message}");
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


    internal SurveyAnswerWithUserViewModel GetFilteredAnswers(string filter, string value)
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
                string whereClause = (string.IsNullOrEmpty(filter) || string.IsNullOrEmpty(value))
      ? ""
      : filter.ToLower() switch
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
                            // Je�li u�ytkownik ju� istnieje, dodaj tylko odpowied�
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
                           

                            // Je�li to nowy u�ytkownik, dodaj go do listy
                            prevUserId = answerWithUser.User.Id;
                            
                        }
                            
                    }

                    if (existingAnswerWithUser.User != null)
                    {
                        allAnswersWithUser.Add(existingAnswerWithUser);
                    }

                }
                var questionTexts = GetAllQuestionTexts();

                foreach (var userAnswers in allAnswersWithUser)
                {
                    foreach (var answer in userAnswers.AnswersList)
                    {
                        if (questionTexts.TryGetValue(answer.QuestionId, out var questionText))
                        {
                            answer.QuestionText = questionText;
                        }
                    }
                }

            }
        }

        return new SurveyAnswerWithUserViewModel { AnswersWithUserList = allAnswersWithUser };

    }



  

    internal SurveyUsersViewModel GetFilteredUsers(string filter, string value)
    {
        List<SurveyUser> users = new();

        using (var con = new SqliteConnection("Data Source=Survey.sqlite"))
        {
            using (var tableCmd = con.CreateCommand())
            {
                con.Open();

                Console.WriteLine($"filter: {filter}, value: {value}");
                string whereClause = (string.IsNullOrEmpty(filter) || string.IsNullOrEmpty(value))
                            ? ""
                        : filter.ToLower() switch
                        {
                            "userid" => "WHERE Id = @value",
                            "age" => "WHERE Age = @value",
                        "sex" => "WHERE Sex = @value",
                        "localization" => "WHERE Localization = @value",
                        _ => ""
                    };
                


                tableCmd.CommandText = $"SELECT Id, Localization, Age, Sex, Education, Name FROM surveyUsers {whereClause}";
                if (!string.IsNullOrEmpty(whereClause))
                {
                    tableCmd.Parameters.AddWithValue("@value", value);
                }
                

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

        return new SurveyUsersViewModel { UsersList = users };
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
                        surveyQuestion.IsActive = reader.GetBoolean(4) == true;

                    }

                    return surveyQuestion;
                }
                ;
            }
        }

    }


    public Dictionary<int, string> GetAllQuestionTexts()
    {
        var result = new Dictionary<int, string>();
        using (var connection = new SqliteConnection("Data Source = Survey.sqlite"))
        {
            connection.Open();
            var command = connection.CreateCommand();
            command.CommandText = "SELECT Id, Name FROM surveyQuestions";

            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    result[reader.GetInt32(0)] = reader.GetString(1);
                }
            }
        }
        return result;
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

        // Dostosuj zwracany JSON, je�li pytanie ma np. inne pola
        return Json(new
        {
            text = question.Name,   // u�yj w�a�ciwo�ci, kt�ra jest pytaniem
            
        });
    }
    [HttpGet]
    public IActionResult FilteredAnswers(string filter, string value)
    {
        var model = GetFilteredAnswers(filter, value);
        return View();
    }
    [HttpGet]
    public IActionResult FilteredUsers(string filter, string value)
    {

        var model = GetFilteredUsers(filter, value);
        return View();
    }

}