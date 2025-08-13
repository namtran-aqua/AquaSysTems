using AquaSolution.Client.Common;
using AquaSolution.Client.Components.ManageMedicalRooms.Products;
using AquaSolution.Shared.CommonDto;
using AquaSolution.Shared.Enum;
using AquaSolution.Shared.ManageMedicalRooms.Products;
using Microsoft.AspNetCore.Components;
using System.Net.Http.Json;

namespace AquaSolution.Client.Pages.ManageMedicalRooms.Productions
{
    public partial class Medicine
    {
        #region Declaration
        [Inject] private HttpClient Http { get; set; }
        private List<ProductDto> _products = new List<ProductDto>();
        private ProductModal ProductModal =new ProductModal();
        #endregion
        #region Innit
        protected override async Task OnInitializedAsync()
        {
            await LoadData();
        }
        private async Task LoadData()
        {
            _products = await Http.GetFromJsonAsync<List<ProductDto>>("api/product/get-all-by-import");
            _products = _products.Where(x=>x.ProductType == ProductType.Medicine).ToList();
            await InvokeAsync(StateHasChanged);
        }
        #endregion
        #region Action 
        private async Task CreatedAsync()
        {
            await ProductModal.Showmodal( new ProductDto(),false,ProductType.Medicine);
        }

        private async Task DeleteAsync(ProductDto productDto)
        {
            var message = $"Are you sure you want to delete the medicine \" {productDto.Name} \" ?";
            var confirm = await MessageBox.Confirm(modal, message.ToString());
            if (confirm)
            {
                var response = await Http.DeleteAsync($"api/product/delete/{productDto.Id}");
                await LoadData();
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
        private async Task EditAsync(ProductDto productDto)
        {
            await ProductModal.Showmodal(productDto, true, ProductType.Medicine);
        }
        #endregion

    }
}
