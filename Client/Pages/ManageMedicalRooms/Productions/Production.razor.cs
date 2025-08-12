using AquaSolution.Client.Common;
using AquaSolution.Client.Components.ManageMedicalRooms.Products;
using AquaSolution.Shared.CommonDto;
using AquaSolution.Shared.Enum;
using AquaSolution.Shared.ManageMedicalRooms.Products;
using AquaSolution.Shared.UserManagements;
using Microsoft.AspNetCore.Components;
using System.Net.Http.Json;

namespace AquaSolution.Client.Pages.ManageMedicalRooms.Productions
{
    public partial class Production
    {
        #region Declaration
        [Inject] private HttpClient Http { get; set; }
        private List<ProductDto> _products = new List<ProductDto>();
        private ProductModal ProductModal =new ProductModal();
        private HasPermission hasPermission = new();
        private Guid PageId { get; set; }
        private bool Created { get; set; }
        private bool Edited { get; set; }
        private bool Deleted { get; set; }
        private UserDto CurrenUser { get; set; }
        #endregion

        #region Innit
        protected override async Task OnInitializedAsync()
        {
            await LoadData();
            await CheckPermission();
        }
        private async Task LoadData()
        {
            _products = await Http.GetFromJsonAsync<List<ProductDto>>("api/product/get-all-by-import");
            await InvokeAsync(StateHasChanged);
        }
        private async Task GetPage()
        {
            var baseUri = new Uri(Navigation.BaseUri);
            var uri = new Uri(Navigation.Uri);

            var basePath = baseUri.AbsolutePath.TrimEnd('/');
            var fullPath = uri.AbsolutePath;

            string currentPath;
            if (!string.IsNullOrEmpty(basePath))
                currentPath = fullPath.Replace(basePath, "");
            else
                currentPath = fullPath;

            currentPath = currentPath.TrimStart('/');

            PageId = await Http.GetFromJsonAsync<Guid>($"api/Page/GetPageIdByUrl/{currentPath}");

        }
        private async Task CheckPermission()
        {
            await GetPage();
            var CurrenUserClass = new CurrenUser(Http, AuthStateProvider);
            CurrenUser = await CurrenUserClass.LoadCurrenUser();
            Created = await hasPermission.CheckPermissions(PageId, PermissionActionType.Add.ToString(), CurrenUser);
            Edited = await hasPermission.CheckPermissions(PageId, PermissionActionType.Edit.ToString(), CurrenUser);
            Deleted = await hasPermission.CheckPermissions(PageId, PermissionActionType.Delete.ToString(), CurrenUser);

        }
        #endregion
        #region Action 
        private async Task CreatedAsync()
        {
            await ProductModal.Showmodal( new ProductDto(),false);
        }

        private async Task DeleteAsync(ProductDto productDto)
        {
            var message = $"Bạn có muốn xóa product \" {productDto.Name} \" không?";
            var confirm = await MessageBox.Confirm(modal, message.ToString());
            if (confirm)
            {
                var response = await Http.DeleteAsync($"api/product/delete/{productDto.Id}");
                await LoadData();
                var content = await response.Content.ReadFromJsonAsync<ApiResponse>();
                if (response.IsSuccessStatusCode)
                {
                    await Message.Success(content?.message ?? "Xóa thành công");
                }
                else
                {
                    await Message.Error(content?.message ?? "Có lỗi xảy ra");
                }
            }
            await InvokeAsync(StateHasChanged);

        }
        private async Task EditAsync(ProductDto productDto)
        {
            await ProductModal.Showmodal(productDto, true);
        }
        #endregion

    }
}
