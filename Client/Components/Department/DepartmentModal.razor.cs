using AntDesign;
using AquaSolution.Shared.Departments;
using AquaSolution.Shared.Menus;
using AquaSolution.Shared.UserManagements;
using Microsoft.AspNetCore.Components;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace AquaSolution.Client.Components.Department
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
            IsModalVisible = true;
            await InvokeAsync(StateHasChanged);
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
                await UpdateAsync(_model);
            }
            else
            {
                await CreatedAsync(_model);
            }
            IsModalVisible = false;
            await OnSave.InvokeAsync();
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
