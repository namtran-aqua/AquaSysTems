using AquaSolution.Shared.ManageMedicalRooms.MedicineSupplyRequest;

namespace AquaSolution.Server.Services.ManageMedicalRooms.MedicineSupplyRequestService
{
    public interface IMedicineSupplyRequestService
    {
        Task<bool> CreatedMedicineSupplyRequest(CreatedMedicineSupplyRequestDto createdMedicineSupplyRequestDto);
        Task<List<MedicineSupplyRequestDto>> GetListMedicineSuplyRequest();
        Task<List<MedicineSupplyRequestDetailDto>> GetListMedicineSuplyRequestDetail( Guid medicineSupplyRequestId);

    }

}
