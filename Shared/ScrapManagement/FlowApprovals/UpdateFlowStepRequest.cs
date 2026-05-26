using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AquaSolution.Shared.ScrapManagement.FlowApprovals
{
    public class UpdateFlowStepRequest
    {
        public Guid Id { get; set; }
        public Guid DecisionMaker { get; set; }
        public int Step { get; set; }
    }
}
