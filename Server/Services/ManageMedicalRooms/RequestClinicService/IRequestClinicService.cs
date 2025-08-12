using AquaSolution.Shared.ManageMedicalRooms.RequestClinics;
using AquaSolution.Shared.ManageMedicalRooms.Treatments;

namespace AquaSolution.Server.Services.ManageMedicalRooms.RequestClinicservice
{
    public interface IRequestClinicservice
    {
        Task<List<MyRequestClinicDto>> GetRequestByUser();
        Task<bool> CreatedRequest(HandleMyRequestClinicDto handleMyRequestClinic);
        Task<bool> ApprovalAsync(Guid requestClinicId,Guid approvalBy);
        Task<bool> RejectedAsync(Guid requestClinicId,Guid rejectBy);
        Task<bool> DoneAsync(Guid requestClinicId,Guid pharmacyManagerId);
        Task<bool> CreatedTreatment(CreatedTreatmentDto createdTreatment);
        Task<MedicalHistoryDto> GetHistory(Guid requestId);
        Task<bool> DeleteMyRequestClinic(Guid requestId);
    }
}
