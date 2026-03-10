using AquaSolution.Shared.Enum.KPIType;
namespace AquaSolution.Shared.KPI.KPISubmit
{
    public class GroupViewKPIForApproval
    {
        public EApprovalStatusType ApprovalStatusType { get; set; }
        public string StatusName { get; set; } = string.Empty;
        public List<ViewKPIForApprovalDto> Items { get; set; } = new();
    }
}
