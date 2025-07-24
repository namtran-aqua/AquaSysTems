using AquaSolution.Client.Common;
using AquaSolution.Client.Components.Department;
using AquaSolution.Client.Components.Users;
using AquaSolution.Shared.CommonDto;
using AquaSolution.Shared.Departments;
using AquaSolution.Shared.UserManagements;
using Microsoft.AspNetCore.Components;
using System.Net.Http.Json;

namespace AquaSolution.Client.Pages.Administration
{
    public partial class DepartmentManagement
    {
        #region Declaration
        [Inject] private HttpClient Http { get; set; }
        private List<DepartmentDto> ListDepartment = new();
        private DepartmentModal departmentModal = new DepartmentModal();
        #endregion

        #region Innit
        protected override async Task OnInitializedAsync()
        {
            await LoadDataAsync();

        }
        private async Task LoadDataAsync()
        {
            ListDepartment = await Http.GetFromJsonAsync<List<DepartmentDto>>("api/department/get-all");
            await InvokeAsync(StateHasChanged);
        }
        #endregion

        #region Action
        private async Task  CreatedDeparment()
        {
            await departmentModal.Showmodal(new DepartmentDto(),false);
        }
        private async Task EditDeparment(DepartmentDto deparmentDto) 
        {
            await departmentModal.Showmodal(deparmentDto, false);
        }
        private async Task DeleteAsync(DepartmentDto deparmentDto)
        {
            var message = $"Bạn có muốn xóa department \" {deparmentDto.Name} \" không?";
            var confirm = await MessageBox.Confirm(modal, message.ToString());
            if (confirm)
            {
                var response = await Http.DeleteAsync($"api/department/delete/{deparmentDto.Id}");
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
