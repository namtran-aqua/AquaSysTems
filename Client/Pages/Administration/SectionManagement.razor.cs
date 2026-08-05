using AquaSolution.Client.Common;
using AquaSolution.Client.Modals.Administration.Sections;
using AquaSolution.Shared.CommonDto;
using AquaSolution.Shared.Administration.Sections;
using Microsoft.AspNetCore.Components;
using System.Net.Http.Json;

namespace AquaSolution.Client.Pages.Administration
{
    public partial class SectionManagement
    {
        #region Declaration
        [Inject] private HttpClient? Http { get; set; }
        private List<SectionDto>? _listSection = new();
        private SectionModal _sectionModal = new SectionModal();
        #endregion

        #region Innit
        protected override async Task OnInitializedAsync()
        {
            await LoadDataAsync();
        }
        private async Task LoadDataAsync()
        {
            if (Http != null)
                _listSection = await Http.GetFromJsonAsync<List<SectionDto>>("api/section/get-all");
            await InvokeAsync(StateHasChanged);
        }
        #endregion

        #region Action
        private async Task CreatedSection()
        {
            await _sectionModal.Showmodal(new SectionDto(), false);
        }
        private async Task EditSection(SectionDto sectionDto) 
        {
            await _sectionModal.Showmodal(sectionDto, true);
        }
        private async Task DeleteAsync(SectionDto sectionDto)
        {
            var message = $"Are you sure you want to delete the section \"{sectionDto.Name}\"?";

            var confirm = await MessageBox.Confirm(Modal, message);
            if (confirm)
            {
                var response = await Http?.DeleteAsync($"api/section/delete/{sectionDto.Id}")!;
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
