using System;
using Microsoft.AspNetCore.Mvc;
using Ankieter.Models;

namespace Ankieter.Models.ViewModels
{

  
    public class SurveyUsersViewModel
    {

        public List<SurveyUser> UsersList { get; set; } = new List<SurveyUser>();

        public SurveyUser User { get; set; } 
    }

}