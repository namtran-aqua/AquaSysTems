using AntDesign;
using AquaSolution.Client.Common;
using AquaSolution.Shared.Administration.UserManagements;
using AquaSolution.Shared.CommonDto;
using AquaSolution.Shared.Enum;
using AquaSolution.Shared.ManageMedicalRooms.MedicineSupplyRequest;
using AquaSolution.Shared.ManageMedicalRooms.Products;
using AquaSolution.Shared.UserManagements;
using Microsoft.AspNetCore.Components;
using OneOf.Types;
using System.Net.Http.Json;
using System.Text;

namespace AquaSolution.Client.Modals.ManageMedicalRooms.MedicineSupplyRequest
{
    public partial class MedicineSupplyRequestModal
    {
        #region Declaration
        [Inject] private HttpClient Http { get; set; }
        private MedicineSupplyRequestDto MedicineSupplyRequestDto { get; set; }
        [Parameter] public EventCallback OnSave { get; set; }
        private Form<CreatedMedicineSupplyRequestDto> formRef = new();
        private Table<CreatedMedicineSupplyRequestDetailDto> Table = new();
        private CreatedMedicineSupplyRequestDto createdMedicineSupplyRequestDto { get; set; } = new();
        private List<UserSelectedDto> userSelected { get; set; } = new();
        private List<ProductExportDto> _products = new List<ProductExportDto>();
        private UserDto CurrenUser { get; set; }
        private bool IsVisibleModal { get; set; }
        private bool IsView { get; set; }
        #endregion
        #region Innit
        public async Task ShowModal(MedicineSupplyRequestDto medicineSupplyRequestDto, bool isView, UserDto user)
        {
            IsView = isView;
            createdMedicineSupplyRequestDto = new();
            IsVisibleModal = true;
            MedicineSupplyRequestDto = medicineSupplyRequestDto;
            CurrenUser = user;
            await LoadUser();
            await LoadProduct();
            await Setparameter();

        }
        private async Task LoadUser()
        {
            userSelected = await Http.GetFromJsonAsync<List<UserSelectedDto>>("api/user/get-list-selected");
        }
        private async Task LoadProduct()
        {
            _products = await Http.GetFromJsonAsync<List<ProductExportDto>>("api/product/get-all-by-export");
        }
        private UserSelectedDto _userValue;
        private UserSelectedDto UserValue
        {
            get => _userValue;
            set
            {
                if (_userValue != value)
                {
                    _userValue = value;
                    createdMedicineSupplyRequestDto.FactoryId = value.FactoryId??Guid.Empty;
                    createdMedicineSupplyRequestDto.FactoryName = value.FactoryName;
                    createdMedicineSupplyRequestDto.DepartmentId = value.DepartmentId ?? Guid.Empty;
                    createdMedicineSupplyRequestDto.DepartmentName = value.DepartmentName;
                    createdMedicineSupplyRequestDto.UserRequestEmail = value.Email;
                    createdMedicineSupplyRequestDto.UserRequestId = value.Id;
                    StateHasChanged();
                }
            }
        }

        private async Task Setparameter()
        {
            if (userSelected != null && CurrenUser != null)
            {
                userSelected = userSelected
                    .Where(x => x.FactoryId == CurrenUser.FactoryId)
                    .ToList();

                userSelected = userSelected
                    .Where(x => x.DepartmentId == CurrenUser.DepartmentId)
                    .ToList();
                UserValue = userSelected.FirstOrDefault(x => x.Id == CurrenUser.Id);
            }
        }
        #endregion
        #region Action
        private void AddDetailRow()
        {

            createdMedicineSupplyRequestDto.CreatedMedicineSupplyRequestDetail.Add(
                   new CreatedMedicineSupplyRequestDetailDto
                   {
                       Id = Guid.NewGuid(),
                   });
        }
        private async Task RemoveDetailRow(CreatedMedicineSupplyRequestDetailDto index)
        {
            if (createdMedicineSupplyRequestDto.CreatedMedicineSupplyRequestDetail.Count > 0)
            {
                var remove = createdMedicineSupplyRequestDto.CreatedMedicineSupplyRequestDetail.FirstOrDefault(x => x.Id == index.Id);
                createdMedicineSupplyRequestDto.CreatedMedicineSupplyRequestDetail.Remove(remove);
            }
        }
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
            if (!createdMedicineSupplyRequestDto.CreatedMedicineSupplyRequestDetail.Any())
            {
                await Message.Error("Details cannot be left blank !");
                return;
            }
            var groupedDetails = createdMedicineSupplyRequestDto.CreatedMedicineSupplyRequestDetail
            .GroupBy(d => d.ProductId)
            .Select(g => new
            {
                Product = g.First().Product,
                RequestedQuantity = g.Sum(x => x.RequestedQuantity)
            });

            foreach (var item in groupedDetails)
            {

                if (item.Product.Id == Guid.Empty)
                {
                    await Message.Error("No products yet");
                    return;

                }
                if (item.RequestedQuantity <= 0)
                {
                    await Message.Error("Quantity must be greater than 0");
                    return;
                }
                if (item.RequestedQuantity > item.Product.Quantity)
                {
                    stringBuilder.AppendLine(item.Product.Name);
                    stringBuilder.Append(
                        $" Input Number of requests {item.RequestedQuantity} and actual quantity {item.Product.Quantity:0} Insufficient stock ;"
                    );
                    check++;
                }
            }
            if (check > 0)
            {

                await MessageBox.Error(modal, stringBuilder.ToString());
                return;
            }
            createdMedicineSupplyRequestDto.CreatedById = CurrenUser.Id;
            createdMedicineSupplyRequestDto.RequestType = MedicineSupplyRequestType.New;
            var response = await Http.PostAsJsonAsync(
                "api/MedicineSupplyRequest/create-medicine-supply-request", createdMedicineSupplyRequestDto);
            var result = await response.Content.ReadFromJsonAsync<ApiResponse>();
            if (response.IsSuccessStatusCode)
            {
                await Message.Success(result.message);
            }
            else
            {
                await Message.Error(result.message);
            }
            IsVisibleModal = false;
        }
        private async Task Close()
        {
            IsVisibleModal = false;
        }

        #endregion
    }
}
