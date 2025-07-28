using AquaSolution.Client.Common;
using AquaSolution.Shared.CommonDto;
using Microsoft.AspNetCore.Components;
using System.Net.Http.Json;
using AquaSolution.Shared.ApprovalFlows;

namespace AquaSolution.Client.Pages.Administration
{
    public partial class ApprovalFlowManagement
    {
        #region Declaration
        [Inject] private HttpClient Http { get; set; }
        private List<ApprovalFlowDto> ListApprovalFlow = new();
        private List<ApprovalFlowGroupDto> groupedList = new();
        private Dictionary<Guid, List<ApprovalFlowDto>> groupedByPosition = new();
        private Dictionary<Guid, string> positionNames = new();
        // private ApprovalFlowModal ApprovalFlowModal = new ApprovalFlowModal();
        #endregion

        #region Innit
        protected override async Task OnInitializedAsync()
        {
            await LoadDataAsync();

        }
        private async Task LoadDataAsync()
        {
            ListApprovalFlow = await Http.GetFromJsonAsync<List<ApprovalFlowDto>>
                ("api/ApprovalFlow/get-all");
             groupedList = ListApprovalFlow
                     .GroupBy(x => new { x.PositionName, x.System })
                     .Select(group => new ApprovalFlowGroupDto
                     {
                         PositionName = group.Key.PositionName,
                         System = group.Key.System,
                         Items = group.Select(item => new ApprovalFlowItemDto
                         {
                             CurrentStep = item.CurrentStep,
                             NextStep = item.NextStep,
                             UserApproveName = item.UserApproveName,
                             ApprovalSettingType = item.ApprovalSettingType
                         }).ToList()
                     })
                     .ToList();

        }
        #endregion

        #region Action
        private async Task CreatedApprovalFlow()
        {
            // await ApprovalFlowModal.Showmodal(new ApprovalFlowDto(), false);
        }
        private async Task EditApprovalFlow(ApprovalFlowDto ApprovalFlowDto)
        {
            // await ApprovalFlowModal.Showmodal(ApprovalFlowDto, true);
        }
        private async Task DeleteAsync(ApprovalFlowDto ApprovalFlowDto)
        {
            var message = $"Bạn có muốn xóa ApprovalFlow \" {ApprovalFlowDto.Name} \" không?";
            var confirm = await MessageBox.Confirm(modal, message.ToString());
            if (confirm)
            {
                var response = await Http.DeleteAsync($"api/ApprovalFlow/delete/{ApprovalFlowDto.Id}");
                await LoadDataAsync();
                var content = await response.Content.ReadFromJsonAsync<ApiResponse>();
                if (response.IsSuccessStatusCode)
                {
                    await Message.Success(content?.message ?? "Xóa thành công");
                }
                else
                {
                    await Message.Error(content?.message ?? "Có lỗi xảy ra");
                }
            }
            await InvokeAsync(StateHasChanged);
        }
        #endregion
    }
}
