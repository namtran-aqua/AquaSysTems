using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AquaSolution.Data.Data.Entities.KPI
{
    public class KPIActualMaster
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public int? Quarter { get; set; }
        public int? HaflYear{ get; set; }
        public int Year { get; set; }
        public decimal? KPIScore { get; set; }
        public decimal? KeyTaskScore { get; set; }
        public decimal? OMGScore { get; set; }
        public decimal? TotaleScore { get; set; }
        public Guid UserId { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
