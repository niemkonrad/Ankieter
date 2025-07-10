using System;
using Microsoft.AspNetCore.Mvc;
using Ankieter.Models;

namespace Ankieter.Models.ViewModels
{

  
    public class StartSurveyViewModel
    {
       


        public SurveyUser User { get; set; } = new();
    }

}