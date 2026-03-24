using AntDesign;
using AquaSolution.Client.Common;
using AquaSolution.Shared.Enum;
using AquaSolution.Shared.ManageMedicalRooms.Products;
using AquaSolution.Shared.UserManagements;
using Microsoft.AspNetCore.Components;
using System.Net.Http.Json;

namespace AquaSolution.Client.Modals.ManageMedicalRooms.Products
{
    public partial class ProductModal
    {
        #region Declaration
        [Inject] private HttpClient Http { get; set; } = new();
        private bool IsModalVisible = false;
        private ProductDto _model = new ProductDto();
        [Parameter] public EventCallback OnSave { get; set; }
        private Form<ProductDto> formRef = new();
        private bool IsEdit {  get; set; }
        private string Title { get; set; }
        private UserDto CurrenUser { get; set; }
        private List<ProductType> ListProductType = new List<ProductType>();
        private ProductType? ProductTypeParam { get; set; }
        private bool IsDisableType { get; set; }
        #endregion
        #region Innit
        public async Task Showmodal(ProductDto productDto,bool isEdit, ProductType? productType = null)
        {
            IsEdit = isEdit;
            ProductTypeParam = productType;
            IsDisableType = productType != null;     
            if (IsEdit)
            {
                Title = "Edit Product";
            }
            else
            {
                Title = "Created Product";
            }
            _model = productDto;
            GetProductType();
            IsModalVisible = true;
            var CurrenUserClass = new CurrenUser(Http, AuthStateProvider);
            CurrenUser = await CurrenUserClass.LoadCurrenUser();
            await InvokeAsync(() => StateHasChanged());
        }
        
 
        private void GetProductType()
        {
            ListProductType = Enum.GetValues(typeof(ProductType))
                    .Cast<ProductType>()
                    .ToList();
            if (_model != null)
            {
                ProductType = ListProductType.FirstOrDefault(x => x == _model.ProductType);
            }
            else
            {
                ProductType = ListProductType.FirstOrDefault();
            }
            if (ProductTypeParam != null)
            {
                ProductType = ListProductType.FirstOrDefault(x => x == ProductTypeParam);
            }
          

        }
        #endregion
        #region Action
        private void HandleCancel() => IsModalVisible = false;

        private async Task SaveAsync()
        {
            var valid = formRef.Validate();
            if (!valid)
            {
                return;
            }
            if (IsEdit)
            {
                _model.UpdatedDate = DateTime.Now;
                _model.UpdateBy = CurrenUser.FullName;
                _model.ProductType = ProductType;
                await UpdateAsync(_model);
            }
            else
            {
                _model.CreatedDate = DateTime.Now;
                _model.CreatedBy = CurrenUser.FullName;
                _model.ProductType = ProductType;
                await CreatedAsync(_model);
            }
            IsModalVisible = false;
            await OnSave.InvokeAsync();
        }
        private ProductType _productType;
        private ProductType ProductType
        {
            get => _productType;
            set
            {
                if (_productType != value)
                {
                    _productType = value;
                }

            }
        }

        #endregion
        #region Handle Data
        private async Task CreatedAsync(ProductDto createdUserDto)
        {
            var response = await Http.PostAsJsonAsync($"api/product/create", createdUserDto);
            if (response.IsSuccessStatusCode)
            {
                await Message.Success("Created successfully.");
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                await Message.Error($"Lỗi: {error}");
            }
        }
        private async Task UpdateAsync(ProductDto updateUserDto)
        {
            var response = await Http.PutAsJsonAsync("api/product/update", updateUserDto);
            if (response.IsSuccessStatusCode)
            {
                await Message.Success("Update successfully!");
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                await Message.Error($"Lỗi: {error}");
            }
        }
        #endregion
    }
}
