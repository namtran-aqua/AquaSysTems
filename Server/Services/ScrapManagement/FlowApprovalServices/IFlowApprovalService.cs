using AquaSolution.Shared.ScrapManagement.FlowApprovals;

namespace AquaSolution.Server.Services.ScrapManagetment.FlowApprovalServices
{
    public interface IFlowApprovalService
    {
        // Lấy toàn bộ luồng theo department và factory
        Task<FlowApprovalResponse> GetFlowApprovalAsync(Guid departmentId, Guid factoryId);

        // Lấy danh sách tất cả luồng theo factory
        Task<List<FlowApprovalResponse>> GetAllByFactoryAsync(Guid factoryId);

        // Tạo mới cả luồng
        Task<bool> CreateFlowApprovalAsync(CreateFlowApprovalRequest request);

        // Update 1 step cụ thể
        Task<bool> UpdateFlowStepAsync(Guid id, UpdateFlowStepRequest request);

        // Xóa 1 step
        Task<bool> DeleteFlowStepAsync(Guid id);

        // Xóa toàn bộ luồng của 1 department
        Task<bool> DeleteFlowApprovalAsync(Guid departmentId, Guid factoryId);
        Task<List<FlowApprovalResponse>> GetAllAsync();
    }
}