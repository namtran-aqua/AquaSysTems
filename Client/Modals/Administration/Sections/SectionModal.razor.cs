using AntDesign;
using AquaSolution.Shared.Administration.Sections;
using AquaSolution.Shared.Departments;
using Microsoft.AspNetCore.Components;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace AquaSolution.Client.Modals.Administration.Sections
{
    public partial class SectionModal
    {
        #region Declaration
        [Inject] private HttpClient Http { get; set; } = new();
        private bool IsModalVisible = false;
        private SectionDto _model = new SectionDto();
        [Parameter] public EventCallback OnSave { get; set; }
        private Form<SectionDto> formRef = new();
        private bool IsEdit {  get; set; }
        private string Title { get; set; }
        private List<DepartmentDto> ListDepartments = new List<DepartmentDto>();
        #endregion

        #region Innit
        public async Task Showmodal(SectionDto sectionDto, bool isEdit)
        {
            IsEdit = isEdit;
            if (IsEdit)
            {
                Title = "Edit Section";
            }
            else
            {
                Title = "Created Section";
            }
            _model = sectionDto;
            await GetDepartments();
            IsModalVisible = true;
            await InvokeAsync(StateHasChanged);
        }
        private async Task GetDepartments()
        {
            try
            {
                var depts = await Http.GetFromJsonAsync<List<DepartmentDto>>("api/department/get-all");
                if (depts != null)
                {
                    ListDepartments = depts;
                }
            }
            catch (Exception ex)
            {
                await Message.Error($"Lỗi khi tải phòng ban: {ex.Message}");
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
                await UpdateAsync(_model);
            }
            else
            {
                await CreatedAsync(_model);
            }
        }
        #endregion

        #region Handle Data
        private async Task CreatedAsync(SectionDto createdDto)
        {
            var response = await Http.PostAsJsonAsync($"api/section", createdDto);
            if (response.IsSuccessStatusCode)
            {
                await Message.Success("Created successfully.");
                IsModalVisible = false;
                await OnSave.InvokeAsync();
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                await Message.Error($"Lỗi: {error}");
            }
        }
        private async Task UpdateAsync(SectionDto updateDto)
        {
            var response = await Http.PutAsJsonAsync($"api/section/{updateDto.Id}", updateDto);
            if (response.IsSuccessStatusCode)
            {
                await Message.Success("Update successfully!");
                IsModalVisible = false;
                await OnSave.InvokeAsync();
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
