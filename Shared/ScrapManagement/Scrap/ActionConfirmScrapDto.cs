using AquaSolution.Shared.Enum.Scrap;
using System;
using System.Collections.Generic;

namespace AquaSolution.Shared.ScrapManagement.Scrap
{
    public class ActionConfirmScrapDto
    {
        public Guid HistoryScrapId { get; set; }
        public Guid ConfirmerId { get; set; }
        public List<ConfirmDetailDto> Details { get; set; } = new();
    }

    public class ConfirmDetailDto
    {
        public Guid HistoryDetailId { get; set; }
        public decimal ConfirmAmount { get; set; }
        public ConfirmUnitType ConfirmUnitType { get; set; }
        public string? ConfirmNote { get; set; }
    }
}
