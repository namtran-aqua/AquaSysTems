using AquaSolution.Shared.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AquaSolution.Shared.ManageMedicalRooms.Products
{
    public class ProductExportDto
    {
        public Guid Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Title =>
       $"<strong>Name:</strong> {Name}<br />" +
       $"<strong>Code:</strong> {Code}<br />" +
       $"<strong>ManufacturingDate:</strong> {ManufacturingDate:dd/MM/yyyy}<br />" +
       $"<strong>ExpirationDate:</strong> {ExpirationDate:dd/MM/yyyy}<br />" +
       $"<strong>Quantity:</strong> {Quantity:F1}";

        public decimal Quantity { get; set; }
        public string? Note { get; set; }
        public ProductType ProductType { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string? UpdateBy { get; set; }
        public string? Unit { get; set; }
        public DateTime? ExpirationDate { get; set; }
        public DateTime? ManufacturingDate { get; set; }
        public bool IsHide { get; set; }
    }

}
