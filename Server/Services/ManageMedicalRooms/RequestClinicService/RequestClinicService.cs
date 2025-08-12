using AquaSolution.Data.Connection;
using AquaSolution.Data.Data.Entities;
using AquaSolution.Data.Repositories;
using AquaSolution.Server.Services.Common.HandleInventories;
using AquaSolution.Server.SignalR;
using AquaSolution.Shared.CommonDto;
using AquaSolution.Shared.Enum;
using AquaSolution.Shared.ManageMedicalRooms.Prescriptions;
using AquaSolution.Shared.ManageMedicalRooms.RequestClinics;
using AquaSolution.Shared.ManageMedicalRooms.Treatments;
using Microsoft.AspNetCore.SignalR;

namespace AquaSolution.Server.Services.ManageMedicalRooms.RequestClinicservice
{
    public class RequestClinicservice : IRequestClinicservice
    {
        private readonly IRepository<RequestClinic> _requestClinicRepo;
        private readonly IRepository<User> _userRepo;
        private readonly IRepository<Treatment> _treatmentRepo;
        private readonly IRepository<Prescription> _prescriptionRepo;
        private readonly IRepository<PrescriptionDetail> _prescriptionDetailRepo;
        private readonly IRepository<Product> _product;
        private readonly IHandleInventory _handleInventory;
        private readonly AquaDbContext _context;
        private readonly IHubContext<SignalrHub> _hubContext;
        public RequestClinicservice
            (
            IRepository<RequestClinic> requestClinicRepo,
            IRepository<User> userRepo,
            IRepository<Treatment> treatmentRepo,
            AquaDbContext context, IHandleInventory handleInventory,
            IRepository<Prescription> prescriptionRepo,
            IRepository<PrescriptionDetail> prescriptionDetailRepo,
            IRepository<Product> product,
            IHubContext<SignalrHub> hubContext
            )
        {
            _requestClinicRepo = requestClinicRepo;
            _userRepo = userRepo;
            _context = context;
            _treatmentRepo = treatmentRepo;
            _prescriptionRepo = prescriptionRepo;
            _prescriptionDetailRepo = prescriptionDetailRepo;
            _product = product;
            _handleInventory = handleInventory;
            _hubContext = hubContext;
        }

