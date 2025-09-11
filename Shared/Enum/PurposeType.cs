using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AquaSolution.Shared.Enum
{
    public enum PurposeType
    {
        [Display(Name = "SickLeave / Nghỉ Bệnh")]
        SickLeave,

        [Display(Name = "MedicationRequest / Đề nghị cấp thuốc")]
        MedicationRequest
    }
}
