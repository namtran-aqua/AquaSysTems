using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AquaSolution.Shared.HRMS
{
    public class BonusYearDto
    {
        public int Id { get; set; }
        public string? EmpWorkDay { get; set; } 
        public string? EmpName { get; set; } 
        public string? EmpCMND { get; set; } 
        public string? EmpDept { get; set; } 
        public DateTime EmpJoinDate { get; set; } 
        public string? Q1Rated { get; set; } 
        public string? Q2Rated { get; set; } 
        public string? Q3Rated { get; set; } 
        public string? Q4Rated { get; set; } 
        public string? Q1Ratio { get; set; } 
        public string? Q2Ratio { get; set; } 
        public string? Q3Ratio { get; set; } 
        public string? Q4Ratio { get; set; } 
        public string? YearRation { get; set; } 
        public string? NoteRatio { get; set; } 
        public string? WorkTimeRation { get; set; } 
        public string? AwardYearRatio { get; set; } 
         public string? BonusYear { get; set; } 
         public string? RankForMeritIncrease { get; set; } 
         public string? RankAverageBonusRate { get; set; } 
         public string? RankQ1Ratio { get; set; } 
         public string? RankQ2Ratio { get; set; } 
         public string? RankQ3Ratio { get; set; } 
         public string? RankQ4Ratio { get; set; } 
    }
}
