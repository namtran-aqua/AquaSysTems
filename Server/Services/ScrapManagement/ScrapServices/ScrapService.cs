using AquaSolution.Data.Data.Entities.Admin;
using AquaSolution.Data.Data.Entities.Scraps;
using AquaSolution.Data.Entities.Scraps;
using AquaSolution.Data.Repositories;
using AquaSolution.Shared.Enum.Scrap;
using AquaSolution.Shared.ScrapManagement.Materials;
using AquaSolution.Shared.ScrapManagement.Scrap;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AquaSolution.Server.Services.ScrapManagetment.ScapServices
{
    public class ScrapService : IScrapService
    {
        private readonly IRepository<Material> _materialRepository;
        private readonly IRepository<Weight> _weightRepository;
        private readonly IRepository<Factory> _factoryRepository;
        private readonly IRepository<Department> _departmentRepository;
        private readonly IRepository<User> _userRepository;
        private readonly IRepository<HistoryScrap> _historyScrapRepository;
        private readonly IRepository<HistoryScrapDetail> _historyScrapDetailRepository;
        private readonly IRepository<FlowApprovalScrap> _flowApprovalScrapRepository;
        private readonly IRepository<RequestApproval> _requestApprovalRepository;

        public ScrapService(IRepository<Material> materialRepository,
                IRepository<Factory> factoryRepository,
                IRepository<Department> departmentRepository,
                IRepository<User> userRepository,
                IRepository<HistoryScrap> historyScrapRepository,
                IRepository<HistoryScrapDetail> historyScrapDetailRepository,
                IRepository<FlowApprovalScrap> flowApprovalScrapRepository,
                IRepository<RequestApproval> requestApprovalRepository,
                IRepository<Weight> weightRepository)
        {
            _materialRepository = materialRepository;
            _weightRepository = weightRepository;
            _factoryRepository = factoryRepository;
            _departmentRepository = departmentRepository;
            _userRepository = userRepository;
            _historyScrapRepository = historyScrapRepository;
            _historyScrapDetailRepository = historyScrapDetailRepository;
            _flowApprovalScrapRepository = flowApprovalScrapRepository;
            _requestApprovalRepository = requestApprovalRepository;
        }

        // ── Không đụng vào ──────────────────────────────────────────────────────────
        public async Task CreateScrap(HandleScrapDto createScrapDto)
        {
            var factory = await _factoryRepository.GetByIdAsync(createScrapDto.FactoryId)
                ?? throw new Exception($"Factory không tồn tại: {createScrapDto.FactoryId}");

            var department = await _departmentRepository.GetByIdAsync(createScrapDto.DepartmentId)
                ?? throw new Exception($"Department không tồn tại: {createScrapDto.DepartmentId}");

            var historyScrapId = Guid.NewGuid();
            decimal totalAmount = 0;
            if (createScrapDto.HistoryDetails != null && createScrapDto.HistoryDetails.Any())
                totalAmount = createScrapDto.HistoryDetails.Sum(d => d.Quantity * d.Weight);

            var historyScrap = new HistoryScrap
            {
                Id = historyScrapId,
                Title = createScrapDto.Title,
                Status = StatusScrap.Pending,
                Description = createScrapDto.Description,
                CreatedDate = createScrapDto.CreatedDate,
                CreatedBy = createScrapDto.CreatedById,
                FactoryId = createScrapDto.FactoryId,
                DepartmentId = createScrapDto.DepartmentId,
                TotalAmount = totalAmount,
            };

            await _historyScrapRepository.InsertAsync(historyScrap);

            if (createScrapDto.HistoryDetails != null)
            {
                var historyDetails = createScrapDto.HistoryDetails.Select(detail => new HistoryScrapDetail
                {
                    Id = Guid.NewGuid(),
                    MaterialId = detail.MaterialId,
                    ScrapHistoryId = historyScrapId,
                    BOMHead = detail.BOMHead,
                    BOMDescription = detail.BOMDescription,
                    TYPE = detail.TYPE,
                    Plant = detail.Plant,
                    Code = detail.Code,
                    Name = detail.Name,
                    Unit = detail.Unit,
                    Quantity = detail.Quantity,
                    Weight = detail.Weight,
                    TotalWeight = detail.Quantity * detail.Weight,
                    Reson = detail.Reson,
                }).ToList();
                await _historyScrapDetailRepository.InsertRangeAsync(historyDetails);
            }

            var flowSteps = await _flowApprovalScrapRepository.Query()
                .Where(x => x.FactoryId == createScrapDto.FactoryId && x.DepartmentId == createScrapDto.DepartmentId)
                .OrderBy(x => x.Step)
                .ToListAsync();

            if (flowSteps.Any())
            {
                var requestApprovals = flowSteps.Select((step, index) => new RequestApproval
                {
                    Id = Guid.NewGuid(),
                    HistoryScrapId = historyScrapId,
                    Title = step.Name,
                    Step = step.Step,
                    DecisionMaker = step.DecisionMaker,
                    Status = step.Step == 1 ? StatusScrap.InterView : StatusScrap.Pending,
                    Comment = null,
                    ActionBy = null,
                    ActionDate = null
                }).ToList();

                await _requestApprovalRepository.InsertRangeAsync(requestApprovals);
            }

            await _historyScrapRepository.SaveChangesAsync();
        }

        // ── Không đụng vào ──────────────────────────────────────────────────────────
        public async Task<List<HistoryScrapDto>> GetHistory()
        {
            var query = from hs in _historyScrapRepository.Query()
                        join factory in _factoryRepository.Query() on hs.FactoryId equals factory.Id
                        join department in _departmentRepository.Query() on hs.DepartmentId equals department.Id
                        join creator in _userRepository.Query() on hs.CreatedBy equals creator.Id
                        join lastUser in _userRepository.Query() on hs.LastActionBy equals lastUser.Id into lastUserGroup
                        from lastUser in lastUserGroup.DefaultIfEmpty()
                        join confirmer in _userRepository.Query() on hs.Confirmer equals confirmer.Id into confirmerGroup
                        from confirmer in confirmerGroup.DefaultIfEmpty()
                        select new HistoryScrapDto
                        {
                            Id = hs.Id,
                            Title = hs.Title,
                            Status = hs.Status,
                            Description = hs.Description,
                            LastActionBy = hs.LastActionBy,
                            LastActionByName = lastUser != null ? lastUser.FullName : string.Empty,
                            LastActionDate = hs.LastActionDate,
                            CreatedDate = hs.CreatedDate,
                            CreatedById = hs.CreatedBy,
                            CreatedByName = creator.FullName,
                            FactoryId = hs.FactoryId,
                            FactoryName = factory.Name,
                            DepartmentId = hs.DepartmentId,
                            DepartmentName = department.Name,
                            Notes = hs.Notes,
                            ConfirmationStatusType = hs.ConfirmationStatusType,
                            ConfirmAmount = hs.ConfirmAmount,
                            TotalAmount = hs.TotalAmount,
                            ConfirmerId = hs.Confirmer,
                            ConfirmerName = confirmer != null ? confirmer.FullName : string.Empty,
                            ConfirmedDate = hs.ConfirmDate
                        };

            var historyScrapDtos = await query.ToListAsync();
            var ids = historyScrapDtos.Select(x => x.Id).ToList();
            var allDetails = await _historyScrapDetailRepository.Query()
                                  .Where(d => ids.Contains(d.ScrapHistoryId)).ToListAsync();
            var materialIds = allDetails.Select(d => d.MaterialId).Distinct().ToList();
            var materials = await _materialRepository.Query()
                                  .Where(m => materialIds.Contains(m.Id)).ToDictionaryAsync(m => m.Id);
            var detailsGrouped = allDetails.GroupBy(d => d.ScrapHistoryId)
                                           .ToDictionary(g => g.Key, g => g.ToList());
            var allApprovals = await _requestApprovalRepository.Query()
                                  .Where(a => ids.Contains(a.HistoryScrapId))
                                  .OrderBy(a => a.Step).ToListAsync();
            var approvalUserIds = allApprovals.Select(a => a.DecisionMaker)
                                  .Union(allApprovals.Where(a => a.ActionBy.HasValue).Select(a => a.ActionBy.Value))
                                  .Distinct().ToList();
            var approvalUsers = await _userRepository.Query()
                                  .Where(u => approvalUserIds.Contains(u.Id))
                                  .ToDictionaryAsync(u => u.Id, u => new { u.FullName, u.Email, u.WorkDayId });
            var approvalsGrouped = allApprovals.GroupBy(a => a.HistoryScrapId)
                                               .ToDictionary(g => g.Key, g => g.ToList());

            foreach (var dto in historyScrapDtos)
            {
                if (detailsGrouped.TryGetValue(dto.Id, out var details))
                {
                    dto.HistoryDetails = details.Select(d =>
                    {
                        materials.TryGetValue(d.MaterialId, out var mat);
                        return new HistoryDetailScrapDto
                        {
                            Id = d.Id,
                            MaterialId = d.MaterialId,
                            Name = mat?.Name ?? string.Empty,
                            Code = mat?.Code ?? string.Empty,
                            Unit = mat?.Unit ?? string.Empty,
                            BOMHead = d.BOMHead,
                            BOMDescription = d.BOMDescription,
                            Quantity = d.Quantity,
                            Weight = d.Weight,
                            TotalWeight = d.TotalWeight,
                            TYPE = d.TYPE,
                            Plant = d.Plant,
                            ScrapHistoryId = d.ScrapHistoryId,
                            Reson = d.Reson
                        };
                    }).ToList();
                }

                if (approvalsGrouped.TryGetValue(dto.Id, out var approvals))
                {
                    dto.Approvals = approvals.Select(a =>
                    {
                        approvalUsers.TryGetValue(a.DecisionMaker, out var decisionMakerInfo);
                        string? actionByName = null, actionByWorkDayId = null, actionByEmail = null;
                        if (a.ActionBy.HasValue && approvalUsers.TryGetValue(a.ActionBy.Value, out var actionByInfo))
                        {
                            actionByName = actionByInfo.FullName;
                            actionByWorkDayId = actionByInfo.WorkDayId;
                            actionByEmail = actionByInfo.Email;
                        }
                        return new RequestApprovalDto
                        {
                            Id = a.Id,
                            HistoryScrapId = a.HistoryScrapId,
                            Status = a.Status,
                            Comment = a.Comment,
                            ActionBy = a.ActionBy,
                            ActionByName = actionByName,
                            ActionByWorkDayId = actionByWorkDayId,
                            ActionByEmail = actionByEmail,
                            ActionDate = a.ActionDate,
                            Title = a.Title,
                            Step = a.Step,
                            DecisionMaker = a.DecisionMaker,
                            DecisionMakerName = decisionMakerInfo?.FullName,
                            DecisionMakerWorkDayId = decisionMakerInfo?.WorkDayId,
                            DecisionMakerEmail = decisionMakerInfo?.Email
                        };
                    }).ToList();
                }
            }

            return historyScrapDtos;
        }

        // ── SỬA: lấy tất cả phiếu user từng tham gia duyệt (không chỉ InterView) ──
        public async Task<List<HistoryScrapDto>> GetScrapForApproval(Guid userId)
        {
            // Lấy tất cả phiếu mà user này là DecisionMaker ở bất kỳ step nào
            // (InterView = đang chờ, Approved = đã duyệt, Rejected = đã từ chối)
            // Loại trừ Pending vì Pending = chưa đến lượt user này
            var relatedScrapIds = await _requestApprovalRepository.Query()
                .Where(a => a.DecisionMaker == userId
                         && a.Status != StatusScrap.Pending)
                .Select(a => a.HistoryScrapId)
                .Distinct()
                .ToListAsync();

            if (!relatedScrapIds.Any())
                return new List<HistoryScrapDto>();

            var query = from hs in _historyScrapRepository.Query()
                            .Where(x => relatedScrapIds.Contains(x.Id))
                        join factory in _factoryRepository.Query() on hs.FactoryId equals factory.Id
                        join department in _departmentRepository.Query() on hs.DepartmentId equals department.Id
                        join creator in _userRepository.Query() on hs.CreatedBy equals creator.Id
                        join lastUser in _userRepository.Query() on hs.LastActionBy equals lastUser.Id into lastUserGroup
                        from lastUser in lastUserGroup.DefaultIfEmpty()
                        join confirmer in _userRepository.Query() on hs.Confirmer equals confirmer.Id into confirmerGroup
                        from confirmer in confirmerGroup.DefaultIfEmpty()
                        select new HistoryScrapDto
                        {
                            Id = hs.Id,
                            Title = hs.Title,
                            Status = hs.Status,
                            Description = hs.Description,
                            LastActionBy = hs.LastActionBy,
                            LastActionByName = lastUser != null ? lastUser.FullName : string.Empty,
                            LastActionDate = hs.LastActionDate,
                            CreatedDate = hs.CreatedDate,
                            CreatedById = hs.CreatedBy,
                            CreatedByName = creator.FullName,
                            FactoryId = hs.FactoryId,
                            FactoryName = factory.Name,
                            DepartmentId = hs.DepartmentId,
                            DepartmentName = department.Name,
                            Notes = hs.Notes,
                            ConfirmationStatusType = hs.ConfirmationStatusType,
                            ConfirmAmount = hs.ConfirmAmount,
                            TotalAmount = hs.TotalAmount,
                            ConfirmerId = hs.Confirmer,
                            ConfirmerName = confirmer != null ? confirmer.FullName : string.Empty,
                            ConfirmedDate = hs.ConfirmDate
                        };

            var historyScrapDtos = await query.ToListAsync();
            var ids = historyScrapDtos.Select(x => x.Id).ToList();

            var allDetails = await _historyScrapDetailRepository.Query()
                                  .Where(d => ids.Contains(d.ScrapHistoryId)).ToListAsync();
            var materialIds = allDetails.Select(d => d.MaterialId).Distinct().ToList();
            var materials = await _materialRepository.Query()
                                  .Where(m => materialIds.Contains(m.Id)).ToDictionaryAsync(m => m.Id);
            var detailsGrouped = allDetails.GroupBy(d => d.ScrapHistoryId)
                                           .ToDictionary(g => g.Key, g => g.ToList());

            var allApprovals = await _requestApprovalRepository.Query()
                                  .Where(a => ids.Contains(a.HistoryScrapId))
                                  .OrderBy(a => a.Step).ToListAsync();
            var approvalUserIds = allApprovals.Select(a => a.DecisionMaker)
                                  .Union(allApprovals.Where(a => a.ActionBy.HasValue).Select(a => a.ActionBy.Value))
                                  .Distinct().ToList();
            var approvalUsers = await _userRepository.Query()
                                  .Where(u => approvalUserIds.Contains(u.Id))
                                  .ToDictionaryAsync(u => u.Id, u => new { u.FullName, u.Email, u.WorkDayId });
            var approvalsGrouped = allApprovals.GroupBy(a => a.HistoryScrapId)
                                               .ToDictionary(g => g.Key, g => g.ToList());

            foreach (var dto in historyScrapDtos)
            {
                if (detailsGrouped.TryGetValue(dto.Id, out var details))
                {
                    dto.HistoryDetails = details.Select(d =>
                    {
                        materials.TryGetValue(d.MaterialId, out var mat);
                        return new HistoryDetailScrapDto
                        {
                            Id = d.Id,
                            MaterialId = d.MaterialId,
                            Name = mat?.Name ?? string.Empty,
                            Code = mat?.Code ?? string.Empty,
                            Unit = mat?.Unit ?? string.Empty,
                            BOMHead = d.BOMHead,
                            BOMDescription = d.BOMDescription,
                            Quantity = d.Quantity,
                            Weight = d.Weight,
                            TotalWeight = d.TotalWeight,
                            TYPE = d.TYPE,
                            Plant = d.Plant,
                            ScrapHistoryId = d.ScrapHistoryId,
                            Reson = d.Reson
                        };
                    }).ToList();
                }

                if (approvalsGrouped.TryGetValue(dto.Id, out var approvals))
                {
                    dto.Approvals = approvals.Select(a =>
                    {
                        approvalUsers.TryGetValue(a.DecisionMaker, out var decisionMakerInfo);
                        string? actionByName = null, actionByWorkDayId = null, actionByEmail = null;
                        if (a.ActionBy.HasValue && approvalUsers.TryGetValue(a.ActionBy.Value, out var actionByInfo))
                        {
                            actionByName = actionByInfo.FullName;
                            actionByWorkDayId = actionByInfo.WorkDayId;
                            actionByEmail = actionByInfo.Email;
                        }
                        return new RequestApprovalDto
                        {
                            Id = a.Id,
                            HistoryScrapId = a.HistoryScrapId,
                            Status = a.Status,
                            Comment = a.Comment,
                            ActionBy = a.ActionBy,
                            ActionByName = actionByName,
                            ActionByWorkDayId = actionByWorkDayId,
                            ActionByEmail = actionByEmail,
                            ActionDate = a.ActionDate,
                            Title = a.Title,
                            Step = a.Step,
                            DecisionMaker = a.DecisionMaker,
                            DecisionMakerName = decisionMakerInfo?.FullName,
                            DecisionMakerWorkDayId = decisionMakerInfo?.WorkDayId,
                            DecisionMakerEmail = decisionMakerInfo?.Email
                        };
                    }).ToList();
                }
            }

            return historyScrapDtos;
        }

        public async Task ActionApproval(ApprovalActionDto request)
        {
            var scrap = await _historyScrapRepository.GetByIdAsync(request.HistoryScrapId)
                        ?? throw new Exception("Không tìm thấy phiếu Scrap");

            var approvals = await _requestApprovalRepository.Query()
                                 .Where(a => a.HistoryScrapId == request.HistoryScrapId)
                                 .OrderBy(a => a.Step)
                                 .ToListAsync();

            var currentStep = approvals.FirstOrDefault(a => a.Status == StatusScrap.InterView);
            if (currentStep == null) throw new Exception("Phiếu không nằm trong trạng thái chờ duyệt của bước nào!");

            if (currentStep.DecisionMaker != request.ActionBy)
                throw new Exception("Bạn không phải là người duyệt bước này!");

            var now = DateTime.Now;

            // Ghi nhận action trên step hiện tại
            currentStep.ActionBy = request.ActionBy;
            currentStep.ActionDate = now;
            currentStep.Comment = request.Comment;

            //// LastAction luôn là người vừa thao tác (approve hoặc reject)
           

            if (!request.IsApproved)
            {
                // Reject ở bất kỳ step nào → đóng phiếu luôn
                currentStep.Status = StatusScrap.Rejected;
                scrap.Status = StatusScrap.Rejected;

                await _requestApprovalRepository.UpdateAsync(currentStep);
            }
            else
            {
                currentStep.Status = StatusScrap.Approved;

                var nextStep = approvals.FirstOrDefault(a => a.Step == currentStep.Step + 1);

                if (nextStep != null)
                {
                    // Còn step tiếp theo → mở step đó, scrap vẫn InterView
                    nextStep.Status = StatusScrap.InterView;
                    //scrap.Status = StatusScrap.InterView;

                    await _requestApprovalRepository.UpdateAsync(currentStep);
                    await _requestApprovalRepository.UpdateAsync(nextStep);
                }
                else
                {
                    // Đây là step cuối → phiếu được duyệt hoàn toàn
                    scrap.Status = StatusScrap.Approved;
                    scrap.LastActionBy = request.ActionBy;
                    scrap.LastActionDate = now;
                    await _requestApprovalRepository.UpdateAsync(currentStep);
                }
            }

            await _historyScrapRepository.UpdateAsync(scrap);
        }

        // ── Không đụng vào ──────────────────────────────────────────────────────────
        public async Task<List<HistoryScrapDto>> GetScrapForConfirm()
        {
            var query = from hs in _historyScrapRepository.Query()
                            .Where(x => x.Status == StatusScrap.Approved
                                    || x.Status == StatusScrap.Done)
                        join factory in _factoryRepository.Query() on hs.FactoryId equals factory.Id
                        join department in _departmentRepository.Query() on hs.DepartmentId equals department.Id
                        join creator in _userRepository.Query() on hs.CreatedBy equals creator.Id
                        join lastUser in _userRepository.Query() on hs.LastActionBy equals lastUser.Id into lastUserGroup
                        from lastUser in lastUserGroup.DefaultIfEmpty()
                        join confirmer in _userRepository.Query() on hs.Confirmer equals confirmer.Id into confirmerGroup
                        from confirmer in confirmerGroup.DefaultIfEmpty()
                        select new HistoryScrapDto
                        {
                            Id = hs.Id,
                            Title = hs.Title,
                            Status = hs.Status,
                            Description = hs.Description,
                            LastActionBy = hs.LastActionBy,
                            LastActionByName = lastUser != null ? lastUser.FullName : string.Empty,
                            LastActionDate = hs.LastActionDate,
                            CreatedDate = hs.CreatedDate,
                            CreatedById = hs.CreatedBy,
                            CreatedByName = creator.FullName,
                            FactoryId = hs.FactoryId,
                            FactoryName = factory.Name,
                            DepartmentId = hs.DepartmentId,
                            DepartmentName = department.Name,
                            Notes = hs.Notes,
                            ConfirmationStatusType = hs.ConfirmationStatusType,
                            ConfirmAmount = hs.ConfirmAmount,
                            TotalAmount = hs.TotalAmount,
                            ConfirmerId = hs.Confirmer,
                            ConfirmerName = confirmer != null ? confirmer.FullName : string.Empty,
                            ConfirmedDate = hs.ConfirmDate
                        };

            var historyScrapDtos = await query.ToListAsync();
            var ids = historyScrapDtos.Select(x => x.Id).ToList();

            var allDetails = await _historyScrapDetailRepository.Query()
                .Where(d => ids.Contains(d.ScrapHistoryId)).ToListAsync();
            var materialIds = allDetails.Select(d => d.MaterialId).Distinct().ToList();
            var materials = await _materialRepository.Query()
                .Where(m => materialIds.Contains(m.Id)).ToDictionaryAsync(m => m.Id);
            var detailsGrouped = allDetails.GroupBy(d => d.ScrapHistoryId)
                .ToDictionary(g => g.Key, g => g.ToList());

            var allApprovals = await _requestApprovalRepository.Query()
                .Where(a => ids.Contains(a.HistoryScrapId)).OrderBy(a => a.Step).ToListAsync();
            var approvalUserIds = allApprovals.Select(a => a.DecisionMaker)
                .Union(allApprovals.Where(a => a.ActionBy.HasValue).Select(a => a.ActionBy!.Value))
                .Distinct().ToList();
            var approvalUsers = await _userRepository.Query()
                .Where(u => approvalUserIds.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id, u => new { u.FullName, u.Email, u.WorkDayId });
            var approvalsGrouped = allApprovals.GroupBy(a => a.HistoryScrapId)
                .ToDictionary(g => g.Key, g => g.ToList());

            foreach (var dto in historyScrapDtos)
            {
                if (detailsGrouped.TryGetValue(dto.Id, out var details))
                {
                    dto.HistoryDetails = details.Select(d =>
                    {
                        materials.TryGetValue(d.MaterialId, out var mat);
                        return new HistoryDetailScrapDto
                        {
                            Id = d.Id,
                            MaterialId = d.MaterialId,
                            Name = mat?.Name ?? string.Empty,
                            Code = mat?.Code ?? string.Empty,
                            Unit = mat?.Unit ?? string.Empty,
                            BOMHead = d.BOMHead,
                            BOMDescription = d.BOMDescription,
                            Quantity = d.Quantity,
                            Weight = d.Weight,
                            TotalWeight = d.TotalWeight,
                            TYPE = d.TYPE,
                            Plant = d.Plant,
                            ScrapHistoryId = d.ScrapHistoryId,
                            Reson = d.Reson
                        };
                    }).ToList();
                }

                if (approvalsGrouped.TryGetValue(dto.Id, out var approvals))
                {
                    dto.Approvals = approvals.Select(a =>
                    {
                        approvalUsers.TryGetValue(a.DecisionMaker, out var dm);
                        string? actionByName = null, actionByWorkDayId = null, actionByEmail = null;
                        if (a.ActionBy.HasValue && approvalUsers.TryGetValue(a.ActionBy.Value, out var ab))
                        {
                            actionByName = ab.FullName; actionByWorkDayId = ab.WorkDayId; actionByEmail = ab.Email;
                        }
                        return new RequestApprovalDto
                        {
                            Id = a.Id,
                            HistoryScrapId = a.HistoryScrapId,
                            Status = a.Status,
                            Comment = a.Comment,
                            ActionBy = a.ActionBy,
                            ActionByName = actionByName,
                            ActionByWorkDayId = actionByWorkDayId,
                            ActionByEmail = actionByEmail,
                            ActionDate = a.ActionDate,
                            Title = a.Title,
                            Step = a.Step,
                            DecisionMaker = a.DecisionMaker,
                            DecisionMakerName = dm?.FullName,
                            DecisionMakerWorkDayId = dm?.WorkDayId,
                            DecisionMakerEmail = dm?.Email
                        };
                    }).ToList();
                }
            }

            return historyScrapDtos;
        }

        // ── Không đụng vào ──────────────────────────────────────────────────────────
        public async Task ConfirmScrap(ConfirmScrapDto request)
        {
            var scrap = await _historyScrapRepository.GetByIdAsync(request.HistoryScrapId)
                ?? throw new Exception("Không tìm thấy phiếu Scrap");

            if (scrap.Status != StatusScrap.Approved)
                throw new Exception("Phiếu chưa được duyệt hoàn toàn!");

            scrap.Status = StatusScrap.Done;
            scrap.Confirmer = request.ConfirmerId;
            scrap.ConfirmDate = DateTime.Now;
            scrap.ConfirmAmount = request.ConfirmAmount;
            scrap.ConfirmationStatusType = request.ConfirmationStatusType;
            scrap.Notes = request.Notes;

            await _historyScrapRepository.UpdateAsync(scrap);
        }
    }
}
