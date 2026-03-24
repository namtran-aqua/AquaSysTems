using AntDesign;
using AquaSolution.Client.Common;
using AquaSolution.Shared.Enum;
using AquaSolution.Shared.ManageMedicalRooms.Products;
using AquaSolution.Shared.ManageMedicalRooms.WarehouseExports;
using AquaSolution.Shared.ManageMedicalRooms.WarehouseImports;
using AquaSolution.Shared.UserManagements;
using Microsoft.AspNetCore.Components;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Http.Json;
using System.Reflection;
using System.Text;
using System.Text.Json;

namespace AquaSolution.Client.Modals.ManageMedicalRooms.WarehouseExports
{
    public partial class WarehouseExportModal
    {
        #region Declaration
        private bool IsVisible { get; set; }
        private Form<CreatedWarehouseExportDto> formRef;
        private Table<WarehouseExportDetailDto> tableRef;
        [Inject] private HttpClient Http { get; set; }
        private List<ProductExportDto> _products = new List<ProductExportDto>();
        private List<ProductExportDto> _product2 = new List<ProductExportDto>();
        private List<ProductExportDto> _soucreproduct = new List<ProductExportDto>();

        private CreatedWarehouseExportDto createdWarehouseExportDto { get; set; } = new();
        private LoadWarehouseExportDto LoadWarehouseExportDto { get;set; } = new();
        private bool IsView { get; set; }
        private UserDto CurrenUser { get; set; }
       [Parameter] public EventCallback OnSaved { get; set; }
        #endregion
        #region Innit
        public async Task ShowModalAsync(LoadWarehouseExportDto loadWarehouseExport, bool isView)
        {
            createdWarehouseExportDto = new CreatedWarehouseExportDto();
            IsVisible = true;
            LoadWarehouseExportDto = loadWarehouseExport;
            IsView = isView;
         
            await LoadProduct();
            if (IsView)
            {
                _soucreproduct = _product2;
            }
            else
            {
                _soucreproduct = _products;
            }
            await SetDataView();
            GetWarehouseExportType();
            var CurrenUserClass = new CurrenUser(Http, AuthStateProvider);
            CurrenUser = await CurrenUserClass.LoadCurrenUser();
            await InvokeAsync(StateHasChanged);
        }
        private async Task LoadProduct()
        {
            var data = await Http.GetFromJsonAsync<List<ProductExportDto>>("api/product/get-all-by-export");
            _products = data.
                Where(x => x.Quantity > 0)
                .GroupBy(x => new
                {
                    ExpirationDate = x.ExpirationDate?.Date,
                    ManufacturingDate = x.ManufacturingDate?.Date
                }).SelectMany(g => g)
                .ToList();

            _product2 = data.ToList();
        }
        private async Task SetDataView()
        {
            if (IsView)
            {
                createdWarehouseExportDto = new();
                createdWarehouseExportDto.WarehouseExportDto.Name = LoadWarehouseExportDto.Name;
                createdWarehouseExportDto.WarehouseExportDto.Note = LoadWarehouseExportDto.Note;
                createdWarehouseExportDto.WarehouseExportDto.CreatedDate = LoadWarehouseExportDto.CreatedDate;
                createdWarehouseExportDto.WarehouseExportDto.CreatedBy = LoadWarehouseExportDto.CreatedBy;
                createdWarehouseExportDto.WarehouseExportDto.Description = LoadWarehouseExportDto.Description;
                createdWarehouseExportDto.WarehouseExportDto.WarehouseExportType =LoadWarehouseExportDto.WarehouseExportType;
                var dataDetail = await Http.GetFromJsonAsync<List<LoadWarehouseExportDetailDto>>($"api/WarehouseExport/get-detail/{LoadWarehouseExportDto.Id}");
                if (dataDetail.Any())
                {
                    foreach (var detail in dataDetail)
                    {
                        createdWarehouseExportDto.WarehouseExportDetailDtos.Add(new WarehouseExportDetailDto
                        {
                            Id = detail.Id,
                            Quantity = detail.Quantity,
                            ProductId = detail.ProductId,
                            ProductType = detail.ProductType,
                            Unit = detail.Unit,
                            DateManufacture = detail.DateManufacture,
                            ExpiryDate = detail.ExpiryDate,
                            ProductName = detail.ProductName
                        });
                    }
                }

            }
            else
            {

            }
        }
        private List<WarehouseExportType> ListWarehouseExportType = new List<WarehouseExportType>();
        private void GetWarehouseExportType()
        {
            ListWarehouseExportType = Enum.GetValues(typeof(WarehouseExportType))
                    .Cast<WarehouseExportType>()
                    .ToList();
            if (createdWarehouseExportDto != null)
            {
                WarehouseExportTypeValue = ListWarehouseExportType
                         .FirstOrDefault(x => x == createdWarehouseExportDto.WarehouseExportDto.WarehouseExportType);
            }
            else
            {
                WarehouseExportTypeValue = ListWarehouseExportType.First();
            }
        }
        private string GetPurposeTypeLabel(WarehouseExportType type)
        {
            return EnumExtensions.GetDisplayName(type);
        }
        private WarehouseExportType _warehouseImportTypeValue;
        private WarehouseExportType WarehouseExportTypeValue
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
            var stringBuilder = new StringBuilder();
            stringBuilder.Append("Product :");
            var check = 0;
            if (!createdWarehouseExportDto.WarehouseExportDetailDtos.Any())
            {
                await Message.Error("Details cannot be left blank !");
                return;
            }

