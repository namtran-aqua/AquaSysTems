
using AquaSolution.Shared.ApprovalFlows;

namespace AquaSolution.Server.Services.ApprovalFlowService
{
    public interface IApprovalFlowService
    {
        Task<List<ApprovalFlowDto>> GetListApprovalFlow();
        Task<bool> DeleteApprovalFlow(Guid ApprovalFlowId);
        Task<bool> CreatedApprovalFlow(ApprovalFlowDto ApprovalFlowDto);
        Task<bool> UpdateApprovalFlow(ApprovalFlowDto ApprovalFlowDto);
    }
}
