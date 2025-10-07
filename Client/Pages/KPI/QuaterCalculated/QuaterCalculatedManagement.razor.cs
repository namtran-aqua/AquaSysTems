using AntDesign;
using AquaSolution.Client.Common;
using AquaSolution.Client.Components.KPI.KPITask;
using AquaSolution.Client.Components.KPI.QuaterCalculated;
using AquaSolution.Shared.Enum;
using AquaSolution.Shared.Enum.KPIType;
using AquaSolution.Shared.KPI.Formula;
using AquaSolution.Shared.KPI.KPITasks;
using AquaSolution.Shared.KPI.QuaterCalculated;
using AquaSolution.Shared.UserManagements;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using System.Net.Http.Json;


namespace AquaSolution.Client.Pages.KPI.QuaterCalculated
{
    public partial class QuaterCalculatedManagement
    {

        #region Declaration
        [Inject] private HttpClient Http { get; set; }
        private List<QuaterCalculatedDto> DataSource = new();
        private Table<QuaterCalculatedDto> tableRef;
        private QuaterCalculatedModal quaterCalculatedModal;
        #endregion
        #region Innit
        protected override async Task OnInitializedAsync()
        {
            await LoadData();
        }
        private async Task LoadData()
        {
            var result = await Http.GetFromJsonAsync<List<QuaterCalculatedDto>>("api/quaterCalculated/get-list");

            if (result is not null)
            {
                DataSource = result;
            }
            else
            {
                DataSource = new();
            }

            StateHasChanged();
        }
        #endregion
        #region Actions
        private async Task CreatedAsync()
        {

            await quaterCalculatedModal.ShowModal();
        }
        private async Task EditAsync(QuaterCalculatedDto QuaterCalculatedDto)
        {
            await quaterCalculatedModal.ShowModal(true, QuaterCalculatedDto);
        }
        private async Task DeleteAsync(QuaterCalculatedDto QuaterCalculatedDto)
        {
            var confirm = await JS.InvokeAsync<bool>("confirm", "Bạn có chắc chắn muốn xóa không?");
            if (!confirm)
                return;
            var response = await Http.DeleteAsync($"api/quaterCalculated/delete/{QuaterCalculatedDto.Id}");

            if (response.IsSuccessStatusCode)
            {
                await LoadData();
                await Message.Success("Deleted successfully");
            }
            else
            {
                await Message.Error("An unexpected error occurred");
            }
        }

        #endregion
      
    }
}
