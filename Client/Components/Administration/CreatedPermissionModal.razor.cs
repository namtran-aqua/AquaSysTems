using AntDesign;
using AquaSolution.Shared.CommonDto;
using AquaSolution.Shared.Enum;
using AquaSolution.Shared.Permissions;
using Microsoft.AspNetCore.Components;
using System.Net.Http;
using System.Net.Http.Json;

namespace AquaSolution.Client.Components.Administration
{
    public partial class CreatedPermissionModal
    {
        #region Declaration
        [Inject] private HttpClient Http { get; set; }
        private bool IsModalVisible = false;
        private bool IsDisable = true;
        private HandlePermissionDto HandlePermissionDto = new HandlePermissionDto();
        private Form<HandlePermissionDto> formRef = new();
        private List<BaseDto> ListPage = new List<BaseDto>();
        private List<BaseDto> ListMenu = new List<BaseDto>();
        [Parameter] public EventCallback OnSave { get; set; }
        private Func<Task> MenuChange { get; set; }
        private List<PermissionActionType> ListAction = new List<PermissionActionType>();
        private List<PermissionType> ListType = new List<PermissionType>();
        #endregion
        #region Innit
        private BaseDto? _valuePage;
        private BaseDto? ValuePage
        {
            get => _valuePage;
            set
            {
                if (_valuePage != value)
                {
                    _valuePage = value;
                }
            }
        }
        private BaseDto? _valueMenu;
        private BaseDto? ValueMenu
        {
            get => _valueMenu;
            set
            {
                if (_valueMenu != value)
                {
                    _valueMenu = value;
                }
                MenuChange.Invoke();
            }
        }
        private PermissionActionType _valueAction;
        private PermissionActionType ValueAction
        {
            get => _valueAction;
            set
            {
                if (_valueAction != value)
                {
                    _valueAction = value;
                }

            }
        }

        private PermissionType ValueType { get; set; }

        public async Task ShowMidaleAsync()
        {
            MenuChange += GetPagesAsync;
            await SetInit();
            IsModalVisible = true;
            await InvokeAsync(StateHasChanged);
        }
        private async Task SetInit()
        {
            ListType = new();
            ListAction = new();
            ListMenu = new();
            ListPage = new();
            HandlePermissionDto = new();
            await GetMenu();
            ValueMenu = ListMenu.FirstOrDefault();
            await GetPagesAsync();
            await GetAction();
            await GetType();
            ValueType = PermissionType.Page;
        }
        private async Task GetPagesAsync()
        {
            ListPage = new List<BaseDto>();
            ListPage = await Http.GetFromJsonAsync<List<BaseDto>>
                ($"api/Page/GetPageByMenuId/{_valueMenu.Id}");
            await InvokeAsync(StateHasChanged);
        }
        private async Task GetAction()
        {
            ListAction = Enum.GetValues(typeof(PermissionActionType))
                      .Cast<PermissionActionType>()
                      .ToList();
        }
        private async Task GetType()
        {
            ListType = Enum.GetValues(typeof(PermissionType))
                      .Cast<PermissionType>()
                      .ToList();
        }
        private async Task GetMenu()
        {
            ListMenu = await Http.GetFromJsonAsync<List<BaseDto>>
                ($"api/menu/get-all-list");
        }
        #endregion
        #region Action
        private async Task SaveAsync()
        {
            var valid = formRef.Validate();
            if (!valid)
            {
                return;
            }
            if (ValuePage == null)
            {
                await Message.Error("Page cannot be blank");
                return;
            }
            HandlePermissionDto = new HandlePermissionDto
            {
                Action = ValueAction,
                Type = ValueType,
                MenuId = ValueMenu?.Id,
                PageId = ValuePage == null ? null : ValuePage.Id,
            };
            var response = await httpClient.PostAsJsonAsync
                ("api/permission/create-permission", HandlePermissionDto);
            var result = await response.Content.ReadFromJsonAsync<ApiResponse>();
            await Message.Success(result?.message);
            await OnSave.InvokeAsync();
            IsModalVisible = false;
        }
        private void Close()
        {
            IsModalVisible = false;
            StateHasChanged();
        }
        #endregion
        #region HandleData

        #endregion
    }
}
