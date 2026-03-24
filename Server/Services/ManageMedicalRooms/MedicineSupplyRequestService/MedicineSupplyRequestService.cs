using AquaSolution.Data.Data.Entities.Admin;
using AquaSolution.Data.Data.Entities.Clinic;
using AquaSolution.Data.Repositories;
using AquaSolution.Shared.ManageMedicalRooms.MedicineSupplyRequest;
using AquaSolution.Shared.ManageMedicalRooms.Products;

namespace AquaSolution.Server.Services.ManageMedicalRooms.MedicineSupplyRequestService
{
    public class MedicineSupplyRequestService : IMedicineSupplyRequestService
    {
        private readonly IRepository<MedicineSupplyRequest> _medicineSupplyRequestRepository;
        private readonly IRepository<MedicineSupplyRequestDetail> _medicineSupplyRequestDetailRepository;
        private readonly IRepository<User> _userRepository;
        private readonly IRepository<Department> _departmentRepository;
        private readonly IRepository<Factory> _factoryRepository;
        private readonly IRepository<Product> _productRepository;

        public MedicineSupplyRequestService(IRepository<MedicineSupplyRequest> medicineSupplyRequestRepository,
            IRepository<MedicineSupplyRequestDetail> medicineSupplyRequestDetailRepository,
            IRepository<User> userRepository,
            IRepository<Department> departmentRepository,
            IRepository<Factory> factoryRepository,
            IRepository<Product> productRepository
            )
        {
            _medicineSupplyRequestRepository = medicineSupplyRequestRepository;
            _medicineSupplyRequestDetailRepository = medicineSupplyRequestDetailRepository;
            _userRepository = userRepository;
            _departmentRepository = departmentRepository;
            _factoryRepository = factoryRepository;
            _productRepository = productRepository;
        }
        public async Task<bool> CreatedMedicineSupplyRequest(CreatedMedicineSupplyRequestDto createdMedicineSupplyRequestDto)
        {
            var checkRowSave = 0;
            var medicineSupplyRequest = new MedicineSupplyRequest
            {
                Id = Guid.NewGuid(),
                Title = createdMedicineSupplyRequestDto.Title,
                Description = createdMedicineSupplyRequestDto.Description,
                DepartmentId = createdMedicineSupplyRequestDto.DepartmentId,
                FactoryId = createdMedicineSupplyRequestDto.FactoryId,
                UserRequestId = createdMedicineSupplyRequestDto.UserRequestId,
                CreatedDate = DateTime.Now,
                CreatedById = createdMedicineSupplyRequestDto.CreatedById,
                Note = createdMedicineSupplyRequestDto.Note,
                RequestType =createdMedicineSupplyRequestDto.RequestType,
            };
            await _medicineSupplyRequestRepository.InsertAsync( medicineSupplyRequest );
            checkRowSave += await _medicineSupplyRequestRepository.SaveChangesAsync();
            foreach (var item in createdMedicineSupplyRequestDto.CreatedMedicineSupplyRequestDetail)
            {
                var medicineSupplyRequestDetail = new MedicineSupplyRequestDetail
                {
                    Id = item.Id,
                    MedicineSupplyRequestId = medicineSupplyRequest.Id,
                    ProductId = item.ProductId,
                    DateManufactured = item.DateManufactured,
                    ExpiryDate = item.ExpiryDate,
                    RequestedQuantity = item.RequestedQuantity,
                };
                await _medicineSupplyRequestDetailRepository.InsertAsync ( medicineSupplyRequestDetail );
            }
            checkRowSave = await _medicineSupplyRequestDetailRepository.SaveChangesAsync();
            return checkRowSave > 0;
        }

