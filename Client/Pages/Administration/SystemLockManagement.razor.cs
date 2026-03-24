using AquaSolution.Client.Common;
using AquaSolution.Client.Modals.Administration.Department;
using AquaSolution.Client.Modals.Administration.SystemLock;
using AquaSolution.Shared.Administration.SystemLock;
using AquaSolution.Shared.CommonDto;
using AquaSolution.Shared.Departments;
using AquaSolution.Shared.UserManagements;
using Microsoft.AspNetCore.Components;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;

namespace AquaSolution.Client.Pages.Administration
{
    public partial class SystemLockManagement
    {
        #region Declaration
        [Inject] private HttpClient? Http { get; set; }
        private List<SystemLockDto>? _systemLockDto = new();
            private SystemLockModal systemLockModal =new SystemLockModal();
        private UserDto? CurrenUser { get; set; }
        #endregion
        #region Innit
        protected override async Task OnInitializedAsync()
        {
            await LoadDataAsync();
            var currenUserClass = new CurrenUser(Http, AuthStateProvider);
            CurrenUser = await currenUserClass.LoadCurrenUser();

        }
        private async Task LoadDataAsync()
        {
            if (Http != null)
                _systemLockDto = await Http.GetFromJsonAsync<List<SystemLockDto>>("api/systemLock/system-locks");
            await InvokeAsync(StateHasChanged);
        }
        #endregion

        #region Action
        private async Task Created()
        {
            if (CurrenUser == null) return;
            await systemLockModal.ShowModelAsync(false,new SystemLockDto(),CurrenUser);
        }
        
        private async Task OnLockChanged(SystemLockDto row, bool isLocket)
        {
            row.IsLocket  = isLocket;
            var response = await Http.PutAsJsonAsync("api/systemlock/update", row);
    
            if (response.IsSuccessStatusCode)
            {
                if (isLocket)
                {
                    await Message.Success($"The {row.PageName} page has been locked.");
                    await LoadDataAsync();
                }
                if (!isLocket)
                {
                    await Message.Success($"The {row.PageName} page has been unlocked.");
                    await LoadDataAsync();
                }

            }
            else
            {
                await Message.Error( "An unexpected error occurred");
            }
            await InvokeAsync(StateHasChanged);
        }
        #endregion

    }
}
