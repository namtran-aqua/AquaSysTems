using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AquaSolution.Shared.UserManagements
{
    public class CreatedAndUpdateUserDto
    {
        public Guid Id { get; set; }
        public string WorkDayId { get; set; }
        [Required(ErrorMessage = "FirstName is required")]
        public string FirstName { get; set; }
        [Required(ErrorMessage = "LastName is required")]
        public string LastName { get; set; }
        public string FullName { get; set; }
        [Required(ErrorMessage = "Email is required")]
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public Guid? GroupId { get; set; }
        public DateTime CreatedTime { get; set; }
        public bool IsActive { get; set; }
        public DateTime? UpdatedTime { get; set; }
        public string? UpdateBy { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public string? AvatarUrl { get; set; }
        public Guid? DepartmentId { get; set; }
        public Guid? ManagerId { get; set; }
        public Guid? PositionId { get; set; }
        public Guid? FactoryId { get; set; }
    }
}
