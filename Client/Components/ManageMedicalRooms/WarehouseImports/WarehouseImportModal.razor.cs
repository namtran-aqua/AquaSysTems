using AntDesign;
using AquaSolution.Client.Common;
using AquaSolution.Shared.Enum;
using AquaSolution.Shared.ManageMedicalRooms.Products;
using AquaSolution.Shared.ManageMedicalRooms.WarehouseImports;
using AquaSolution.Shared.UserManagements;
using Microsoft.AspNetCore.Components;
using System.Net.Http.Json;

namespace AquaSolution.Client.Components.ManageMedicalRooms.WarehouseImports
{
    public partial class WarehouseImportModal
    {
        #region Declaration
        private bool IsVisible { get; set; }
        private Form<CreatedWarehouseImportDto> formRef;
        private Table<WarehouseImportDetailDto> tableRef;
        [Inject] private HttpClient Http { get; set; }
        private List<ProductDto> _products = new List<ProductDto>();
        private CreatedWarehouseImportDto createdWarehouseImportDto { get; set; } = new();
        private LoadWarehouseImportDto LoadWarehouseImportDto { get; set; } = new();
        private bool IsView { get; set; }
        private UserDto CurrenUser { get; set; }
        [Parameter] public EventCallback OnSaved { get; set; }
        #endregion
        #region Innit
        public async Task ShowModalAsync(LoadWarehouseImportDto loadWarehouseImport, bool isView)
        {
            createdWarehouseImportDto = new CreatedWarehouseImportDto();
            IsVisible = true;
            LoadWarehouseImportDto = loadWarehouseImport;
            IsView = isView;
            await LoadProduct();
            await SetDataView();
            GetWarehouseImportType();
            var CurrenUserClass = new CurrenUser(Http, AuthStateProvider);
            CurrenUser = await CurrenUserClass.LoadCurrenUser();
            await InvokeAsync(StateHasChanged);
        }
        private async Task LoadProduct()
        {
            _products = await Http.GetFromJsonAsync<List<ProductDto>>("api/product/get-all-by-import");
        }
        private WarehouseImportDetailDto? _currentDetail;
        private ProductDto? _product;
        private ProductDto? ProductDto 
        { 
            get => _product;
            set
            {
                if (value != _product)
                {
                    _product = value;
                }
       
            }
        }
        private async Task SetDataView()
        {
            if (IsView)
            {
                createdWarehouseImportDto = new();
                createdWarehouseImportDto.WarehouseImportDto.Code = LoadWarehouseImportDto.Code;
                createdWarehouseImportDto.WarehouseImportDto.Name = LoadWarehouseImportDto.Name;
                createdWarehouseImportDto.WarehouseImportDto.Note = LoadWarehouseImportDto.Note;
                createdWarehouseImportDto.WarehouseImportDto.CreatedDate = LoadWarehouseImportDto.CreatedDate;
                createdWarehouseImportDto.WarehouseImportDto.CreatedBy = LoadWarehouseImportDto.CreatedBy;
                createdWarehouseImportDto.WarehouseImportDto.Description = LoadWarehouseImportDto.Description;
                createdWarehouseImportDto.WarehouseImportDto.WarehouseImportType = LoadWarehouseImportDto.WarehouseImportType;
                var dataDetail = await Http.GetFromJsonAsync<List<LoadWarehouseImportDetailDto>>($"api/WarehouseImport/get-detail/{LoadWarehouseImportDto.Id}");
                if (dataDetail.Any())
                {
                    foreach (var detail in dataDetail)
                    {
                        var matchedProduct = _products.FirstOrDefault(p => p.Id == detail.ProductId);
                        createdWarehouseImportDto.WarehouseImportDetailDtos.Add(new WarehouseImportDetailDto
                        {
                            Id = detail.Id,
                            DateManufacture = detail.DateManufacture,
                            ExpiryDate = detail.ExpiryDate,
                            Quantity = detail.Quantity,
                            ProductId = detail.ProductId,
                            ProductType = detail.ProductType,
                            Unit =detail.Unit,
                        });
                    }
                }

            }
            else
            {

            }
        }
        private List<WarehouseImportType> ListWarehouseImportType = new List<WarehouseImportType>();
        private void GetWarehouseImportType()
        {
            ListWarehouseImportType = Enum.GetValues(typeof(WarehouseImportType))
                    .Cast<WarehouseImportType>()
                    .ToList();
            if (createdWarehouseImportDto != null)
            {
                WarehouseImportTypeValue = ListWarehouseImportType
                         .FirstOrDefault(x => x == createdWarehouseImportDto.WarehouseImportDto.WarehouseImportType);
            }
            else
            {
                WarehouseImportTypeValue = ListWarehouseImportType.First();
            }
        }
        private WarehouseImportType _warehouseImportTypeValue;
        private WarehouseImportType WarehouseImportTypeValue
        {
            get => _warehouseImportTypeValue;
            set
            {
                if (_warehouseImportTypeValue != value)
                {
                    _warehouseImportTypeValue = value;
                }

            }
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
            if (!createdWarehouseImportDto.WarehouseImportDetailDtos.Any())
            {
                await Message.Error("Details cannot be left blank !");
                return;
            }
            foreach (var itemDetail in createdWarehouseImportDto.WarehouseImportDetailDtos)
            {
                if (itemDetail.ExpiryDate == null)
                {
                    await Message.Error("ExpiryDate cannot be left blank !");
                    return;
                }
                if (itemDetail.Quantity <= 0)
                {
                    await Message.Error("Quantity Must be greater than 0");
                    return;
                }
                if (itemDetail.ProductId== Guid.Empty)
                {
                    await Message.Error("product cannot be left blank");
                    return;
                }

            }
            createdWarehouseImportDto.WarehouseImportDto.CreatedBy = CurrenUser.Id;
            var response = await Http.PostAsJsonAsync("api/WarehouseImport/import", createdWarehouseImportDto);
            if (response.IsSuccessStatusCode)
            {
                await Message.Success("Success!");
                IsVisible = false;
                await OnSaved.InvokeAsync();
            }
            else
            {
                await Message.Error("Error!");
            }
        }
        private void Close()
        {
            IsVisible = false;
            StateHasChanged();
        }
        private void AddDetailRow()
        {
            var detail = new WarehouseImportDetailDto();
            createdWarehouseImportDto.WarehouseImportDetailDtos.Add(detail);
        }
        private void OnProductChanged(WarehouseImportDetailDto detail, ProductDto? product)
        {
            if (product != null)
            {
                detail.ProductId = product.Id;
                detail.ProductType = product.ProductType;
                detail.Unit = product.Unit;
            }
        }

        private async Task RemoveDetailRow(WarehouseImportDetailDto item)
        {
            if (createdWarehouseImportDto.WarehouseImportDetailDtos.Count > 0)
            {
                createdWarehouseImportDto.WarehouseImportDetailDtos.Remove(item);
            }
        }
        #endregion

    }
}
