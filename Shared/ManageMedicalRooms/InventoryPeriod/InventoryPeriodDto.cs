using AquaSolution.Shared.Enum;
using System.ComponentModel.DataAnnotations;

namespace AquaSolution.Shared.ManageMedicalRooms.InventoryPeriod
{
    public class InventoryPeriodDto
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        [Required(ErrorMessage = "Please enter the year")]
        [RegularExpression(@"^\d{4}$", ErrorMessage = "Year must have exactly 4 digits")]
        public int Year { get; set; }
        [Required(ErrorMessage = "Please enter the month")]
        [Range(1, 12, ErrorMessage = "Month must be between 1 and 12")]
        public int Month { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public Guid CreatedById { get; set; }
        public string? CreatedByName { get; set; }
        public string? Note { get; set; }
    }
}
