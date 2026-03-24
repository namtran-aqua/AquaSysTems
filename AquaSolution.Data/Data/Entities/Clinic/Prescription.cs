using AquaSolution.Shared.Enum;

namespace AquaSolution.Data.Data.Entities.Clinic
{
    public class Prescription
    {
        public Guid Id { get; set; }
        public Guid RequestId {  get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string? Note { get; set; }
        public DateTime CreatedDate { get; set; }
        public Guid? PharmacyManagerId { get; set; }
    }
}
