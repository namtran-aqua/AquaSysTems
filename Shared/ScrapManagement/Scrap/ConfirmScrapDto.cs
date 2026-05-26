using AquaSolution.Shared.Enum.Scrap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AquaSolution.Shared.ScrapManagement.Scrap
{
    public class ConfirmScrapDto
    {
        public Guid HistoryScrapId { get; set; }
        public Guid ConfirmerId { get; set; }
        public decimal ConfirmAmount { get; set; }
        public ConfirmationStatusType ConfirmationStatusType { get; set; }
        public string? Notes { get; set; }
    }
}
