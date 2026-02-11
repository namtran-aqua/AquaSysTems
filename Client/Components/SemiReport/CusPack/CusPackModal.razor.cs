using AntDesign;
using AquaSolution.Shared.SemiReport;
using Microsoft.AspNetCore.Components;
using System.Net.Http.Json;

namespace AquaSolution.Client.Components.SemiReport.CusPack
{
    public partial class CusPackModal
    {
        #region Declaration
        [Inject] private HttpClient Http { get; set; }
        private bool IsModalVisible = false;
        private CusPackNoDto CusPackNoDto { get; set; } = new();
        [Parameter] public EventCallback OnSave { get; set; }
        private Form<CusPackNoDto> formRef = new();
        private bool IsEdit { get; set; }

        #endregion
        #region Innit
        public async Task ShowModal(bool isEdit, CusPackNoDto? cusPackNoDto = null)
        {
            IsEdit = isEdit;
            CusPackNoDto = new();
            if (cusPackNoDto != null)
            {
                CusPackNoDto = cusPackNoDto;
            }
            IsModalVisible = true;
            await InvokeAsync(StateHasChanged);
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
            var response = await Http.PutAsJsonAsync($"api/CusPack/update", CusPackNoDto);
            if (response.IsSuccessStatusCode)
            {
                await Message.Success("Cập nhật thành công !");
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                await Message.Error($"Cập nhật thất bại ! {error}");
            }
        }
        private async Task CreatedAsync()
        {
            var response = await Http.PostAsJsonAsync("api/CusPack/created", CusPackNoDto);

            if (response.IsSuccessStatusCode)
            {

                await Message.Success("Tạo thành công!");
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                await Message.Error($"Tạo thất bại: {error}");
            }
        }
        #endregion
    }
}
