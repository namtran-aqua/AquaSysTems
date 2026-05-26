using AntDesign;
using AquaSolution.Client.Pages.KPI.UserTask;
using AquaSolution.Shared.ScrapManagement.FlowApprovals;
using AquaSolution.Shared.UserManagements;
using Microsoft.AspNetCore.Components;
using System.Net.Http.Json;

namespace AquaSolution.Client.Modals.ScrapManagement.FlowApprovalScraps
{
    public partial class EditStepModal 
    {
        #region Declaration
        [Inject] private HttpClient Http { get; set; }
        [Inject] private IMessageService Message { get; set; }

        [Parameter] public EventCallback OnSaved { get; set; }

        private bool IsModalVisible { get; set; } = false;
        private bool _loading = false;
        private bool _submitting = false;

        private FlowApprovalScrapDto? _step { get; set; }
        private Guid _selectedUserId { get; set; } = Guid.Empty;
        private List<UserDto> _users { get; set; } = new();
        #endregion

        #region Init
        public async Task ShowModal(FlowApprovalScrapDto step)
        {
            _step = step;
            _selectedUserId = step.DecisionMaker;

            await LoadUsers();
            IsModalVisible = true;
            StateHasChanged();
        }

        private async Task LoadUsers()
        {
            try
            {
                _loading = true;
                StateHasChanged();

                var result = await Http.GetFromJsonAsync<List<UserDto>>("api/User/get-all");
                var user = result ?? new();
                _users = user.Where(x => x.IsActive == true).ToList();
            }
            catch
            {
                await Message.Error("Không thể tải danh sách người dùng!");
            }
            finally
            {
                _loading = false;
            }
        }
        #endregion

        #region Actions
        private async Task SubmitAsync()
        {
            if (_selectedUserId == Guid.Empty)
            {
                await Message.Warning("Vui lòng chọn người duyệt!");
                return;
            }

            if (_selectedUserId == _step!.DecisionMaker)
            {
                await Message.Warning("Người duyệt chưa thay đổi!");
                return;
            }

            try
            {
                _submitting = true;

                var request = new UpdateFlowStepRequest
                {
                    Id = _step.Id,
                    DecisionMaker = _selectedUserId,
                    Step = _step.Step
                };

                var response = await Http.PutAsJsonAsync(
                    $"api/FlowApprovalScrap/update-step/{_step.Id}", request);

                if (response.IsSuccessStatusCode)
                {
                    await Message.Success("Cập nhật người duyệt thành công!");
                    await OnSaved.InvokeAsync();
                    Close();
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    await Message.Error($"Thất bại: {error}");
                }
            }
            catch (Exception ex)
            {
                await Message.Error($"Lỗi: {ex.Message}");
            }
            finally
            {
                _submitting = false;
            }
        }

        private void Close()
        {
            IsModalVisible = false;
            _step = null;
            _selectedUserId = Guid.Empty;
            StateHasChanged();
        }
        #endregion
    }
}