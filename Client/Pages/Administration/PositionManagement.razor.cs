using AquaSolution.Client.Common;
using AquaSolution.Client.Modals.Administration.Position;
using AquaSolution.Shared.CommonDto;
using AquaSolution.Shared.Position;
using Microsoft.AspNetCore.Components;
using System.Net.Http.Json;

namespace AquaSolution.Client.Pages.Administration
{
    public partial class PositionManagement
    {
        #region Declaration
        [Inject] private HttpClient? Http { get; set; }
        private List<PositionDto>? _listPosition = new();
        private PositionModal _positionModal = new PositionModal();
        #endregion

        #region Innit
        protected override async Task OnInitializedAsync()
        {
            await LoadDataAsync();

        }
        private async Task LoadDataAsync()
        {
            if (Http != null) _listPosition = await Http.GetFromJsonAsync<List<PositionDto>>("api/position/get-all");
            await InvokeAsync(StateHasChanged);
        }
        #endregion

        #region Action
        private async Task CreatedPosition()
        {
            await _positionModal.Showmodal(new PositionDto(), false);
        }
        private async Task EditPosition(PositionDto positionDto)
        {
            await _positionModal.Showmodal(positionDto, true);
        }
        private async Task DeleteAsync(PositionDto positionDto)
        {
            var message = $"Are you sure you want to delete the position \" {positionDto.Name} \" ?";
            var confirm = await MessageBox.Confirm(Modal, message);
            if (confirm)
            {
                var response = await Http?.DeleteAsync($"api/position/delete/{positionDto.Id}")!;
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
