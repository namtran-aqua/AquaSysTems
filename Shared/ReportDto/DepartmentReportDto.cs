using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AquaSolution.Shared.ReportDto
{
    public class DepartmentReportDto
    {
        public Guid DepartmentId { get; set; }
        public string DepartmentName { get; set; } = string.Empty;
        public int TotalOrders { get; set; }
        public decimal TotalQuantity { get; set; }
        public decimal TotalWeight { get; set; }
        public decimal ConfirmedWeight { get; set; }
        public double ConfirmRate => TotalWeight == 0 ? 0 : Math.Round((double)(ConfirmedWeight / TotalWeight) * 100, 1);
        //public string StatusLabel { get; set; } = "Normal";
    }
}
