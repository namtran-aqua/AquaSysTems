namespace AquaSolution.Shared.ScrapManagement.FlowApprovals
{

    public class CreateFlowStepRequest
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public Guid DecisionMaker { get; set; }
        public string Description { get; set; } = string.Empty;
        public int Step { get; set; }
    }
}