using AquaSolution.Client.Common;
using AquaSolution.Client.Components.Administration.Position;
using AquaSolution.Shared.CommonDto;
using AquaSolution.Shared.Position;
using Microsoft.AspNetCore.Components;
using System.Net.Http.Json;

namespace AquaSolution.Client.Pages.Administration
{
    public partial class PositionManagement
    {
        #region Declaration
        [Inject] private HttpClient Http { get; set; }
        private List<PositionDto> ListPosition = new();
        private PositionModal positionModal = new PositionModal();
        #endregion

        #region Innit
        protected override async Task OnInitializedAsync()
        {
            await LoadDataAsync();

        }
        private async Task LoadDataAsync()
        {
            ListPosition = await Http.GetFromJsonAsync<List<PositionDto>>("api/position/get-all");
            await InvokeAsync(StateHasChanged);
        }
        #endregion

        #region Action
        private async Task CreatedPosition()
        {
            await positionModal.Showmodal(new PositionDto(), false);
        }
        private async Task EditPosition(PositionDto positionDto)
        {
            await positionModal.Showmodal(positionDto, true);
        }
        private async Task DeleteAsync(PositionDto positionDto)
        {
            var message = $"Are you sure you want to delete the position \" {positionDto.Name} \" ?";
            var confirm = await MessageBox.Confirm(modal, message.ToString());
            if (confirm)
            {
                var response = await Http.DeleteAsync($"api/position/delete/{positionDto.Id}");
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
