using AquaSolution.Client.Common;
using AquaSolution.Client.Modals.ITSuport.RequestSuportCategory;
using AquaSolution.Shared.ITSuport.RequestSuportCategory;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Net.Http.Json;

namespace AquaSolution.Client.Pages.ITSuport.RequestCategory
{
    public partial class RequestCategory
    {
        #region Declaration
        [Inject] private HttpClient Http { get; set; }
        private List<RequestSuportCategoryDto> _requestSuportCategories = new();
        private RequestSuportCategoryModal requestSuportCategoryModal = new();
        #endregion
        #region Innit
        protected override async Task OnInitializedAsync()
        {
            await LoadData();
        }
        private async Task LoadData()
        {
            var response = await Http.GetFromJsonAsync<List<RequestSuportCategoryDto>>("api/RequestSuportCategory/get-all");

            if (response != null)
            {
                _requestSuportCategories = response;
            }
            else
            {
                _requestSuportCategories = new List<RequestSuportCategoryDto>();
            }

        }
        #endregion
        #region Action
        private async Task CreatedAsync()
        {
            await requestSuportCategoryModal.ShowModal(false, false);
        }
        private async Task EditAsync(RequestSuportCategoryDto requestSuportCategoryDto)
        {
            await requestSuportCategoryModal.ShowModal(false, true, requestSuportCategoryDto);
        }
        private async Task DeleteAsync(Guid id)
        {

            var confirm = await JS.InvokeAsync<bool>("confirm", "Bạn có chắc chắn muốn xóa không?");
            if (!confirm)
                return;

            var response = await Http.DeleteAsync($"api/RequestSuportCategory/delete/{id}");

            if (response.IsSuccessStatusCode)
            {
                await Message.Success("Delete successfully !");
            }
            else
            {
                await Message.Error("Delete failed !");
            }
            await LoadData();
            await InvokeAsync(StateHasChanged);

        }
        private async Task ViewAsync(RequestSuportCategoryDto requestSuportCategoryDto)
        {
            await requestSuportCategoryModal.ShowModal(true, false, requestSuportCategoryDto);
        }
        #endregion
    }
}
