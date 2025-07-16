using Microsoft.AspNetCore.Mvc;
using System;
using System.ComponentModel.DataAnnotations;


namespace Ankieter.Models
{

   

    public class SurveyUser
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Miasto jest wymagane")]
        public string Localization { get; set; }
        [Range(1, 120, ErrorMessage = "Wprowad� prawid�owy wiek")]
        public int Age { get; set; }
        [Required(ErrorMessage = "P�e� jest wymagana")]
        public char Sex { get; set; }
        [Required(ErrorMessage = "Wykszta�cenie jest wymagane")]
        public int Education { get; set; } // 0 - Primary, 1 - Secondary, 2 - Higher, 3 - Postgraduate
        [Required(ErrorMessage = "Imi� jest wymagane")]
        public string Name { get; set; }
        
    }
}