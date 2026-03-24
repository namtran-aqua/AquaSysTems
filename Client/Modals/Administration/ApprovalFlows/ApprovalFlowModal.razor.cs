using AntDesign;
using AquaSolution.Shared.ApprovalFlows;
using AquaSolution.Shared.Enum;
using AquaSolution.Shared.Position;
using AquaSolution.Shared.UserManagements;
using Microsoft.AspNetCore.Components;
using System.Net.Http.Json;

namespace AquaSolution.Client.Modals.Administration.ApprovalFlows
{
    public partial class ApprovalFlowModal
    {
        #region Declaration
        [Inject] private HttpClient Http { get; set; } = new();
        private bool IsModalVisible = false;
        [Parameter] public EventCallback OnSave { get; set; }
        private Form<ApprovalFlowDto> formRef = new();
        private ApprovalFlowDto _modal = new ApprovalFlowDto();
        private bool IsEdit { get; set; }
        private string Title { get; set; }
        private List<UserContributerDto> AllUser = new();
        private List<PositionDto> ListPosition = new List<PositionDto>();
        private List<ApprovalSettingType> ListApprovalSettingType = new List<ApprovalSettingType>();
        #endregion
        #region Init
        public async Task Showmodal(ApprovalFlowDto modal, bool isEdit)
        {
            IsEdit = isEdit;
            _modal = new();
            await LoadUser();
            await LoadApprovalSettingType();
            await LoadPosition();
            if (IsEdit)
            {
                Title = "Edit Factory";
                _modal = modal;
            }
            else
            {
                Title = "Created Factory";
            }

            IsModalVisible = true;
            await InvokeAsync(StateHasChanged);
        }
        private async Task LoadUser()
        {
            var result = await Http.GetFromJsonAsync<List<UserContributerDto>>("api/user/get-contributer");
            if (result != null && result.Any())
            {
                AllUser = result;
            }

        }
        private async Task LoadPosition()
        {
            var data = await Http.GetFromJsonAsync<List<PositionDto>>("api/position/get-all");
            if (data != null)
            {
                ListPosition = data;
            }
        }
        private async Task LoadApprovalSettingType()
        {
            ListApprovalSettingType = Enum.GetValues(typeof(ApprovalSettingType))
               .Cast<ApprovalSettingType>()
               .ToList();
        }
        #endregion
        #region Action
        private async Task SaveAsync()
        {
            bool isaved = false;
            if (!IsEdit)
            {
                isaved= await CreatedAsync();
            }
            else
            {
                isaved= await UpdateAsync();
            }
            if(isaved)
            {
                IsModalVisible = false;
                await OnSave.InvokeAsync();
            }
        }
        private void HandleCancel() => IsModalVisible = false;
        #endregion
        #region handler 
        private async Task<bool> CreatedAsync()
        {
            var response = await Http.PostAsJsonAsync($"api/approvalFlow/create", _modal);
            if (response.IsSuccessStatusCode)
            {
                await Message.Success("Created successfully.");
                return true;
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                await Message.Error($"Lỗi: {error}");
                return false;
            }
        }
        private async Task<bool> UpdateAsync()
        {
            var response = await Http.PutAsJsonAsync($"api/approvalFlow/update", _modal);
            if (response.IsSuccessStatusCode)
            {
                await Message.Success("Created successfully.");
                return true;
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                await Message.Error($"Lỗi: {error}");
                return false;
            }
        }
        #endregion
    }
}
