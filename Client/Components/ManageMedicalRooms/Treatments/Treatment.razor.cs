using AntDesign;
using AquaSolution.Client.Common;
using AquaSolution.Shared.CommonDto;
using AquaSolution.Shared.Enum;
using AquaSolution.Shared.ManageMedicalRooms.RequestClinics;
using AquaSolution.Shared.ManageMedicalRooms.Prescriptions;
using AquaSolution.Shared.ManageMedicalRooms.Products;
using AquaSolution.Shared.ManageMedicalRooms.Treatments;
using AquaSolution.Shared.ManageMedicalRooms.WarehouseExports;
using AquaSolution.Shared.ManageMedicalRooms.WarehouseImports;
using AquaSolution.Shared.UserManagements;
using Microsoft.AspNetCore.Components;
using System.Net.Http.Json;
using System.Reflection.Metadata;
using AquaSolution.Shared.ManageMedicalRooms.RequestClinics;

namespace AquaSolution.Client.Components.ManageMedicalRooms.Treatments
{
    public partial class Treatment
    {
        #region Declaration
        [Inject] private HttpClient Http { get; set; } = new();
        [Parameter] public EventCallback OnSave { get; set; }
        private Form<CreatedTreatmentDto> formRef = new();
        private Table<CreatedPrescriptionDetailDto> Table = new();
        private List<ProductExportDto> _products = new List<ProductExportDto>();
        private bool IsVisible { get; set; }
        private bool IsView { get; set; }
        private List<TreatmentType> ListTreatmentType = new List<TreatmentType>();
        private CreatedTreatmentDto CreatedTreatment { get; set; } = new();
        private bool IsPrescription { get; set; }
        #endregion
        #region Innit
        public async Task OpenModal(CreatedTreatmentDto createdTreatmentDto, bool isView)
        {
            CreatedTreatment = new CreatedTreatmentDto();
            IsView = isView;
            IsVisible = true;
            CreatedTreatment = createdTreatmentDto;
            await LoadTreatmentType();
            await LoadProduct();
            await InvokeAsync(StateHasChanged);
        }
        private async Task LoadTreatmentType()
        {
            ListTreatmentType = Enum.GetValues(typeof(TreatmentType))
                .Cast<TreatmentType>()
                .ToList();
            TreatmentTypeValue = ListTreatmentType
                    .First();
        }
        private TreatmentType _treatmentType;
        private TreatmentType TreatmentTypeValue
        {
            get => _treatmentType;
            set
            {
                if (_treatmentType != value)
                {
                    _treatmentType = value;
                    if(value == TreatmentType.GiveMedicine)
                    {
                        IsPrescription =true;
                    }
                    else
                    {
                        IsPrescription =false;
                    }
                    CreatedTreatment.TreatmentType = value;
                    StateHasChanged();
                }
            }
        }
        private async Task LoadProduct()
        {
            _products = await Http.GetFromJsonAsync<List<ProductExportDto>>("api/product/get-all-by-export");
        }
        #endregion
        #region Action
        private async Task SaveAsync()
        {
            var validate = formRef.Validate();
            if (!validate)
            {
                return;
            }
            var dto = new UpdateRequestClinicStatusDto
            {
                RequestClinicId = CreatedTreatment.RequestId,
                UserId = CreatedTreatment.PharmacyManagerId,
            };
                CreatedTreatment.Prescription.Code = $"Givemedicineto {CreatedTreatment.PatientName} {DateTime.Now:yyyyMMdd}".ToUpper();
                CreatedTreatment.Prescription.Name = $"Give medicine to {CreatedTreatment.PatientName} {DateTime.Now:yyyyMMdd}".ToUpper();
            if(CreatedTreatment.TreatmentType == TreatmentType.GiveMedicine)
            {
                CreatedTreatment.Prescription.Id = Guid.NewGuid();
            }
            var responsetreatment = await Http.PostAsJsonAsync("api/MyRequestClinic/created-treatment", CreatedTreatment);
            if (responsetreatment.IsSuccessStatusCode)
            {
                var response = await Http.PostAsJsonAsync("api/MyRequestClinic/done", dto);
                await Message.Success("Success!");
                IsVisible = false;
                await OnSave.InvokeAsync();
            }
            else
            {
                await Message.Error("Error!");
            }
            await InvokeAsync(StateHasChanged);
        }
        private void Close()
        {
            IsVisible = false;
            StateHasChanged();
        }
        private void AddDetailRow()
        {
            CreatedTreatment.Prescription?.CreatedPrescriptionDetail.Add(
                new CreatedPrescriptionDetailDto
                {
                    Id = Guid.NewGuid()
                });
        }
        private async Task RemoveDetailRow(CreatedPrescriptionDetailDto index)
        {
            if (CreatedTreatment.Prescription?.CreatedPrescriptionDetail.Count > 0)
            {
                var remove = CreatedTreatment.Prescription?.CreatedPrescriptionDetail.FirstOrDefault(x => x.Id == index.Id);
                CreatedTreatment.Prescription?.CreatedPrescriptionDetail.Remove(remove);
            }
        }
        #endregion
    }
}
