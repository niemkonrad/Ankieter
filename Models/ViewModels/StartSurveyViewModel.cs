using Ankieter.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;

namespace Ankieter.Models.ViewModels
{

  
    public class StartSurveyViewModel
    {


        [ValidateNever]
        public List<SelectListItem> Cities { get; set; }

        public SurveyUser User { get; set; } = new();
    }

}