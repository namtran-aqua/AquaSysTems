using AquaSolution.Shared.Enum.KPIType;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AquaSolution.Shared.KPI.Result
{
    public class ViewResultKpiDto
    {
        public Guid SubmitId { get; set; }
        public string UserName { get; set; }
        public string WorkDayId { get; set; }
        public string Approver { get; set; }
        public StatusKPIRequestType Status { get; set; }
        public KPITotalScoreType kPITotalScoreType { get; set; }
        public DateTime ApprovalDate { get; set; }
        public int? Month { get; set; }
        public int? Quarter { get; set; }
        public int? HalfYear { get; set; }
        public int? Year { get;set; }
        public decimal?KPIScore { get; set; }
        public decimal?KeyTaskScore { get; set; }
        public decimal?OMGScore { get; set; }
        public decimal?TotalScroe { get; set; }
        public string? Description { get; set; }
        public Guid? DepartmentId { get; set; }
        public Guid? FactoryId { get; set; }
        public string? Department{ get; set; }
        public string? Factory { get; set; }

    }
}
