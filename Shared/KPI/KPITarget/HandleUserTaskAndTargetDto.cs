using AquaSolution.Shared.KPI.UserTask;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AquaSolution.Shared.KPI.KPIMonthlyTarget
{
    public class HandleUserTaskAndTargetDto
    {
        public  List<HandleUserTaskDto> HandleUserTaskDtos { get; set; } = new();
        public List<List<UpdateTargetDto>> UpdateTargetDtos { get; set; } = new();
    }
}
