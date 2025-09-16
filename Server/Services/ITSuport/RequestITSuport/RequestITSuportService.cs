using AquaSolution.Data.Data.Entities;
using AquaSolution.Data.Repositories;
using AquaSolution.Shared.Enum;
using AquaSolution.Shared.ITSuport.Attachments;
using AquaSolution.Shared.ITSuport.RequestSuport;
using AquaSolution.Shared.ITSuport.RequestSuportCategory;

namespace AquaSolution.Server.Services.ITSuport.RequestSuportCategories
{
    public class RequestITSuportService : IRequestITSuportService
    {
        private readonly IRepository<User> _userRepo;
        private readonly IRepository<RequestSuportCategory> _requestSuportCategoryRepo;
        private readonly IRepository<RequestSuport> _requestSuportRepo;
        private readonly IRepository<Department> _departmentRepo;
        private readonly IRepository<Factory> _factoryRepo;
        private readonly IRepository<Attachment> _attachmentRepo;

        public RequestITSuportService(
               IRepository<User> userRepo,
                  IRepository<RequestSuportCategory> requestSuportCategoryRepo,
                   IRepository<RequestSuport> requestSuportRepo,
                   IRepository<Department> departmentRepo,

                     IRepository<Factory> factoryRepo,
                     IRepository<Attachment> attachmentRepo
                  )
        {
            _userRepo = userRepo;
            _requestSuportCategoryRepo = requestSuportCategoryRepo;
            _requestSuportRepo = requestSuportRepo;
            _departmentRepo = departmentRepo;
            _factoryRepo = factoryRepo;
            _attachmentRepo = attachmentRepo;
        }

        public async Task<bool> CreatedAsync(HandleRequestSuportDto handleRequestSuportDto)
        {
            var requestSuport = new RequestSuport
            {
                Id = Guid.NewGuid(),
                RequestSuportCategoryId = handleRequestSuportDto.RequestSuportCategoryId,
                Status = handleRequestSuportDto.Status,
                RequestBy = handleRequestSuportDto.RequestBy,
                CreatedDate = handleRequestSuportDto.CreatedDate,
                RequestDescription = handleRequestSuportDto.RequestDescription,
                RequestSolution = handleRequestSuportDto.RequestSolution,
                TechnicianId = handleRequestSuportDto.TechnicianId,
                RequestTitle = handleRequestSuportDto.RequestTitle,
                CreatedById = handleRequestSuportDto.CreatedById,
            };
            if (handleRequestSuportDto.Attachments != null)
            {
                foreach (var attach in handleRequestSuportDto.Attachments)
                {
                    var attachment = new Attachment
                    {
                        Id = Guid.NewGuid(),
                        RequestSuportId = requestSuport.Id,
                        FilePath = attach.FilePath,
                        FileExtend = attach.FileExtend,
                        FileName = attach.FileName,
                        FileSize = attach.FileSize,
                        CreatedTime = attach.CreatedTime
                    };
                    await _attachmentRepo.InsertAsync(attachment);
                }
            }
            await _requestSuportRepo.InsertAsync(requestSuport);

            return await _requestSuportRepo.SaveChangesAsync() > 0;

        }

        public async Task<bool> DeleteAssync(Guid requestITSuportId)
        {
            var requestSuport = await _requestSuportRepo.FirstOrDefaultAsync(x => x.Id == requestITSuportId);
            if (requestSuport != null)
            {
                return await _requestSuportRepo.DeleteAsync(requestSuport);
            }
            return false;
        }

