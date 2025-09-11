using AntDesign;
using AquaSolution.Shared.CommonDto;
using AquaSolution.Shared.Enum;
using AquaSolution.Shared.ITSuport.RequestSuportCategory;
using AquaSolution.Shared.Permissions;
using AquaSolution.Shared.UserManagements;
using Microsoft.AspNetCore.Components;
using System.Net.Http.Json;

namespace AquaSolution.Client.Components.ITSuport.RequestSuportCategory
{
    public partial class RequestSuportCategoryModal
    {
        #region Declaration
        [Inject] private HttpClient Http { get; set; }
        private bool IsModalVisible = false;
        private RequestSuportCategoryDto RequestSuportCategoryDto { get; set; } = new();
        [Parameter] public EventCallback OnSave { get; set; }
        private Form<RequestSuportCategoryDto> formRef = new();
        private List<UserContributerDto> ListTechnician = new List<UserContributerDto>();
        private bool IsView { get; set; }
        private bool IsEdit { get; set; }

        #endregion
        #region Innit
        public async Task ShowModal(bool isView, bool isEdit, RequestSuportCategoryDto? requestSuportCategoryDto = null)
        {
            IsView = isView;
            IsEdit = isEdit;
            await LoadTechnician();
            if (requestSuportCategoryDto != null)
            {
                RequestSuportCategoryDto = requestSuportCategoryDto;
            }
            IsModalVisible = true;
            await InvokeAsync(StateHasChanged);
        }
        private async Task LoadTechnician()
        {
            ListTechnician = new List<UserContributerDto>();
            var data = await Http.GetFromJsonAsync<List<UserContributerDto>>("api/user/get-contributer");
            ListTechnician = data.Where(x => x.DepartmentType == DepartmentType.IT).ToList();
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
            if (IsEdit)
            {
                await UpdateAsync();
            }
            else
            {
                await CreatedAsync();
            }
            await OnSave.InvokeAsync();
            IsModalVisible = false;
        }
        private async Task Close()
        {
            IsModalVisible = false;
        }
        #endregion
        #region HandleData
        private async Task UpdateAsync()
        {
    

            var response = await Http.PutAsJsonAsync($"api/RequestSuportCategory/update", RequestSuportCategoryDto);

            if (response.IsSuccessStatusCode)
            {
                await Message.Success("Updated successfully !");
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                await Message.Error($"Updated failed: {error}");
            }

        }
        private async Task CreatedAsync()
        {

            var response = await Http.PostAsJsonAsync("api/RequestSuportCategory/created", RequestSuportCategoryDto);

            if (response.IsSuccessStatusCode)
            {

                await Message.Success("Created successfully !");
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                await Message.Error($"Create failed: {error}");
            }
        }
        #endregion
    }
}
