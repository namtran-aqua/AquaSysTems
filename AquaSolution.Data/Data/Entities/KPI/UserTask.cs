namespace AquaSolution.Data.Data.Entities
{
    public class UserTask
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid KPITaskId { get; set; }
        public bool IsActive { get; set; }
    }
}
