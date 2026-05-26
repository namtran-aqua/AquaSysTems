namespace AquaSolution.Shared.ScrapManagement.FlowApprovals
{
    public class CreateFlowApprovalRequest
    {
        public Guid DepartmentId { get; set; }
        public Guid FactoryId { get; set; }
        public List<CreateFlowStepRequest> Steps { get; set; } = new();
    }
}