        public async Task<List<MedicineSupplyRequestDto>> GetListMedicineSuplyRequest()
        {
            var user = await _userRepository.GetQueryableAsync();
            var department = await _departmentRepository.GetQueryableAsync();
            var factory = await _factoryRepository.GetQueryableAsync();

            var query = from medicineSupplyRequest in await _medicineSupplyRequestRepository.GetQueryableAsync()
                        join departmentQuery in  department on medicineSupplyRequest.DepartmentId equals departmentQuery.Id
                        join factoryQuery in factory on medicineSupplyRequest.FactoryId equals factoryQuery.Id
                       
                        join userRequester in user on medicineSupplyRequest.UserRequestId equals userRequester.Id

                        join createdBy in user on medicineSupplyRequest.CreatedById equals createdBy.Id

                        join approval in user on medicineSupplyRequest.ApprovalId equals approval.Id
                        into a from approval in a.DefaultIfEmpty()

                        join reject in user on medicineSupplyRequest.RejectId equals reject.Id
                        into r from reject in r.DefaultIfEmpty()

                        join parmacyManagement in user on medicineSupplyRequest.PharmacyManagerId equals parmacyManagement.Id
                        into p from parmacyManagement in p.DefaultIfEmpty()
                        orderby medicineSupplyRequest.CreatedDate descending
                        select new MedicineSupplyRequestDto
                        {
                            Id = medicineSupplyRequest.Id,
                            Title = medicineSupplyRequest.Title,
                            Description = medicineSupplyRequest.Description,
                            DepartmentId = medicineSupplyRequest.DepartmentId,
                            DepartmentName = departmentQuery.Name,
                            FactoryId = medicineSupplyRequest.FactoryId,
                            FactoryName = factoryQuery.Name,
                            UserRequestId = userRequester.Id,
                            UserRequestName = userRequester.FullName,
                            CreatedDate = medicineSupplyRequest.CreatedDate,
                            CreatedById = medicineSupplyRequest.CreatedById,
                            CreatedByName = createdBy.FullName,
                            ApprovalId = approval.Id,
                            ApprovalName = approval.FullName,
                            ApprovedDate = medicineSupplyRequest.ApprovedDate,
                            RejectId = reject.Id,
                            RejectName = reject.FullName,
                            RejectDate = medicineSupplyRequest.RejectDate,
                            PharmacyManagerId = parmacyManagement.Id,
                            PharmacyManagerName = parmacyManagement.FullName,
                            MedicineDispensingDate = medicineSupplyRequest.MedicineDispensingDate,
                            Note = medicineSupplyRequest.Note,
                            RequestType = medicineSupplyRequest.RequestType,
                        };
            var data  =  query.ToList();
            if(data != null)
            {
                return data;
            }
            return new List<MedicineSupplyRequestDto> ();
        }

        public async Task<List<MedicineSupplyRequestDetailDto>> GetListMedicineSuplyRequestDetail(Guid medicineSupplyRequestId)
        {
            try
            {
                var query = from medicineSupplyRequestDetail in await _medicineSupplyRequestDetailRepository.GetQueryableAsync()
                            join product in await _productRepository.GetQueryableAsync()
                            on medicineSupplyRequestDetail.ProductId equals product.Id
                            where medicineSupplyRequestDetail.MedicineSupplyRequestId == medicineSupplyRequestId
                            select new MedicineSupplyRequestDetailDto
                            {
                                Id = medicineSupplyRequestDetail.Id,
                                MedicineSupplyRequestId = medicineSupplyRequestId,
                                ProductId = medicineSupplyRequestDetail.ProductId,
                                DateManufactured = medicineSupplyRequestDetail.DateManufactured,
                                ExpiryDate = medicineSupplyRequestDetail.ExpiryDate,
                                RequestedQuantity = medicineSupplyRequestDetail.RequestedQuantity,
                                QuantityIssued = medicineSupplyRequestDetail.QuantityIssued,
                                Note = medicineSupplyRequestDetail.Note,
                                Product = new ProductExportDto
                                {
                                    Id = product.Id,
                                    Code = product.Code,
                                    Name = product.Name,
                                    Unit = product.Unit

                                }
                            };
                var data = query.ToList();
                if (data != null)
                {
                    return data;
                }
                return new List<MedicineSupplyRequestDetailDto>();

            }
            catch(Exception ex)
            {
                throw ex;
            }
           
        }
    }
}
