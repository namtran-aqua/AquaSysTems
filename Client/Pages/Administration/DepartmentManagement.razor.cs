using AquaSolution.Client.Common;
using AquaSolution.Client.Modals.Administration.Department;
using AquaSolution.Shared.CommonDto;
using AquaSolution.Shared.Departments;
using Microsoft.AspNetCore.Components;
using System.Net.Http.Json;

namespace AquaSolution.Client.Pages.Administration
{
    public partial class DepartmentManagement
    {
        #region Declaration
        [Inject] private HttpClient? Http { get; set; }
        private List<DepartmentDto>? _listDepartment = new();
        private DepartmentModal _departmentModal = new DepartmentModal();
        #endregion

        #region Innit
        protected override async Task OnInitializedAsync()
        {
            await LoadDataAsync();

        }
        private async Task LoadDataAsync()
        {
            if (Http != null)
                _listDepartment = await Http.GetFromJsonAsync<List<DepartmentDto>>("api/department/get-all");
            await InvokeAsync(StateHasChanged);
        }
        #endregion

        #region Action
        private async Task  CreatedDepartment()
        {
            await _departmentModal.Showmodal(new DepartmentDto(),false);
        }
        private async Task EditDepartment(DepartmentDto departmentDto) 
        {
            await _departmentModal.Showmodal(departmentDto, true);
        }
        private async Task DeleteAsync(DepartmentDto departmentDto)
        {
            var message = $"Are you sure you want to delete the department \"{departmentDto.Name}\"?";

            var confirm = await MessageBox.Confirm(Modal, message);
            if (confirm)
            {
                var response = await Http?.DeleteAsync($"api/department/delete/{departmentDto.Id}")!;
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
