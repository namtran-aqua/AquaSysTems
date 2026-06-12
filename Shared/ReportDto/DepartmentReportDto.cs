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
        public decimal TotalWeight { get; set; }
        public decimal ConfirmedWeight { get; set; }

        public int TotalOrderApproval { get; set; }
        public int TotalOrderReject { get; set; }
        public int TotalOrderPending { get; set; }
        public int TotalOrderDone { get; set; }


        // Số lượng (từ HistoryScrapDetail.Quantity) — theo từng trạng thái
        public decimal TotalQuantity { get; set; }   // tất cả
        public decimal TotalQuantityApproval { get; set; }   // đã duyệt
        public decimal TotalQuantityReject { get; set; }   // từ chối
        public decimal TotalQuantityDone { get; set; }   // hoàn thành
        public decimal TotalQuantityPending { get; set; }   // đang chờ
    }
}
