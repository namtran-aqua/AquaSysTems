using AntDesign;
using AquaSolution.Data.Data.Entities.Admin;
using AquaSolution.Data.Data.Entities.RequestITSuports;
using AquaSolution.Data.Repositories;
using AquaSolution.Shared.Enum;
using AquaSolution.Shared.ITSuport.Attachments;
using AquaSolution.Shared.ITSuport.RequestSuport;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

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

        //public async Task<List<RequestSuportDto>> GetAllAsync()
        //{
        //    var user = await _userRepo.GetQueryableAsync();
        //    var query = from requestSuport in await _requestSuportRepo.GetQueryableAsync()
        //                join created in user on requestSuport.CreatedById equals created.Id


        //                join requestCategory in await _requestSuportCategoryRepo.GetQueryableAsync()
        //                on requestSuport.RequestSuportCategoryId equals requestCategory.Id

        //                join requestBy in user on requestSuport.RequestBy equals requestBy.Id

        //                join technicial in user on requestSuport.TechnicianId equals technicial.Id
        //                into t
        //                from technicial in t.DefaultIfEmpty()

        //                join department in await _departmentRepo.GetQueryableAsync()
        //                on requestBy.DepartmentId equals department.Id
        //                into d
        //                from department in d.DefaultIfEmpty()

        //                join factory in await _factoryRepo.GetQueryableAsync()
        //                on requestBy.FactoryId equals factory.Id
        //                into f
        //                from factory in f.DefaultIfEmpty()
        //                select new RequestSuportDto
        //                {
        //                    Id = requestSuport.Id,
        //                    RequestTitle = requestSuport.RequestTitle,
        //                    RequestSuportCategoryId = requestSuport.RequestSuportCategoryId,
        //                    RequestSuportCategoryName = requestCategory.Name,
        //                    Status = requestSuport.Status,
        //                    RequestById = requestSuport.RequestBy,
        //                    RequestByName = requestBy.FullName,
        //                    RequestByEmail = requestBy.Email,
        //                    CreatedDate = requestSuport.CreatedDate,
        //                    RequestDescription = requestSuport.RequestDescription,
        //                    RequestSolution = requestSuport.RequestSolution,
        //                    TechnicianId = requestSuport.TechnicianId,
        //                    TechnicianName = technicial.FullName,
        //                    TechnicianEmail = technicial.Email,
        //                    InProgessDate = requestSuport.InProgessDate,
        //                    DueDate = requestSuport.DueDate,
        //                    ResolveDate = requestSuport.ResolveDate,
        //                    CancelDate = requestSuport.CancelDate,
        //                    Department = department.Name,
        //                    Factory = $"{factory.Name} - {factory.Code}",
        //                    CreatedName = created.FullName,
        //                    CreatedEmail = created.Email,
        //                    CreatedById = requestSuport.CreatedById,
        //                    TicketNumber = requestSuport.TicketNumber,
        //                };
        //    var data = query
        //            .OrderByDescending(x => x.CreatedDate)
        //            .ToList();
        //    if (data != null)
        //    {
        //        return data;
        //    }
        //    return new List<RequestSuportDto>();
        //}
        public async Task<List<RequestSuportDto>> GetAllAsync()
        {
            var users = _userRepo.Query();
            var requestSuports = _requestSuportRepo.Query();
            var categories = _requestSuportCategoryRepo.Query();
            var departments = _departmentRepo.Query();
            var factories = _factoryRepo.Query();

            var query = from requestSuport in requestSuports
                        join created in users on requestSuport.CreatedById equals created.Id
                        join requestCategory in categories
                            on requestSuport.RequestSuportCategoryId equals requestCategory.Id

                        join requestBy in users on requestSuport.RequestBy equals requestBy.Id
                        join technicial in users on requestSuport.TechnicianId equals technicial.Id
                            into t
                        from technicial in t.DefaultIfEmpty()

                        join department in departments on requestBy.DepartmentId equals department.Id
                            into d
                        from department in d.DefaultIfEmpty()
                        join factory in factories on requestBy.FactoryId equals factory.Id
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
                            TechnicianName = technicial != null ? technicial.FullName : null,
                            TechnicianEmail = technicial != null ? technicial.Email : null,
                            InProgessDate = requestSuport.InProgessDate,
                            DueDate = requestSuport.DueDate,
                            ResolveDate = requestSuport.ResolveDate,
                            CancelDate = requestSuport.CancelDate,
                            Department = department != null ? department.Name : null,
                            Factory = factory != null ? $"{factory.Name} - {factory.Code}" : null,
                            CreatedName = created.FullName,
                            CreatedEmail = created.Email,
                            CreatedById = requestSuport.CreatedById,
                            TicketNumber = requestSuport.TicketNumber,
                            RequestByWorkDay = requestBy.WorkDayId,
                        };

            return await query
                .OrderByDescending(x => x.CreatedDate)
                .ToListAsync();
        }
        public async Task<PagedResult<RequestSuportDto>> GetPagedAsync(RequestSuportQueryDto request)
        {
            var users = _userRepo.Query();
            var requestSuports = _requestSuportRepo.Query();
            var categories = _requestSuportCategoryRepo.Query();
            var departments = _departmentRepo.Query();
            var factories = _factoryRepo.Query();

            var query = from requestSuport in requestSuports
                        join created in users on requestSuport.CreatedById equals created.Id
                        join requestCategory in categories
                            on requestSuport.RequestSuportCategoryId equals requestCategory.Id
                        join requestBy in users on requestSuport.RequestBy equals requestBy.Id
                        join technicial in users on requestSuport.TechnicianId equals technicial.Id
                            into t
                        from technicial in t.DefaultIfEmpty()
                        join department in departments on requestBy.DepartmentId equals department.Id
                            into d
                        from department in d.DefaultIfEmpty()
                        join factory in factories on requestBy.FactoryId equals factory.Id
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
                            TechnicianName = technicial != null ? technicial.FullName : null,
                            TechnicianEmail = technicial != null ? technicial.Email : null,
                            InProgessDate = requestSuport.InProgessDate,
                            DueDate = requestSuport.DueDate,
                            ResolveDate = requestSuport.ResolveDate,
                            CancelDate = requestSuport.CancelDate,
                            Department = department != null ? department.Name : null,
                            Factory = factory != null ? $"{factory.Name} - {factory.Code}" : null,
                            CreatedName = created.FullName,
                            CreatedEmail = created.Email,
                            CreatedById = requestSuport.CreatedById,
                            TicketNumber = requestSuport.TicketNumber,
                            RequestByWorkDay = requestBy.WorkDayId,
                        };

            // Filter theo role
            if (!request.IsITOrAdmin && request.CurrentUserId.HasValue)
                query = query.Where(x => x.RequestById == request.CurrentUserId);

            // Filter search text
            if (!string.IsNullOrEmpty(request.RequesterName))
                query = query.Where(x => x.RequestByName.Contains(request.RequesterName));

            if (!string.IsNullOrEmpty(request.RequesterEmail))
                query = query.Where(x => x.RequestByEmail.Contains(request.RequesterEmail));

            if (request.TicketCode.HasValue)
                query = query.Where(x => x.TicketNumber == request.TicketCode.Value);

            // Filter status (multi-select từ toolbar)
            if (request.Statuses != null && request.Statuses.Any())
            {
                var statusEnums = request.Statuses
                    .Select(s => Enum.TryParse<RequestSuportStatusType>(s, out var e) ? (RequestSuportStatusType?)e : null)
                    .Where(e => e.HasValue)
                    .Select(e => e!.Value)
                    .ToList();

                if (statusEnums.Any())
                    query = query.Where(x => statusEnums.Contains(x.Status));
            }

            // Filter technician (multi-select từ toolbar)
            if (request.TechnicianNames != null && request.TechnicianNames.Any())
                query = query.Where(x => x.TechnicianName != null && request.TechnicianNames.Contains(x.TechnicianName));

            var total = await query.CountAsync();

            var items = await query
                .OrderByDescending(x => x.CreatedDate)
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync();

            return new PagedResult<RequestSuportDto> { Items = items, Total = total };
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
            if (requestSuport == null) return false;

            requestSuport.RequestTitle = handleRequestSuportDto.RequestTitle;
            requestSuport.RequestSuportCategoryId = handleRequestSuportDto.RequestSuportCategoryId;
            requestSuport.Status = handleRequestSuportDto.Status;
            requestSuport.RequestBy = handleRequestSuportDto.RequestBy;
            requestSuport.CreatedDate = handleRequestSuportDto.CreatedDate;
            requestSuport.RequestDescription = handleRequestSuportDto.RequestDescription;
            requestSuport.RequestSolution = handleRequestSuportDto.RequestSolution;
            requestSuport.TechnicianId = handleRequestSuportDto.TechnicianId;
            requestSuport.DueDate = handleRequestSuportDto.DueDate;

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

            // Diff attachment
            var existing = await _attachmentRepo.GetListAsync(x => x.RequestSuportId == requestSuport.Id);
            var incomingPaths = handleRequestSuportDto.Attachments?.Select(a => a.FilePath).ToHashSet() ?? new();
            var existingPaths = existing.Select(a => a.FilePath).ToHashSet();

            foreach (var att in existing.Where(a => !incomingPaths.Contains(a.FilePath)))
                await _attachmentRepo.DeleteAsync(att);

            foreach (var attach in (handleRequestSuportDto.Attachments ?? new List<AttachmentDto>())
                .Where(a => !existingPaths.Contains(a.FilePath)))
            {
                await _attachmentRepo.InsertAsync(new Attachment
                {
                    Id = Guid.NewGuid(),
                    RequestSuportId = requestSuport.Id,
                    FilePath = attach.FilePath,
                    FileExtend = attach.FileExtend,
                    FileName = attach.FileName,
                    FileSize = attach.FileSize,
                    CreatedTime = attach.CreatedTime
                });
            }

            return await _requestSuportRepo.UpdateAsync(requestSuport);
        }
        public async Task<RequestSuportStatsDto> GetStatsAsync(Guid? currentUserId, bool isITOrAdmin)
        {
            var query = _requestSuportRepo.Query();

            // Giới hạn theo role — user thường chỉ thấy ticket của mình
            if (!isITOrAdmin && currentUserId.HasValue)
                query = query.Where(x => x.RequestBy == currentUserId);

            var stats = await query
                .GroupBy(_ => 1)
                .Select(g => new RequestSuportStatsDto
                {
                    Total = g.Count(),
                    Open = g.Count(x => x.Status == RequestSuportStatusType.Open),
                    InProgress = g.Count(x => x.Status == RequestSuportStatusType.InProgress),
                    Resolved = g.Count(x => x.Status == RequestSuportStatusType.Resolved),
                    Cancel =g.Count(x=>x.Status == RequestSuportStatusType.Cancel)
                })
                .FirstOrDefaultAsync();

            return stats ?? new RequestSuportStatsDto();
        }

    }
}
