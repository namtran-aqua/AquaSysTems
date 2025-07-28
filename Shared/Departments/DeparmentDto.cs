using AquaSolution.Shared.Enum;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AquaSolution.Shared.Departments
{
    public class DepartmentDto
    {
        public Guid Id { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string Code { get; set; }
        public string? Description { get; set; }
        public DepartmentType DepartmentType { get; set; }

        public string? Note { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
