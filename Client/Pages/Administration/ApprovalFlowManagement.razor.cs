using AquaSolution.Client.Common;
using AquaSolution.Client.Components.Administration.ApprovalFlows;
using AquaSolution.Shared.ApprovalFlows;
using AquaSolution.Shared.CommonDto;
using AquaSolution.Shared.Enum;
using AquaSolution.Shared.UserManagements;
using Microsoft.AspNetCore.Components;
using NPOI.SS.Formula.Functions;
using System.Net.Http.Json;

namespace AquaSolution.Client.Pages.Administration
{
    public partial class ApprovalFlowManagement
    {
        #region Declaration
        [Inject] private HttpClient? Http { get; set; }
        private List<ApprovalFlowDto>? _listApprovalFlow = new();
        private List<ApprovalFlowGroupDto> _groupedList = new();
        private ApprovalFlowModal approvalFlowModal;
        #endregion

        #region Innit
        protected override async Task OnInitializedAsync()
        {
            await LoadDataAsync();

        }
        private async Task LoadDataAsync()
        {
            if (Http != null)
            {
                try
                {

                    _listApprovalFlow = await Http.GetFromJsonAsync<List<ApprovalFlowDto>>("api/approvalFlow/get-all");

                }
                catch (Exception ex)
                {
                    throw ex;
                }

                if (_listApprovalFlow != null)
                {
                    _groupedList = _listApprovalFlow
                        .GroupBy(x => new { x.FlowApproval })
                        .Select(g => new ApprovalFlowGroupDto
                        {

                            Items = g.OrderBy(x => x.CurrentStep).ToList()
                        })
                        .ToList();
                }
            }
        }
        #endregion

        #region Action
        private async Task CreatedApprovalFlow()
        {
            await approvalFlowModal.Showmodal(new ApprovalFlowDto(), false);
        }
        private async Task EditApprovalFlow(ApprovalFlowDto approvalFlowDto)
        {
            await approvalFlowModal.Showmodal(approvalFlowDto, true);
        }
        private async Task DeleteAsync(ApprovalFlowDto approvalFlowDto)
        {
            var message = $"Are you sure you want to delete the ApprovalFlow \" {approvalFlowDto.Name} \" không?";
            var confirm = await MessageBox.Confirm(Modal, message.ToString());
            if (confirm)
            {
                var response = await Http.DeleteAsync($"api/ApprovalFlow/delete/{approvalFlowDto.Id}");
                await LoadDataAsync();
                var content = await response.Content.ReadFromJsonAsync<ApiResponse>();
                if (response.IsSuccessStatusCode)
                {
                    await Message.Success(content?.message ?? "Deleted successfully");
                }
                else
                {
                    await Message.Error(content?.message ?? "An unexpected error occurred");
                }
            }
            await InvokeAsync(StateHasChanged);
        }
        #endregion
    }
}
