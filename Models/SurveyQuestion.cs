using System;
using Microsoft.AspNetCore.Mvc;


namespace Ankieter.Models
{

    public class SurveyQuestion
    {
        public int Id { get; set; }
        
        public string Name { get; set; } = string.Empty; // Question text or name

        public int Type { get; set; } = 0;// 0 - Not selected, 1 - Value (1-10), 2 - Multiple Choice, 3- Open Text 

        public DateTime TimeStamp { get; set; } = DateTime.Now;

        public bool IsActive { get; set; } = false;
    }

   
}