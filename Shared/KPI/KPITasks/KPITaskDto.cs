using AquaSolution.Shared.Enum.KPIType;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AquaSolution.Shared.KPI.KPITasks
{
    public class KPITaskDto
    {
        public Guid Id { get; set; }
        public string TaskName { get; set; } = string.Empty;
        public KPICategoryType KPICategory { get; set; }
        public string TaskDescription { get; set; }
        public string CalculatedMdethod { get; set; }
        public string DataSource { get; set; }
        public string PIC { get; set; }
        public KPIIndexType KPIIndexType { get; set; }

        public Guid FormulaId { get; set; }
        public string Formula { get; set; }
        public Guid CalculatedId { get; set; }
        public string Calculated { get; set; }
        public KPIFormulaType KPIFormulaType { get; set; }
        public decimal Max { get; set; }
        public decimal Bottom { get; set; }
        public Guid FactoryId { get; set; }
        public string Factory { get; set; }
        public Guid DepartmentId { get; set; }
        public string Department { get; set; }
        public string Unit { get; set; }
        public decimal Weight { get; set; }
        public Guid CreatedById { get; set; }
        public string CreatedByName { get; set; }
        public DateTime CreatedDate { get; set; }
        public QuarterCalculateType QuarterCalculateType { get; set; }
    }
}
