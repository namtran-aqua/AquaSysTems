using AntDesign;
using AquaSolution.Client.Modals.KPI.Formula;
using AquaSolution.Shared.KPI.Formula;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Net.Http.Json;



namespace AquaSolution.Client.Pages.KPI.KPIFormula
{
    public partial class KPIFormulaManagement
    {

        #region Declaration
        [Inject] private HttpClient Http { get; set; }
        private List<FormulaDto> DataSource = new();
        private Table<FormulaDto> tableRef;
        private FormulaModal formulaModal;
        #endregion
        #region Innit
        protected override async Task OnInitializedAsync()
        {
            await LoadData();
        }
        private async Task LoadData()
        {
            var result = await Http.GetFromJsonAsync<List<FormulaDto>>("api/formula/get-list");

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

          await formulaModal.ShowModal();
        }
        private async Task EditAsync(FormulaDto kPITaskDto)
        {
            await formulaModal.ShowModal(true, kPITaskDto);
        }
        private async Task DeleteAsync(FormulaDto kPITaskDto)
        {
            var confirm = await JS.InvokeAsync<bool>("confirm", "Bạn có chắc chắn muốn xóa không?");
            if (!confirm)
                return;
            var response = await Http.DeleteAsync($"api/formula/delete/{kPITaskDto.Id}");

            if (response.IsSuccessStatusCode)
            {
                await LoadData();
                await Message.Success( "Deleted successfully");
            }
            else
            {
                await Message.Error( "An unexpected error occurred");
            }
        }

        #endregion
      
    }
}
