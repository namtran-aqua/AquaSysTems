using AquaSolution.Client.Common;
using AquaSolution.Client.Components.Administration.Department;
using AquaSolution.Shared.CommonDto;
using AquaSolution.Shared.Departments;
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
            await departmentModal.Showmodal(deparmentDto, true);
        }
        private async Task DeleteAsync(DepartmentDto deparmentDto)
        {
            var message = $"Are you sure you want to delete the department \"{deparmentDto.Name}\"?";

            var confirm = await MessageBox.Confirm(modal, message.ToString());
            if (confirm)
            {
                var response = await Http.DeleteAsync($"api/department/delete/{deparmentDto.Id}");
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
