using System;
using Microsoft.AspNetCore.Mvc;


namespace Ankieter.Models
{

   

    public class SurveyUser
    {
        public int Id { get; set; }

        public string Localization { get; set; }
        public int Age { get; set; }
        public char Sex { get; set; }
        public int Education { get; set; } // 0 - Primary, 1 - Secondary, 2 - Higher, 3 - Postgraduate
        public string Name { get; set; }
        
    }
}