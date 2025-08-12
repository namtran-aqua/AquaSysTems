using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AquaSolution.Shared.ManageMedicalRooms.InventoryPeriod
{
    public class CreatedInventoryPeriodDto
    {
        public InventoryPeriodDto InventoryPeriodDto { get; set; } = new();
        public List<InventoryPeriodDetailDto> InventoryPeriodDetailDtos { get; set; } = new();
    }
}
