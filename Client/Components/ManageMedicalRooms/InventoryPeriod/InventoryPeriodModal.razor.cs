using AntDesign;
using AquaSolution.Client.Common;
using AquaSolution.Shared.ManageMedicalRooms.Inventories;
using AquaSolution.Shared.ManageMedicalRooms.InventoryPeriod;
using Microsoft.AspNetCore.Components;
using System.Net.Http.Json;

namespace AquaSolution.Client.Components.ManageMedicalRooms.InventoryPeriod
{
    public partial class InventoryPeriodModal
    {
        #region Declaration
        [Inject] private HttpClient Http { get; set; } = new();
        private List<InventoryDto> _listInventory = new List<InventoryDto>();
        private bool IsModalVisible = false;
        private Form<CreatedInventoryPeriodDto> formRef;
        private Table<InventoryPeriodDetailDto> tableRef;
        private bool IsStart { get; set; }
        private bool IsView { get; set; }
        [Parameter]public  EventCallback<Task>OnSaved { get; set; }

        private CreatedInventoryPeriodDto CreatedInventoryPeriodDto = new();
        private CreatedInventoryPeriodDto ViewInventoryPeriodDto = new();
        private Guid CurrenUser { get; set; }
        #endregion
        #region Innit
        public async Task ShowModalAsync(CreatedInventoryPeriodDto createdInventoryPeriodDto, Guid currenUser, bool isView)
        {
            ViewInventoryPeriodDto = createdInventoryPeriodDto;
            CurrenUser = currenUser;
            IsModalVisible = true;
            IsView = isView;
            if (isView)
            {
                ViewInventoryPeriodDto.InventoryPeriodDetailDtos = await Http.GetFromJsonAsync<List<InventoryPeriodDetailDto>>
                ($"api/InventoryPeriod/get-inventory-period-detail/{ViewInventoryPeriodDto.InventoryPeriodDto.Id}");
            }
            await InvokeAsync(StateHasChanged);

        }
        #endregion
        #region ACtion
        private void OnActualQuantityChanged(decimal? newValue, InventoryPeriodDetailDto row)
        {
            row.ActualQuantity = newValue;
            row.RemainingValidity = row.ActualQuantity - row.SystemQuantity;

        }
        private async Task SaveAsync()
        {
            var validate = formRef.Validate();
            if (!validate)
            {
                return;
            }
            var response = await Http.PutAsJsonAsync($"api/InventoryPeriod/update-actual-inventory-period"
                , ViewInventoryPeriodDto.InventoryPeriodDetailDtos);
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
        private async Task StartAsync()
        {
            CreatedInventoryPeriodDto = new();
            var validate = formRef.Validate();
            if (!validate)
            {
                return;
            }
            _listInventory = await Http.GetFromJsonAsync<List<InventoryDto>>("api/Inventory/get-all");
            string? code = await Http.GetFromJsonAsync<string>("api/common/get-code-inventoryPeriod");
            if (string.IsNullOrEmpty(code))
            {
                await Message.Error("Unable to generate code, please try again!");
            }
            else
            {
                ViewInventoryPeriodDto.InventoryPeriodDto.Code = code;
            }
            ViewInventoryPeriodDto.InventoryPeriodDto.Code = code;
            CreatedInventoryPeriodDto.InventoryPeriodDto.Code = ViewInventoryPeriodDto.InventoryPeriodDto.Code;
            CreatedInventoryPeriodDto.InventoryPeriodDto.Name = ViewInventoryPeriodDto.InventoryPeriodDto.Name;
            CreatedInventoryPeriodDto.InventoryPeriodDto.Year = ViewInventoryPeriodDto.InventoryPeriodDto.Year;
            CreatedInventoryPeriodDto.InventoryPeriodDto.Month = ViewInventoryPeriodDto.InventoryPeriodDto.Month;
            CreatedInventoryPeriodDto.InventoryPeriodDto.Note = ViewInventoryPeriodDto.InventoryPeriodDto.Note;
            CreatedInventoryPeriodDto.InventoryPeriodDto.CreatedById = CurrenUser;
            foreach (var item in _listInventory)
            {
                CreatedInventoryPeriodDto.InventoryPeriodDetailDtos.Add(
                    new InventoryPeriodDetailDto
                    {
                        InventoryId = item.Id,
                        ProductId = item.ProductId,
                        SystemQuantity = item.Quantity,
                        DateManufacture = item.ManufacturingDate,
                        ExpiryDate = item.ExpirationDate,
                        ProductType = item.ProductType,
                    });
            }
            var respone = await Http.PostAsJsonAsync($"api/InventoryPeriod/create-inventoryPeriod", CreatedInventoryPeriodDto);
            if (respone.IsSuccessStatusCode)
            {
                var inventoryPeriodId = await respone.Content.ReadFromJsonAsync<Guid>();
                ViewInventoryPeriodDto.InventoryPeriodDto.Id = inventoryPeriodId;
                ViewInventoryPeriodDto.InventoryPeriodDetailDtos = await Http.GetFromJsonAsync<List<InventoryPeriodDetailDto>>
                    ($"api/InventoryPeriod/get-inventory-period-detail/{ViewInventoryPeriodDto.InventoryPeriodDto.Id}");
            }
            else
            {
                var errorMessage = await respone.Content.ReadAsStringAsync();
                await Message.Error(errorMessage);
            }
            IsStart = true;
            await InvokeAsync(StateHasChanged);
        }
        private void Close() => IsModalVisible = false;
        #endregion

    }
}
