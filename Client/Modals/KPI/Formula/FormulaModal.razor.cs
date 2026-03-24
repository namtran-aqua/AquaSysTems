using AntDesign;
using AquaSolution.Shared.Enum.KPIType;
using AquaSolution.Shared.KPI.Formula;
using Microsoft.AspNetCore.Components;
using System.Net.Http.Json;

namespace AquaSolution.Client.Modals.KPI.Formula
{
    public partial class FormulaModal
    {
        #region Declaration
        private bool IsModalVisible = false;
        [Inject] private HttpClient Http { get; set; }
        [Parameter] public EventCallback OnSave { get; set; }
        private FormulaDto formulaDto = new FormulaDto();
        private Form<FormulaDto> formRef;
        private bool IsEdit { get; set; }
        private List<KPIFormulaType> ListKPIFormulaType = new List<KPIFormulaType>();

        #endregion
        #region Init
        public async Task ShowModal(bool isEdit = false, FormulaDto? FormulaDto = null)
        {
            IsEdit = isEdit;

            if (isEdit)
            {
                formulaDto = FormulaDto ?? new FormulaDto(); 
            }
            else
            {
                formulaDto = new FormulaDto();
      
            }

            IsModalVisible = true;

            GetEnum();
            StateHasChanged();
        }


        private void GetEnum()
        {
            ListKPIFormulaType = Enum.GetValues(typeof(KPIFormulaType))
                    .Cast<KPIFormulaType>()
                    .ToList();
        }
        #endregion
        #region Actions
        private async Task SaveAsync()
        {
            bool isSave= false;
            if(IsEdit)
            {
                isSave = await UpdateAsync();
            }
            else
            {
                isSave = await CreatedAsync();
            }
            if(isSave)
            {
                await OnSave.InvokeAsync();
                IsModalVisible = false;
            }
        }

        private async Task<bool> CreatedAsync()
        {
         
            var response = await Http.PostAsJsonAsync("api/formula/create", formulaDto);

            if (response.IsSuccessStatusCode)
            {
                await Message.Success("Created successfully.");
                return true;
            }
            else
            {
                await Message.Error("Created failed.");
                return false;
            }
        }
        private async Task<bool> UpdateAsync()
        {
            var response = await Http.PutAsJsonAsync("api/formula/update", formulaDto);

            if (response.IsSuccessStatusCode)
            {
                await Message.Success("Updated successfully.");
                return true;
            }
            else
            {
                await Message.Error("Updated failed.");
                return false;
            }
        }

        private void Close()
        {
            IsModalVisible = false;
            StateHasChanged();
        }
        #endregion
    }
}
