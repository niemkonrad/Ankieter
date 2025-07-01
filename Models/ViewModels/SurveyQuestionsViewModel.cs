using System;
using Microsoft.AspNetCore.Mvc;
using Ankieter.Models;

namespace Ankieter.Models.ViewModels
{

  
    public class SurveyQuestionsViewModel
    {
        public List<SurveyQuestion> QuestionsList { get; set; } = new List<SurveyQuestion>();


        public SurveyQuestion Question { get; set; } = new();
    }

}