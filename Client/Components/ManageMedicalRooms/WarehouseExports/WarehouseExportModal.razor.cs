using AntDesign;
using AquaSolution.Client.Common;
using AquaSolution.Shared.Enum;
using AquaSolution.Shared.ManageMedicalRooms.Products;
using AquaSolution.Shared.ManageMedicalRooms.WarehouseExports;
using AquaSolution.Shared.UserManagements;
using Microsoft.AspNetCore.Components;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace AquaSolution.Client.Components.ManageMedicalRooms.WarehouseExports
{
    public partial class WarehouseExportModal
    {
        #region Declaration
        private bool IsVisible { get; set; }
        private Form<CreatedWarehouseExportDto> formRef;
        private Table<WarehouseExportDetailDto> tableRef;
        [Inject] private HttpClient Http { get; set; }
        private List<ProductExportDto> _products = new List<ProductExportDto>();
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
            await SetDataView();
            GetWarehouseExportType();
            var CurrenUserClass = new CurrenUser(Http, AuthStateProvider);
            CurrenUser = await CurrenUserClass.LoadCurrenUser();
            await InvokeAsync(StateHasChanged);
        }
        private async Task LoadProduct()
        {
            _products = await Http.GetFromJsonAsync<List<ProductExportDto>>("api/product/get-all-by-export");
        }
        private async Task SetDataView()
        {
            if (IsView)
            {
                createdWarehouseExportDto = new();
                createdWarehouseExportDto.WarehouseExportDto.Code = LoadWarehouseExportDto.Code;
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
                        var matchedProduct = _products.FirstOrDefault(p => p.Id == detail.ProductId);
                        createdWarehouseExportDto.WarehouseExportDetailDtos.Add(new WarehouseExportDetailDto
                        {
                            Id = detail.Id,
                            Quantity = detail.Quantity,
                            productDto = matchedProduct
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
                if (itemDetail.productDto.ExpirationDate == null)
                {
                    await Message.Error("ExpiryDate cannot be left blank !");
                    return;
                }

                if (itemDetail.Quantity <= 0)
                {
                    await Message.Error("Quantity must be greater than 0");
                    return;
                }

                if (itemDetail.productDto.Id == Guid.Empty)
                {
                    await Message.Error("Product cannot be left blank");
                    return;
                }
                var checkQuantity = _products.FirstOrDefault(x=>x.Id == itemDetail.productDto.Id);
                if (checkQuantity != null) 
                {
                    if(checkQuantity.Quantity < itemDetail.Quantity) 
                    {
                        stringBuilder.AppendLine(itemDetail.productDto.Name.ToString());
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
        private void AddDetailRow()
        {
            createdWarehouseExportDto.WarehouseExportDetailDtos?.Add(
                new WarehouseExportDetailDto
                {
                    Id = Guid.NewGuid()
                });
        }

        private async Task RemoveDetailRow(WarehouseExportDetailDto index)
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
