using AntDesign;
using AquaSolution.Shared.Administration.SystemLock;
using AquaSolution.Shared.ApprovalFlows;
using AquaSolution.Shared.CommonDto;
using AquaSolution.Shared.UserManagements;
using Microsoft.AspNetCore.Components;
using System.Net.Http.Json;

namespace AquaSolution.Client.Modals.Administration.SystemLock
{
    public partial class SystemLockModal
    {
        #region Declaration
        [Inject] private HttpClient Http { get; set; }
        [Parameter] public EventCallback OnSave { get; set; }
        private bool IsVisibleModal { get; set; }
        private SystemLockDto _systemLockDto = new SystemLockDto();
        private UserDto? CurrenUser { get; set; }
        private List<BaseDto> _listPage { get; set; }

        private bool _isEdit { get; set; }
        #endregion
        #region Innit

        public async Task ShowModelAsync(bool isEdit, SystemLockDto systemLockDto , UserDto userDto)
        {
            _systemLockDto = systemLockDto;
            _isEdit = isEdit;
            CurrenUser = userDto;
            await LoadPage();
            IsVisibleModal = true;
            await InvokeAsync(StateHasChanged);
        }
        private async Task LoadPage()
        {
            _listPage = new List<BaseDto>();
            _listPage = await Http?.GetFromJsonAsync<List<BaseDto>>($"api/Page/get-all-pages");
            await InvokeAsync(StateHasChanged);
        }
       
        #endregion
        #region Action
        private async Task SaveAsync()
        {
            if(!_isEdit)
            {
              
                _systemLockDto.LockedDate = DateTime.UtcNow;
                _systemLockDto.LockedBy = CurrenUser != null ? CurrenUser.Id : Guid.Empty;
                _systemLockDto.IsLocket = false;
                var response = await Http.PostAsJsonAsync("api/systemlock/create", _systemLockDto);
                var content = await response.Content.ReadFromJsonAsync<ApiResponse>();
                if (response.IsSuccessStatusCode)
                {
                    await OnSave.InvokeAsync();
                    await Message.Success(content?.message ?? "Created successfully");
                    IsVisibleModal = false;
                }
                else
                {
                    await Message.Error(content?.message ?? "An unexpected error occurred");
                }
            }
            await OnSave.InvokeAsync();


        }
        private async Task CloseModal()
        {
            IsVisibleModal = false;
            await InvokeAsync(StateHasChanged);
        }
        #endregion

    }
}
