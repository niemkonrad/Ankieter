using Ankieter.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System;


namespace Ankieter.Models
{

    public class SurveyAnswerWithUser
    {



        public List<SurveyAnswer> AnswersList { get; set; }
        public SurveyAnswer Answer { get; set; }
        public SurveyUser User { get; set; }
    }
}