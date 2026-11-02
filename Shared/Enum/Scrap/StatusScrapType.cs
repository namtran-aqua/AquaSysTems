using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AquaSolution.Shared.Enum.Scrap
{
    public enum StatusScrap
    {
        Pending,
        InterView,
        Approved,
        Rejected,
        Done,
        WaitingForApproval,
        WaitingForConfirmation
    }
}
