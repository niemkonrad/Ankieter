using System;
using Microsoft.AspNetCore.Mvc;
using Ankieter.Models;

namespace Ankieter.Models.ViewModels
{

    public class SurveyAnswerWithUserViewModel
    {

       
        public List<SurveyAnswerWithUser> AnswersWithUserList{ get; set; } = new List<SurveyAnswerWithUser>();

        public List<SurveyAnswerViewModel> AnswerList { get; set; } = new List<SurveyAnswerViewModel>();


        public SurveyAnswer Answer { get; set; } = new();

        public SurveyUser User { get; set; } = new();


    }
}