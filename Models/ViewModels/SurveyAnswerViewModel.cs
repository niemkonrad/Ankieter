using System;
using Microsoft.AspNetCore.Mvc;
using Ankieter.Models;

namespace Ankieter.Models.ViewModels
{

    public class SurveyAnswerViewModel
    {
        public List<SurveyAnswer> AnswersList { get; set; } = new List<SurveyAnswer>();


        public SurveyAnswer Answer { get; set; } = new();
    }
}