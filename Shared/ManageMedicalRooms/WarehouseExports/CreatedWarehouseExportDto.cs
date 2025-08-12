using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AquaSolution.Shared.ManageMedicalRooms.WarehouseExports
{
    public class CreatedWarehouseExportDto
    {
        public WarehouseExportDto WarehouseExportDto { get; set; } = new();
        public List<WarehouseExportDetailDto> WarehouseExportDetailDtos { get; set; } = new List<WarehouseExportDetailDto>();
    }
}