        public async Task<bool> CreatedRequest(HandleMyRequestClinicDto handleMyRequestClinic)
        {
            var requestClinic = new RequestClinic
            {
                Id = handleMyRequestClinic.Id,
                WorkDayUserRequestId = handleMyRequestClinic.WorkDayUserRequestId,
                UserRequestId = handleMyRequestClinic.UserRequestId,
                UserRequestName = handleMyRequestClinic.UserRequestName,
                RequestTitle = handleMyRequestClinic.RequestTitle,
                PurposeType = handleMyRequestClinic.PurposeType,
                ManagerId = handleMyRequestClinic.ManagerId,
                Status = handleMyRequestClinic.Status,
                CreatedDate = handleMyRequestClinic.CreatedDate,
                Note = handleMyRequestClinic.Note,
                CreatedBy = handleMyRequestClinic.CreatedBy,
            };
            await _requestClinicRepo.InsertAsync(requestClinic);
            var result = await _requestClinicRepo.SaveChangesAsync();
            await _hubContext.Clients.All.SendAsync("ChangeStatusRequestClinic");
            return result > 0;
        }
        public async Task<List<MyRequestClinicDto>> GetRequestByUser()
        {
            try
            {
                var requestClinicQueryAble = await _requestClinicRepo.GetQueryableAsync();
                var userQuery = await _userRepo.GetQueryableAsync();

                var dataRequest = from request in requestClinicQueryAble
                                  join createdby in userQuery on request.CreatedBy equals createdby.Id

                                  join manage_ in userQuery on request.ManagerId equals manage_.Id into manage
                                  from manager in manage.DefaultIfEmpty()

                                  join approvalBy_ in userQuery on request.ApprovalBy equals approvalBy_.Id into apprval
                                  from approvalBy in apprval.DefaultIfEmpty()

                                  join rejectBy_ in userQuery on request.RejectBy equals rejectBy_.Id into reject
                                  from rejectBy in reject.DefaultIfEmpty()

                                  join pharmacyManager_ in userQuery on request.PharmacyManagerId equals pharmacyManager_.Id into pharmacy
                                  from pharmacyManager in pharmacy.DefaultIfEmpty()
                                  select new MyRequestClinicDto
                                  {
                                      Id = request.Id,
                                      UserRequestId = request.UserRequestId,
                                      WorkDayUserRequestId = request.WorkDayUserRequestId,
                                      UserRequestName = request.UserRequestName,
                                      RequestTitle = request.RequestTitle,
                                      PurposeType = request.PurposeType,
                                      ManagerId = request.ManagerId,
                                      ManagerName = manager.FullName,
                                      Status = request.Status,
                                      ApprovalDate = request.ApprovalDate,
                                      ApprovalById = request.ApprovalBy,
                                      ApprovalByName = approvalBy != null ? approvalBy.FullName : null,
                                      RejectDate = request.RejectDate,
                                      RejectById = request.RejectBy,
                                      RejectByName = rejectBy != null ? rejectBy.FullName : null,
                                      SuccesDate = request.SuccesDate,
                                      PharmacyManagerName = pharmacyManager != null ? pharmacyManager.FullName : null,
                                      CreatedDate = request.CreatedDate,
                                      Note = request.Note,
                                      HistoryReuqest = request.HistoryReuqest,
                                      CreatedBy = request.CreatedBy,
                                      CreatedName = createdby.FullName,
                                      CreatedWorkDay = createdby.WorkDayId,
                                      EmailRequestter = createdby.Email
                                  };

                var listRequest = dataRequest.OrderBy(x => x.CreatedDate).ToList();

                return listRequest.Count > 0 ? listRequest : new List<MyRequestClinicDto>();
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        public async Task<bool> ApprovalAsync(Guid requestClinicId, Guid approvalBy)
        {
            var request = await _requestClinicRepo.FirstOrDefaultAsync(x => x.Id == requestClinicId);
            if (request == null)
                return false;
            request.Status = StatusClinicType.Approval;
            request.ApprovalDate = DateTime.Now;
            request.ApprovalBy = approvalBy;
            var update = await _requestClinicRepo.UpdateAsync(request);
            await _hubContext.Clients.All.SendAsync("ChangeStatusRequestClinic");
            return update;
        }
        public async Task<bool> RejectedAsync(Guid requestClinicId, Guid rejectBy)
        {
            var request = await _requestClinicRepo.FirstOrDefaultAsync(x => x.Id == requestClinicId);
            if (request == null)
                return false;
            request.Status = StatusClinicType.Rejected;
            request.ApprovalDate = DateTime.Now;
            request.ApprovalBy = rejectBy;
            var update = await _requestClinicRepo.UpdateAsync(request);
            await _hubContext.Clients.All.SendAsync("ChangeStatusRequestClinic");
            return update;
        }
        public async Task<bool> DoneAsync(Guid requestClinicId, Guid pharmacyManagerId)
        {
            var request = await _requestClinicRepo.FirstOrDefaultAsync(x => x.Id == requestClinicId);
            if (request == null)
                return false;
            request.Status = StatusClinicType.Done;
            request.SuccesDate = DateTime.Now;
            request.PharmacyManagerId = pharmacyManagerId;
            var update = await _requestClinicRepo.UpdateAsync(request);
            await _hubContext.Clients.All.SendAsync("ChangeStatusRequestClinic");
            return update;
        }
        public async Task<bool> CreatedTreatment(CreatedTreatmentDto createdTreatment)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            var treatment = new Treatment
            {
                RequestId = createdTreatment.RequestId,
                Diagnose = createdTreatment.Diagnose,
                Treatments = createdTreatment.Treatments,
                Note = createdTreatment.Note,
                CheckInTime = DateTime.Now,
                PharmacyManagerId = createdTreatment.PharmacyManagerId,
                TreatmentType = createdTreatment.TreatmentType
            };
            await _treatmentRepo.InsertAsync(treatment);
            if (createdTreatment.Prescription.Id != Guid.Empty)
            {
                var prescription = new Prescription
                {
                    Id = createdTreatment.Prescription.Id,
                    RequestId = createdTreatment.RequestId,
                    Code = createdTreatment.Prescription.Code,
                    Name = createdTreatment.Prescription.Name,
                    CreatedDate = DateTime.Now,
                    PharmacyManagerId = createdTreatment.PharmacyManagerId
                };
                await _prescriptionRepo.InsertAsync(prescription);
                foreach (var item in createdTreatment.Prescription.CreatedPrescriptionDetail)
                {
                    var prescriptionDetail = new PrescriptionDetail
                    {
                        Id = item.Id,
                        PrescriptionId = prescription.Id,
                        Quantity = item.Quantity,
                        ProductId = item.productDto.Id,
                        ExpirationDate = item.productDto.ExpirationDate,
                        ManufacturingDate = item.productDto.ManufacturingDate
                    };
                    var handleInventorydto = new HandleInventoryDto
                    {
                        ProductId = prescriptionDetail.ProductId,
                        ExpirationDate = prescriptionDetail.ExpirationDate,
                        Quantity = prescriptionDetail.Quantity,
                        ManufacturingDate = prescriptionDetail.ManufacturingDate,
                    };
                    await _handleInventory.MinusInventory(handleInventorydto);
                    await _prescriptionDetailRepo.InsertAsync(prescriptionDetail);
                }
            }
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            return true;
        }
        public async Task<MedicalHistoryDto?> GetHistory(Guid requestId)
        {
            try
            {// Lấy dữ liệu từ DB
                var requestClinic = await _requestClinicRepo.GetQueryableAsync();
                var user = await _userRepo.GetQueryableAsync();
                var treatment = await _treatmentRepo.GetQueryableAsync();
                var prescription = await _prescriptionRepo.GetQueryableAsync();
                var prescriptionDetail = await _prescriptionDetailRepo.GetQueryableAsync();
                var product = await _product.GetQueryableAsync();

                // Query chính - lấy 1 bản ghi duy nhất
                var data = (from requestClinicQuery in requestClinic
                            join requester in user on requestClinicQuery.UserRequestId equals requester.Id

                            join manager in user on requestClinicQuery.ManagerId equals manager.Id
                            into m
                            from manager in m.DefaultIfEmpty()

                            join createdby in user on requestClinicQuery.CreatedBy equals createdby.Id

                            join prescriptionQuery in prescription on requestClinicQuery.Id equals prescriptionQuery.RequestId
                            into p1
                            from prescriptionQuery in p1.DefaultIfEmpty()

                            join approval in user on requestClinicQuery.ApprovalBy equals approval.Id into a
                            from approval in a.DefaultIfEmpty()

                            join reject in user on requestClinicQuery.RejectBy equals reject.Id into r
                            from reject in r.DefaultIfEmpty()

                            join parmacy in user on requestClinicQuery.PharmacyManagerId equals parmacy.Id into p
                            from parmacy in p.DefaultIfEmpty()

                            join treatmentQuery in treatment on requestClinicQuery.Id equals treatmentQuery.RequestId
                            into t from treatmentQuery in t.DefaultIfEmpty()

                            where requestClinicQuery.Id == requestId
                            select new MedicalHistoryDto
                            {
                                Id = requestClinicQuery.Id,
                                WorkDayUserRequestId = requestClinicQuery.WorkDayUserRequestId,
                                UserRequestId = requestClinicQuery.UserRequestId,
                                UserRequestName = requestClinicQuery.UserRequestName,
                                RequestTitle = requestClinicQuery.RequestTitle,
                                PurposeType = requestClinicQuery.PurposeType,
                                ManagerId = requestClinicQuery.ManagerId,
                                ManagerName = manager.FullName,
                                EmailRequestter = requester.Email,
                                WorkDayManager = manager.WorkDayId,
                                Status = requestClinicQuery.Status,
                                ApprovalDate = requestClinicQuery.ApprovalDate,
                                ApprovalByName = approval.FullName,
                                ApprovalById = approval.Id,
                                RejectDate = requestClinicQuery.RejectDate,
                                RejectById = requestClinicQuery.RejectBy,
                                RejectByName = reject.FullName,
                                SuccesDate = requestClinicQuery.SuccesDate,
                                PharmacyManagerId = requestClinicQuery.PharmacyManagerId,
                                CreatedDate = requestClinicQuery.CreatedDate,
                                PharmacyManagerName = parmacy.FullName,
                                Note = requestClinicQuery.Note,
                                HistoryReuqest = requestClinicQuery.HistoryReuqest,
                                CreatedBy = requestClinicQuery.CreatedBy,
                                CreatedName = createdby.FullName,
                                CreatedWorkDay = createdby.WorkDayId,
                                PrescriptionId = prescriptionQuery.Id,
                                PrescriptionCode = prescriptionQuery.Code,
                                PrescriptionName = prescriptionQuery.Name,
                                PrescriptionCreatedTime = prescriptionQuery.CreatedDate,
                                Diagnose = treatmentQuery.Diagnose,
                                Treatments = treatmentQuery.Treatments,
                                TreatmentNote = treatmentQuery.Note,
                                CheckInTime = treatmentQuery.CheckInTime,
                                TreatmentType = treatmentQuery.TreatmentType,
                                TreatmentId = treatmentQuery.RequestId
                            }).FirstOrDefault();

                if (data == null)
                    return null;
                var detailItems = (from detail in prescriptionDetail
                                   where detail.PrescriptionId == data.PrescriptionId
                                   join prod in product on detail.ProductId equals prod.Id
                                   orderby detail.Id
                                   select new PrescriptionDetailDto
                                   {
                                       Id = detail.Id,
                                       PrescriptionId = detail.PrescriptionId,
                                       Quantity = detail.Quantity,
                                       ProductId = detail.ProductId,
                                       ExpirationDate = detail.ExpirationDate,
                                       ManufacturingDate = detail.ManufacturingDate,
                                       ProductName = prod.Name,
                                       ProductType = prod.ProductType,
                                       Unit = prod.Unit,
                                   }).ToList();

                data.PrescriptionDetail = detailItems;

                return data;

            }
            catch( Exception ex)
            {
                throw ex;
            }
            
        }

        public async Task<bool> DeleteMyRequestClinic(Guid requestId)
        {
            var requestClinic = await _requestClinicRepo.FirstOrDefaultAsync(x => x.Id == requestId);
            if (requestClinic != null)
            {
               return await _requestClinicRepo.DeleteAsync(requestClinic);
            }
            return false;
        }
    }
}
