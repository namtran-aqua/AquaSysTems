

namespace AquaSolution.Data.Data.Entities.Scraps
{
    public class FlowApprovalScrap
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public Guid DepartmentId { get; set; }
        
        public Guid? SectionId { get; set; }

        public Guid FactoryId { get; set; }

        public Guid DecisionMaker { get; set; }

        public string Description { get; set; }

        public int Step { get; set; }
    }
}