            foreach (var itemDetail in createdWarehouseExportDto.WarehouseExportDetailDtos)
            {
   
                if (itemDetail.Quantity <= 0)
                {
                    await Message.Error("Quantity must be greater than 0");
                    return;
                }

                if (itemDetail.ProductId == Guid.Empty)
                {
                    await Message.Error("Product cannot be left blank");
                    return;
                }
                var checkQuantity = _products.FirstOrDefault(x=>x.Id == itemDetail.ProductId 
                && x.ManufacturingDate == itemDetail.DateManufacture && x.ExpirationDate ==itemDetail.ExpiryDate);
                if (checkQuantity != null) 
                {
                    if(checkQuantity.Quantity < itemDetail.Quantity) 
                    {
                        stringBuilder.AppendLine(itemDetail.ProductName.ToString());
                        stringBuilder.Append($" Input quantity {itemDetail.Quantity} and actual quantity {checkQuantity.Quantity.ToString("0")} Insufficient stock  ;");
                        check += 1;
                    }
                }
            }
            if(check > 0)
            {

               await MessageBox.Error(modal, stringBuilder.ToString());
                return;
            }

            createdWarehouseExportDto.WarehouseExportDto.CreatedBy = CurrenUser.Id;
            createdWarehouseExportDto.WarehouseExportDto.WarehouseExportType = _warehouseImportTypeValue;
            var response = await Http.PostAsJsonAsync("api/WarehouseExport/export", createdWarehouseExportDto);

            if (response.IsSuccessStatusCode)
            {
                await Message.Success("Success!");
                IsVisible = false;
                await OnSaved.InvokeAsync();
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();

                try
                {
                    using var doc = JsonDocument.Parse(errorContent);
                    var root = doc.RootElement;

                    if (root.TryGetProperty("message", out var messageProp))
                    {
                        var message = messageProp.GetString();
                        await Message.Error(message);
                    }
                    else
                    {
                        await Message.Error("An unexpected error occurred.");
                    }
                }
                catch
                {
                    await Message.Error("An unexpected error occurred.");
                }
            }
        }

        private void Close()
        {
            IsVisible = false;
            StateHasChanged();
        }
        private void OnProductChanged(WarehouseExportDetailDto detail, ProductExportDto? product)
        {
            if (product != null)
            {
                detail.ProductId = product.Id;
                detail.ProductType = product.ProductType;
                detail.Unit = product.Unit;
                detail.ProductName = product.Name;
                detail.DateManufacture = product.ManufacturingDate;
                detail.ExpiryDate = product.ExpirationDate;
            }
        }
        private void AddDetailRow()
        {
            createdWarehouseExportDto.WarehouseExportDetailDtos?.Add(
                new WarehouseExportDetailDto
                {
                    Id = Guid.NewGuid()
                });
        }

        private void RemoveDetailRow(WarehouseExportDetailDto index)
        {
            if (createdWarehouseExportDto.WarehouseExportDetailDtos.Count > 0)
            {
                var remove = createdWarehouseExportDto.WarehouseExportDetailDtos.FirstOrDefault(x => x.Id == index.Id);
                createdWarehouseExportDto.WarehouseExportDetailDtos.Remove(remove);
            }

        }
        #endregion

    }
}