        public async Task<List<RequestSuportDto>> GetAllAsync()
        {
            var user = await _userRepo.GetQueryableAsync();
            var query = from requestSuport in await _requestSuportRepo.GetQueryableAsync()
                        join created in user on requestSuport.CreatedById equals created.Id


                        join requestCategory in await _requestSuportCategoryRepo.GetQueryableAsync()
                        on requestSuport.RequestSuportCategoryId equals requestCategory.Id

                        join requestBy in user on requestSuport.RequestBy equals requestBy.Id

                        join technicial in user on requestSuport.TechnicianId equals technicial.Id
                        into t
                        from technicial in t.DefaultIfEmpty()

                        join department in await _departmentRepo.GetQueryableAsync()
                        on requestBy.DepartmentId equals department.Id
                        into d
                        from department in d.DefaultIfEmpty()

                        join factory in await _factoryRepo.GetQueryableAsync()
                        on requestBy.FactoryId equals factory.Id
                        into f
                        from factory in f.DefaultIfEmpty()
                        select new RequestSuportDto
                        {
                            Id = requestSuport.Id,
                            RequestTitle = requestSuport.RequestTitle,
                            RequestSuportCategoryId = requestSuport.RequestSuportCategoryId,
                            RequestSuportCategoryName = requestCategory.Name,
                            Status = requestSuport.Status,
                            RequestById = requestSuport.RequestBy,
                            RequestByName = requestBy.FullName,
                            RequestByEmail = requestBy.Email,
                            CreatedDate = requestSuport.CreatedDate,
                            RequestDescription = requestSuport.RequestDescription,
                            RequestSolution = requestSuport.RequestSolution,
                            TechnicianId = requestSuport.TechnicianId,
                            TechnicianName = technicial.FullName,
                            TechnicianEmail = technicial.Email,
                            InProgessDate = requestSuport.InProgessDate,
                            DueDate = requestSuport.DueDate,
                            ResolveDate = requestSuport.ResolveDate,
                            CancelDate = requestSuport.CancelDate,
                            Department = department.Name,
                            Factory = $"{factory.Name} - {factory.Code}",
                            CreatedName = created.FullName,
                            CreatedEmail = created.Email,
                        };
            var data = query
                    .OrderByDescending(x => x.CreatedDate)
                    .ToList();
            if (data != null)
            {
                return data;
            }
            return new List<RequestSuportDto>();
        }

        public async Task<List<AttachmentDto>> LoadListAttachment(Guid requestITSuportId)
        {
            var query = from attachment in await _attachmentRepo.GetQueryableAsync()
                        where attachment.RequestSuportId == requestITSuportId
                        select new AttachmentDto
                        {
                            Id = attachment.Id,
                            RequestSuportId = attachment.RequestSuportId,
                            FileExtend = attachment.FileExtend,
                            FileName = attachment.FileName,
                            FilePath = attachment.FilePath,
                            FileSize = attachment.FileSize,
                            CreatedTime = attachment.CreatedTime
                        };
            var data = query.OrderByDescending(x => x.CreatedTime)
                    .ToList();
            if (data != null)
            {
                return data;
            }
            return new List<AttachmentDto>();
        }

        public async Task<bool> UpdateAsync(HandleRequestSuportDto handleRequestSuportDto)
        {
            var requestSuport = await _requestSuportRepo.FirstOrDefaultAsync(x => x.Id == handleRequestSuportDto.Id);
            if (requestSuport != null)
            {
                requestSuport.RequestTitle = handleRequestSuportDto.RequestTitle;
                requestSuport.RequestSuportCategoryId = handleRequestSuportDto.RequestSuportCategoryId;
                requestSuport.Status = handleRequestSuportDto.Status;
                requestSuport.RequestBy = handleRequestSuportDto.RequestBy;
                requestSuport.CreatedDate = handleRequestSuportDto.CreatedDate;
                requestSuport.RequestDescription = handleRequestSuportDto.RequestDescription;
                requestSuport.RequestSolution = handleRequestSuportDto.RequestSolution;
                requestSuport.TechnicianId = handleRequestSuportDto.TechnicianId;
                switch (requestSuport.Status)
                {
                    case RequestSuportStatusType.InProgress:
                        requestSuport.InProgessDate = handleRequestSuportDto.InProgessDate;
                        break;
                    case RequestSuportStatusType.Resolved:
                        requestSuport.ResolveDate = handleRequestSuportDto.ResolveDate;
                        break;
                    case RequestSuportStatusType.OnHold:
                        requestSuport.OnHoldDate = handleRequestSuportDto.OnHoldDate;
                        break;
                    case RequestSuportStatusType.Cancel:
                        requestSuport.CancelDate = handleRequestSuportDto.CancelDate;
                        break;

                }
                requestSuport.DueDate = handleRequestSuportDto.DueDate;

                var attachments = await _attachmentRepo.GetListAsync(x => x.RequestSuportId == requestSuport.Id);
                if (attachments != null && attachments.Any())
                {
                    foreach (var att in attachments)
                    {
                        await _attachmentRepo.DeleteAsync(att);
                    }
                }
                if (handleRequestSuportDto.Attachments != null)
                {
                    foreach (var attach in handleRequestSuportDto.Attachments)
                    {
                        var attachment = new Attachment
                        {
                            Id = Guid.NewGuid(),
                            RequestSuportId = requestSuport.Id,
                            FilePath = attach.FilePath,
                            FileExtend = attach.FileExtend,
                            FileName = attach.FileName,
                            FileSize = attach.FileSize,
                            CreatedTime = attach.CreatedTime
                        };
                        await _attachmentRepo.InsertAsync(attachment);
                    }
                }
                await _requestSuportRepo.SaveChangesAsync();
                return await _requestSuportRepo.UpdateAsync(requestSuport);
            }
            return false;
        }

    }
}
