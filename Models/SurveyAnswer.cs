using System;
using Microsoft.AspNetCore.Mvc;


namespace Ankieter.Models
{


    public class SurveyAnswer
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int QuestionId { get; set; }
        public string? Answer { get; set; } // For Text and Open Text and numeric answers - 

        public DateTime TimeStamp { get; set; } 
        public Boolean SurveyCompleted { get; set; } = true; // 0 - Not completed, 1 - Completed 
    }

    
}