using AntDesign;
using AquaSolution.Shared.Departments;
using AquaSolution.Shared.Enum;
using AquaSolution.Shared.Menus;
using AquaSolution.Shared.UserManagements;
using Microsoft.AspNetCore.Components;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace AquaSolution.Client.Modals.Administration.Department
{
    public partial class DepartmentModal
    {
        #region Declaration
        [Inject] private HttpClient Http { get; set; } = new();
        private bool IsModalVisible = false;
        private DepartmentDto _model = new DepartmentDto();
        [Parameter] public EventCallback OnSave { get; set; }
        private Form<DepartmentDto> formRef = new();
        private bool IsEdit {  get; set; }
        private string Title { get; set; }
        private List<DepartmentType> ListDepartmentType = new List<DepartmentType>();
        #endregion
        #region Innit
        public async Task Showmodal(DepartmentDto departmentDto,bool isEdit)
        {
            IsEdit = isEdit;
            if (IsEdit)
            {
                Title = "Edit Department";
            }
            else
            {
                Title = "Created Department";
            }
            _model = departmentDto;
            GetDepartmentType();
            IsModalVisible = true;
            await InvokeAsync(StateHasChanged);
        }
        private void GetDepartmentType()
        {
            ListDepartmentType = Enum.GetValues(typeof(DepartmentType))
                    .Cast<DepartmentType>()
                    .ToList();
            if (_model != null)
            {
                DepartmentType = ListDepartmentType
                         .FirstOrDefault(x => x == _model.DepartmentType);
            }
            else
            {
                DepartmentType = ListDepartmentType
                        .First();
            }
        }
        #endregion
        #region Action
        private void HandleCancel() => IsModalVisible = false;

        private async Task SaveAsync()
        {
            var valid = formRef.Validate();
            if (!valid)
            {
                return;
            }
            if (IsEdit)
            {
                _model.DepartmentType = DepartmentType;
                await UpdateAsync(_model);
            }
            else
            {
                _model.DepartmentType = DepartmentType;
                await CreatedAsync(_model);
            }
            IsModalVisible = false;
            await OnSave.InvokeAsync();
        }
        private DepartmentType _departmentType;
        private DepartmentType DepartmentType
        {
            get => _departmentType;
            set
            {
                if (_departmentType != value)
                {
                    _departmentType = value;
                }

            }
        }
        #endregion
        #region Handle Data
        private async Task CreatedAsync(DepartmentDto createdUserDto)
        {
            var response = await Http.PostAsJsonAsync($"api/department/create", createdUserDto);
            if (response.IsSuccessStatusCode)
            {
                await Message.Success("Created successfully.");
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                await Message.Error($"Lỗi: {error}");
            }
        }
        private async Task UpdateAsync(DepartmentDto updateUserDto)
        {
            var response = await Http.PutAsJsonAsync("api/department/update", updateUserDto);
            if (response.IsSuccessStatusCode)
            {
                await Message.Success("Update successfully!");
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                await Message.Error($"Lỗi: {error}");
            }
        }
        #endregion
    }
}
