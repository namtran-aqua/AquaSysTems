
using AquaSolution.Client.Components.SemiReport.CusPack;
using AquaSolution.Shared.SemiReport;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Net.Http.Json;

namespace AquaSolution.Client.Pages.ScanSemi
{
    public partial class CusPack
    {
        #region Declaration
        [Inject] private HttpClient Http { get; set; }
        private List<CusPackNoDto> _cusPackNo = new();
        private CusPackModal cusPackModal = new();
        public bool Loading { get; set; }
        #endregion
        #region Innit
        protected override async Task OnInitializedAsync()
        {
            await LoadData();

        }
        private async Task LoadData()
        {
            try
            {
                var response = await Http.GetFromJsonAsync<List<CusPackNoDto>>("api/CusPack/get-all");
                _cusPackNo = response ?? new List<CusPackNoDto>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading users: {ex.Message}");
            }
        }
        #endregion
        #region Action
        private async Task CreatedAsync()
        {
            await cusPackModal.ShowModal(false);
        }
        private async Task EditAsync(CusPackNoDto cusPackNoDto)
        {
            await cusPackModal.ShowModal(true, cusPackNoDto);
        }
        private async Task DeleteAsync(int id)
        {
            var confirm = await JS.InvokeAsync<bool>("confirm", "Bạn có chắc chắn muốn xóa không?");
            if (!confirm)
                return;
            var response = await Http.DeleteAsync($"api/CusPack/delete/{id}");
            if (response.IsSuccessStatusCode)
            {
                await Message.Success("Xóa thành công !");
            }
            else
            {
                await Message.Error("Xóa thất bại!");
            }
            await LoadData();
            await InvokeAsync(StateHasChanged);
        }
        #endregion
    }
}
