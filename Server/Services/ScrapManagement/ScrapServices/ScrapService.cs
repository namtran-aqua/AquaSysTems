using AquaSolution.Data.Data.Entities.Admin;
using AquaSolution.Data.Data.Entities.Scraps;
using AquaSolution.Data.Entities.Scraps;
using AquaSolution.Data.Repositories;
using AquaSolution.Shared.Enum.Scrap;
using AquaSolution.Shared.ScrapManagement.Materials;
using AquaSolution.Shared.ScrapManagement.Receivers;
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
        private readonly IRepository<Section> _sectionRepository;
        private readonly IRepository<WasteItem> _wasteItemRepository;
        private readonly IRepository<ScrapReceiver> _scrapReceiverRepository;
        private readonly IRepository<ScrapFinalApprover> _scrapFinalApproverRepository;
        private readonly AquaSolution.Data.Connection.AquaDbContext _context;

        public ScrapService(IRepository<Material> materialRepository,
                IRepository<Factory> factoryRepository,
                IRepository<Department> departmentRepository,
                IRepository<User> userRepository,
                IRepository<HistoryScrap> historyScrapRepository,
                IRepository<HistoryScrapDetail> historyScrapDetailRepository,
                IRepository<FlowApprovalScrap> flowApprovalScrapRepository,
                IRepository<RequestApproval> requestApprovalRepository,
                IRepository<Weight> weightRepository,
                IRepository<Section> sectionRepository,
                IRepository<WasteItem> wasteItemRepository,
                IRepository<ScrapReceiver> scrapReceiverRepository,
                IRepository<ScrapFinalApprover> scrapFinalApproverRepository,
                AquaSolution.Data.Connection.AquaDbContext context)
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
            _sectionRepository = sectionRepository;
            _wasteItemRepository = wasteItemRepository;
            _scrapReceiverRepository = scrapReceiverRepository;
            _scrapFinalApproverRepository = scrapFinalApproverRepository;
            _context = context;
        }

        public async Task CreateScrap(HandleScrapDto createScrapDto)
        {
            var factory = await _factoryRepository.GetByIdAsync(createScrapDto.FactoryId)
                ?? throw new Exception($"Factory không tồn tại: {createScrapDto.FactoryId}");

            var department = await _departmentRepository.GetByIdAsync(createScrapDto.DepartmentId)
                ?? throw new Exception($"Department không tồn tại: {createScrapDto.DepartmentId}");

            var flowSteps = await _flowApprovalScrapRepository.Query()
                .Where(x => x.FactoryId == createScrapDto.FactoryId && x.DepartmentId == createScrapDto.DepartmentId && x.SectionId == createScrapDto.SectionId)
                .OrderBy(x => x.Step)
                .ToListAsync();

            if (!flowSteps.Any())
            {
                flowSteps = await _flowApprovalScrapRepository.Query()
                    .Where(x => x.FactoryId == createScrapDto.FactoryId && x.DepartmentId == createScrapDto.DepartmentId && x.SectionId == null)
                    .OrderBy(x => x.Step)
                    .ToListAsync();
            }

            if (!flowSteps.Any())
            {
                throw new Exception($"Không có flow approval nào được cấu hình cho FactoryId: {createScrapDto.FactoryId}, DepartmentId: {createScrapDto.DepartmentId}, SectionId: {createScrapDto.SectionId}");
            }

            var receiver = await _scrapReceiverRepository.Query()
                .FirstOrDefaultAsync(x => x.FactoryId == createScrapDto.FactoryId && x.IsActive);
            if (receiver == null)
            {
                throw new Exception($"Chưa cấu hình người nhận hàng cho Factory '{factory.Name}'. Vui lòng liên hệ quản trị viên để cấu hình trước!");
            }

            var finalApprover = await _scrapFinalApproverRepository.Query()
                .FirstOrDefaultAsync(x => x.IsActive);
            if (finalApprover == null)
            {
                throw new Exception("Chưa cấu hình người xác nhận cuối trong hệ thống. Vui lòng liên hệ quản trị viên để cấu hình trước!");
            }

            var historyScrapId = Guid.NewGuid();
            decimal totalAmount = 0;
            if (createScrapDto.HistoryDetails != null && createScrapDto.HistoryDetails.Any())
                totalAmount = createScrapDto.HistoryDetails.Sum(d => d.Quantity * d.Weight);

            var historyScrap = new HistoryScrap
            {
                Id = historyScrapId,
                Title = createScrapDto.Title,
                Status = StatusScrap.WaitingForApproval,
                Description = createScrapDto.Description,
                CreatedDate = createScrapDto.CreatedDate,
                CreatedBy = createScrapDto.CreatedById,
                FactoryId = createScrapDto.FactoryId,
                DepartmentId = createScrapDto.DepartmentId,
                SectionId = createScrapDto.SectionId,
                TotalAmount = totalAmount,
                Responsibility = createScrapDto.Responsibility,
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
                    WarehouseLocation = detail.WarehouseLocation,
                    ImageUrl = detail.ImageUrl,
                }).ToList();
                await _historyScrapDetailRepository.InsertRangeAsync(historyDetails);
            }

            var requestApprovals = new List<RequestApproval>();

            // 1. Bước Người tạo (đã hoàn thành khi tạo đơn)
            requestApprovals.Add(new RequestApproval
            {
                Id = Guid.NewGuid(),
                HistoryScrapId = historyScrapId,
                Title = "Người tạo",
                Step = 1,
                DecisionMaker = createScrapDto.CreatedById,
                Status = StatusScrap.Approved,
                Comment = "Khởi tạo phiếu",
                ActionBy = createScrapDto.CreatedById,
                ActionDate = DateTime.Now
            });

            // 2. Các bước Người duyệt phòng ban
            int currentStep = 2;
            foreach (var step in flowSteps)
            {
                requestApprovals.Add(new RequestApproval
                {
                    Id = Guid.NewGuid(),
                    HistoryScrapId = historyScrapId,
                    Title = step.Name,
                    Step = currentStep,
                    DecisionMaker = step.DecisionMaker,
                    Status = currentStep == 2 ? StatusScrap.InterView : StatusScrap.Pending,
                    Comment = null,
                    ActionBy = null,
                    ActionDate = null
                });
                currentStep++;
            }

            // 3. Bước Người nhận hàng (theo Factory)
            requestApprovals.Add(new RequestApproval
            {
                Id = Guid.NewGuid(),
                HistoryScrapId = historyScrapId,
                Title = "Người nhận hàng",
                Step = currentStep,
                DecisionMaker = receiver.UserId,
                Status = StatusScrap.Pending,
                Comment = null,
                ActionBy = null,
                ActionDate = null
            });
            currentStep++;

            // 4. Bước Người xác nhận cuối
            requestApprovals.Add(new RequestApproval
            {
                Id = Guid.NewGuid(),
                HistoryScrapId = historyScrapId,
                Title = "Người xác nhận cuối",
                Step = currentStep,
                DecisionMaker = finalApprover.UserId,
                Status = StatusScrap.Pending,
                Comment = null,
                ActionBy = null,
                ActionDate = null
            });

            await _requestApprovalRepository.InsertRangeAsync(requestApprovals);
            await _historyScrapRepository.SaveChangesAsync();
        }

        // ── Không đụng vào ──────────────────────────────────────────────────────────
        public async Task<List<HistoryScrapDto>> GetHistory()
        {
            var query = from hs in _historyScrapRepository.Query()
                        join factory in _factoryRepository.Query() on hs.FactoryId equals factory.Id
                        join department in _departmentRepository.Query() on hs.DepartmentId equals department.Id
                        join section in _sectionRepository.Query() on hs.SectionId equals section.Id into secGroup
                        from section in secGroup.DefaultIfEmpty()
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
                            SectionId = hs.SectionId,
                            SectionName = section != null ? section.Name : string.Empty,
                            Notes = hs.Notes,
                            ConfirmationStatusType = hs.ConfirmationStatusType,
                            ConfirmAmount = hs.ConfirmAmount,
                            TotalAmount = hs.TotalAmount,
                            Responsibility = hs.Responsibility,
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
                            Reson = d.Reson,
                            WarehouseLocation = d.WarehouseLocation,
                            ImageUrl = d.ImageUrl,
                            ConfirmAmount = d.ConfirmAmount,
                            ConfirmUnitType = d.ConfirmUnitType,
                            ConfirmNote = d.ConfirmNote,
                            WasteType = d.WasteType,
                            WasteCode = d.WasteCode,
                            WasteName = d.WasteName
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

            return historyScrapDtos.OrderByDescending(x => x.CreatedDate).ToList();
        }

        // ── SỬA: lấy tất cả phiếu user từng tham gia duyệt (không chỉ InterView) ──
        public async Task<List<HistoryScrapDto>> GetScrapForApproval(Guid userId, bool isAdmin)
        {
            // ── B1: Lấy RequestApproval làm bảng chính ───────────────────────────────
            // Admin → lấy tất cả (trừ Pending vì Pending = chưa đến lượt)
            // User  → chỉ lấy dòng mà DecisionMaker = userId (trừ Pending)
            var approvalQuery = _requestApprovalRepository.Query()
                .Where(a => a.Status != StatusScrap.Pending);

            if (!isAdmin)
                approvalQuery = approvalQuery.Where(a => a.DecisionMaker == userId);

            // Lấy danh sách HistoryScrapId liên quan
            var relatedScrapIds = await approvalQuery
                .Select(a => a.HistoryScrapId)
                .Distinct()
                .ToListAsync();

            if (!relatedScrapIds.Any())
                return new List<HistoryScrapDto>();

            // ── B2: Load HistoryScrap + join lookup 1 lần ────────────────────────────
            var scraps = await (
                from hs in _historyScrapRepository.Query()
                    .Where(x => relatedScrapIds.Contains(x.Id))
                join factory in _factoryRepository.Query() on hs.FactoryId equals factory.Id
                join department in _departmentRepository.Query() on hs.DepartmentId equals department.Id
                join section in _sectionRepository.Query() on hs.SectionId equals section.Id into secGroup
                from section in secGroup.DefaultIfEmpty()
                join creator in _userRepository.Query() on hs.CreatedBy equals creator.Id
                join lastUser in _userRepository.Query() on hs.LastActionBy equals lastUser.Id into lastUserGrp
                from lastUser in lastUserGrp.DefaultIfEmpty()
                join confirmer in _userRepository.Query() on hs.Confirmer equals confirmer.Id into confirmerGrp
                from confirmer in confirmerGrp.DefaultIfEmpty()
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
                    SectionId = hs.SectionId,
                    SectionName = section != null ? section.Name : string.Empty,
                    Notes = hs.Notes,
                    ConfirmationStatusType = hs.ConfirmationStatusType,
                    ConfirmAmount = hs.ConfirmAmount,
                    TotalAmount = hs.TotalAmount,
                    Responsibility = hs.Responsibility,
                    ConfirmerId = hs.Confirmer,
                    ConfirmerName = confirmer != null ? confirmer.FullName : string.Empty,
                    ConfirmedDate = hs.ConfirmDate
                }
            ).AsNoTracking().ToListAsync();

            var ids = scraps.Select(x => x.Id).ToList();

            // ── B3: Load Detail 1 lần ─────────────────────────────────────────────────
            var allDetails = await _historyScrapDetailRepository.Query()
                .Where(d => ids.Contains(d.ScrapHistoryId))
                .AsNoTracking()
                .ToListAsync();

            var materialIds = allDetails.Select(d => d.MaterialId).Distinct().ToList();
            var materials = await _materialRepository.Query()
                .Where(m => materialIds.Contains(m.Id))
                .AsNoTracking()
                .ToDictionaryAsync(m => m.Id);

            var detailsGrouped = allDetails
                .GroupBy(d => d.ScrapHistoryId)
                .ToDictionary(g => g.Key, g => g.ToList());

            // ── B4: Load tất cả RequestApproval của các phiếu này ────────────────────
            // Lấy toàn bộ approval của phiếu (không chỉ của user) để hiển thị luồng đầy đủ
            var allApprovals = await _requestApprovalRepository.Query()
                .Where(a => ids.Contains(a.HistoryScrapId))
                .OrderBy(a => a.Step)
                .AsNoTracking()
                .ToListAsync();

            // Load user liên quan 1 lần (DecisionMaker + ActionBy)
            var approvalUserIds = allApprovals
                .Select(a => a.DecisionMaker)
                .Union(allApprovals.Where(a => a.ActionBy.HasValue).Select(a => a.ActionBy!.Value))
                .Distinct()
                .ToList();

            var approvalUsers = await _userRepository.Query()
                .Where(u => approvalUserIds.Contains(u.Id))
                .AsNoTracking()
                .ToDictionaryAsync(u => u.Id, u => new { u.FullName, u.Email, u.WorkDayId });

            var approvalsGrouped = allApprovals
                .GroupBy(a => a.HistoryScrapId)
                .ToDictionary(g => g.Key, g => g.ToList());

            // ── B5: Map vào DTO ───────────────────────────────────────────────────────
            foreach (var dto in scraps)
            {
                // Detail
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
                            Reson = d.Reson,
                            WarehouseLocation = d.WarehouseLocation,
                            ImageUrl = d.ImageUrl,
                            ConfirmAmount = d.ConfirmAmount,
                            ConfirmUnitType = d.ConfirmUnitType,
                            ConfirmNote = d.ConfirmNote,
                            WasteType = d.WasteType,
                            WasteCode = d.WasteCode,
                            WasteName = d.WasteName
                        };
                    }).ToList();
                }

                // Approvals
                if (approvalsGrouped.TryGetValue(dto.Id, out var approvals))
                {
                    dto.Approvals = approvals.Select(a =>
                    {
                        approvalUsers.TryGetValue(a.DecisionMaker, out var dm);

                        string? actionByName = null, actionByWorkDayId = null, actionByEmail = null;
                        if (a.ActionBy.HasValue && approvalUsers.TryGetValue(a.ActionBy.Value, out var ab))
                        {
                            actionByName = ab.FullName;
                            actionByWorkDayId = ab.WorkDayId;
                            actionByEmail = ab.Email;
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

            return scraps.OrderByDescending(x => x.CreatedDate).ToList();
        }


        //public async Task<List<HistoryScrapDto>> GetScrapForApproval(Guid userId)
        //{
        //    // Lấy tất cả phiếu mà user này là DecisionMaker ở bất kỳ step nào
        //    // (InterView = đang chờ, Approved = đã duyệt, Rejected = đã từ chối)
        //    // Loại trừ Pending vì Pending = chưa đến lượt user này
        //    var relatedScrapIds = await _requestApprovalRepository.Query()
        //        .Where(a => a.DecisionMaker == userId
        //                 && a.Status != StatusScrap.Pending)
        //        .Select(a => a.HistoryScrapId)
        //        .Distinct()
        //        .ToListAsync();

        //    if (!relatedScrapIds.Any())
        //        return new List<HistoryScrapDto>();

        //    var query = from hs in _historyScrapRepository.Query()
        //                    .Where(x => relatedScrapIds.Contains(x.Id))
        //                join factory in _factoryRepository.Query() on hs.FactoryId equals factory.Id
        //                join department in _departmentRepository.Query() on hs.DepartmentId equals department.Id
        //                join creator in _userRepository.Query() on hs.CreatedBy equals creator.Id
        //                join lastUser in _userRepository.Query() on hs.LastActionBy equals lastUser.Id into lastUserGroup
        //                from lastUser in lastUserGroup.DefaultIfEmpty()
        //                join confirmer in _userRepository.Query() on hs.Confirmer equals confirmer.Id into confirmerGroup
        //                from confirmer in confirmerGroup.DefaultIfEmpty()
        //                select new HistoryScrapDto
        //                {
        //                    Id = hs.Id,
        //                    Title = hs.Title,
        //                    Status = hs.Status,
        //                    Description = hs.Description,
        //                    LastActionBy = hs.LastActionBy,
        //                    LastActionByName = lastUser != null ? lastUser.FullName : string.Empty,
        //                    LastActionDate = hs.LastActionDate,
        //                    CreatedDate = hs.CreatedDate,
        //                    CreatedById = hs.CreatedBy,
        //                    CreatedByName = creator.FullName,
        //                    FactoryId = hs.FactoryId,
        //                    FactoryName = factory.Name,
        //                    DepartmentId = hs.DepartmentId,
        //                    DepartmentName = department.Name,
        //                    Notes = hs.Notes,
        //                    ConfirmationStatusType = hs.ConfirmationStatusType,
        //                    ConfirmAmount = hs.ConfirmAmount,
        //                    TotalAmount = hs.TotalAmount,
        //                    ConfirmerId = hs.Confirmer,
        //                    ConfirmerName = confirmer != null ? confirmer.FullName : string.Empty,
        //                    ConfirmedDate = hs.ConfirmDate
        //                };

        //    var historyScrapDtos = await query.ToListAsync();
        //    var ids = historyScrapDtos.Select(x => x.Id).ToList();

        //    var allDetails = await _historyScrapDetailRepository.Query()
        //                          .Where(d => ids.Contains(d.ScrapHistoryId)).ToListAsync();
        //    var materialIds = allDetails.Select(d => d.MaterialId).Distinct().ToList();
        //    var materials = await _materialRepository.Query()
        //                          .Where(m => materialIds.Contains(m.Id)).ToDictionaryAsync(m => m.Id);
        //    var detailsGrouped = allDetails.GroupBy(d => d.ScrapHistoryId)
        //                                   .ToDictionary(g => g.Key, g => g.ToList());

        //    var allApprovals = await _requestApprovalRepository.Query()
        //                          .Where(a => ids.Contains(a.HistoryScrapId))
        //                          .OrderBy(a => a.Step).ToListAsync();
        //    var approvalUserIds = allApprovals.Select(a => a.DecisionMaker)
        //                          .Union(allApprovals.Where(a => a.ActionBy.HasValue).Select(a => a.ActionBy.Value))
        //                          .Distinct().ToList();
        //    var approvalUsers = await _userRepository.Query()
        //                          .Where(u => approvalUserIds.Contains(u.Id))
        //                          .ToDictionaryAsync(u => u.Id, u => new { u.FullName, u.Email, u.WorkDayId });
        //    var approvalsGrouped = allApprovals.GroupBy(a => a.HistoryScrapId)
        //                                       .ToDictionary(g => g.Key, g => g.ToList());

        //    foreach (var dto in historyScrapDtos)
        //    {
        //        if (detailsGrouped.TryGetValue(dto.Id, out var details))
        //        {
        //            dto.HistoryDetails = details.Select(d =>
        //            {
        //                materials.TryGetValue(d.MaterialId, out var mat);
        //                return new HistoryDetailScrapDto
        //                {
        //                    Id = d.Id,
        //                    MaterialId = d.MaterialId,
        //                    Name = mat?.Name ?? string.Empty,
        //                    Code = mat?.Code ?? string.Empty,
        //                    Unit = mat?.Unit ?? string.Empty,
        //                    BOMHead = d.BOMHead,
        //                    BOMDescription = d.BOMDescription,
        //                    Quantity = d.Quantity,
        //                    Weight = d.Weight,
        //                    TotalWeight = d.TotalWeight,
        //                    TYPE = d.TYPE,
        //                    Plant = d.Plant,
        //                    ScrapHistoryId = d.ScrapHistoryId,
        //                    Reson = d.Reson
        //                };
        //            }).ToList();
        //        }

        //        if (approvalsGrouped.TryGetValue(dto.Id, out var approvals))
        //        {
        //            dto.Approvals = approvals.Select(a =>
        //            {
        //                approvalUsers.TryGetValue(a.DecisionMaker, out var decisionMakerInfo);
        //                string? actionByName = null, actionByWorkDayId = null, actionByEmail = null;
        //                if (a.ActionBy.HasValue && approvalUsers.TryGetValue(a.ActionBy.Value, out var actionByInfo))
        //                {
        //                    actionByName = actionByInfo.FullName;
        //                    actionByWorkDayId = actionByInfo.WorkDayId;
        //                    actionByEmail = actionByInfo.Email;
        //                }
        //                return new RequestApprovalDto
        //                {
        //                    Id = a.Id,
        //                    HistoryScrapId = a.HistoryScrapId,
        //                    Status = a.Status,
        //                    Comment = a.Comment,
        //                    ActionBy = a.ActionBy,
        //                    ActionByName = actionByName,
        //                    ActionByWorkDayId = actionByWorkDayId,
        //                    ActionByEmail = actionByEmail,
        //                    ActionDate = a.ActionDate,
        //                    Title = a.Title,
        //                    Step = a.Step,
        //                    DecisionMaker = a.DecisionMaker,
        //                    DecisionMakerName = decisionMakerInfo?.FullName,
        //                    DecisionMakerWorkDayId = decisionMakerInfo?.WorkDayId,
        //                    DecisionMakerEmail = decisionMakerInfo?.Email
        //                };
        //            }).ToList();
        //        }
        //    }

        //    return historyScrapDtos;
        //}

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

            var now = DateTime.Now;

            // Ghi nhận action trên step hiện tại
            currentStep.ActionBy = request.ActionBy;
            currentStep.ActionDate = now;
            currentStep.Comment = request.Comment;

            if (!request.IsApproved)
            {
                // Reject: Ghi nhận người duyệt đã từ chối (bước của người reject có status là Rejected)
                currentStep.Status = StatusScrap.Rejected;
                currentStep.ActionBy = request.ActionBy;
                currentStep.ActionDate = now;
                currentStep.Comment = request.Comment;

                // Trạng thái của đơn là Rejected
                scrap.Status = StatusScrap.Rejected;

                var receiverStep = approvals.FirstOrDefault(a => a.Title == "Người nhận hàng");
                bool isApproverStep = currentStep.Step > 1 && (receiverStep == null || currentStep.Step < receiverStep.Step);
                bool isFinalApproverStep = receiverStep != null && currentStep.Step > receiverStep.Step;

                if (isApproverStep)
                {
                    // Đối với Các bước Người duyệt, miễn là bị reject, đều cần quay lại bước người tạo để sửa
                    var step1 = approvals.FirstOrDefault(a => a.Step == 1);
                    if (step1 != null)
                    {
                        step1.Status = StatusScrap.InterView;
                        await _requestApprovalRepository.UpdateAsync(step1);
                    }

                    // Reset tất cả các bước Người duyệt từ Step 2 đến trước currentStep về Pending
                    foreach (var s in approvals.Where(a => a.Step > 1 && a.Step < currentStep.Step))
                    {
                        s.Status = StatusScrap.Pending;
                        s.Comment = null;
                        s.ActionBy = null;
                        s.ActionDate = null;
                        await _requestApprovalRepository.UpdateAsync(s);
                    }
                }
                else if (isFinalApproverStep)
                {
                    // Người xác nhận cuối reject: quay về bước Người nhận hàng làm tiếp
                    if (receiverStep != null)
                    {
                        receiverStep.Status = StatusScrap.InterView;
                        await _requestApprovalRepository.UpdateAsync(receiverStep);
                    }
                }
                else
                {
                    var prevStep = approvals.FirstOrDefault(a => a.Step == currentStep.Step - 1);
                    if (prevStep != null)
                    {
                        prevStep.Status = StatusScrap.InterView;
                        await _requestApprovalRepository.UpdateAsync(prevStep);
                    }
                }

                scrap.LastActionBy = request.ActionBy;
                scrap.LastActionDate = now;
                await _requestApprovalRepository.UpdateAsync(currentStep);
            }
            else
            {
                currentStep.Status = StatusScrap.Approved;

                var nextStep = approvals.FirstOrDefault(a => a.Step == currentStep.Step + 1);

                if (nextStep != null)
                {
                    nextStep.Status = StatusScrap.InterView;

                    var receiverStep = approvals.FirstOrDefault(a => a.Title == "Người nhận hàng");
                    if (receiverStep != null && nextStep.Step == receiverStep.Step)
                    {
                        // Tất cả Người duyệt đã duyệt xong -> chuyển sang bước Người nhận hàng: Waiting for Confirmation
                        scrap.Status = StatusScrap.WaitingForConfirmation;
                    }
                    else
                    {
                        // Vẫn trong các bước Người duyệt: Waiting for Approval
                        scrap.Status = StatusScrap.WaitingForApproval;
                    }

                    scrap.LastActionBy = request.ActionBy;
                    scrap.LastActionDate = now;

                    await _requestApprovalRepository.UpdateAsync(currentStep);
                    await _requestApprovalRepository.UpdateAsync(nextStep);
                }
                else
                {
                    // Đây là step cuối cùng (Người xác nhận cuối duyệt xong) → phiếu Hoàn thành
                    scrap.Status = StatusScrap.Done;
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
                            .Where(x => x.Status == StatusScrap.WaitingForConfirmation
                                     || x.Status == StatusScrap.WaitingForApproval
                                     || x.Status == StatusScrap.Approved
                                     || x.Status == StatusScrap.Done
                                     || x.Status == StatusScrap.Rejected)
                        join factory in _factoryRepository.Query() on hs.FactoryId equals factory.Id
                        join department in _departmentRepository.Query() on hs.DepartmentId equals department.Id
                        join section in _sectionRepository.Query() on hs.SectionId equals section.Id into secGroup
                        from section in secGroup.DefaultIfEmpty()
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
                            SectionId = hs.SectionId,
                            SectionName = section != null ? section.Name : string.Empty,
                            Notes = hs.Notes,
                            ConfirmationStatusType = hs.ConfirmationStatusType,
                            ConfirmAmount = hs.ConfirmAmount,
                            TotalAmount = hs.TotalAmount,
                            Responsibility = hs.Responsibility,
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
                            Reson = d.Reson,
                            WarehouseLocation = d.WarehouseLocation,
                            ImageUrl = d.ImageUrl,
                            // Confirm fields
                            ConfirmAmount = d.ConfirmAmount,
                            ConfirmUnitType = d.ConfirmUnitType,
                            ConfirmNote = d.ConfirmNote,
                            WasteType = d.WasteType,
                            WasteCode = d.WasteCode,
                            WasteName = d.WasteName
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

            return historyScrapDtos.OrderByDescending(x => x.CreatedDate).ToList();
        }

        public async Task ConfirmScrap(ActionConfirmScrapDto request)
        {
            var scrap = await _historyScrapRepository.GetByIdAsync(request.HistoryScrapId)
                ?? throw new Exception("Không tìm thấy phiếu Scrap");

            if (scrap.Status != StatusScrap.WaitingForConfirmation && scrap.Status != StatusScrap.Approved && scrap.Status != StatusScrap.Rejected)
                throw new Exception("Phiếu chưa được duyệt hoàn toàn hoặc không ở trạng thái chờ nhận hàng!");

            // Lấy tất cả details của phiếu này để kiểm tra bảo mật
            var existingDetails = await _historyScrapDetailRepository.Query()
                .Where(d => d.ScrapHistoryId == request.HistoryScrapId)
                .ToListAsync();

            var existingDetailIds = existingDetails.Select(d => d.Id).ToHashSet();

            // Validate
            foreach (var detailDto in request.Details)
            {
                if (!existingDetailIds.Contains(detailDto.HistoryDetailId))
                {
                    throw new Exception($"Lỗi bảo mật: Mã chi tiết {detailDto.HistoryDetailId} không thuộc phiếu Scrap này!");
                }

                if (detailDto.ConfirmUnitType == ConfirmUnitType.Unit)
                {
                    if (detailDto.ConfirmAmount % 1 != 0)
                    {
                        throw new Exception($"Dữ liệu không hợp lệ: Nhận theo số lượng (Cái) không được nhập số thập phân ({detailDto.ConfirmAmount}).");
                    }
                }
            }

            // Ghi nhận confirm từng detail
            foreach (var detailDto in request.Details)
            {
                var detailToUpdate = existingDetails.First(d => d.Id == detailDto.HistoryDetailId);
                detailToUpdate.ConfirmAmount    = detailDto.ConfirmAmount;
                detailToUpdate.ConfirmUnitType  = detailDto.ConfirmUnitType;
                detailToUpdate.ConfirmNote      = detailDto.ConfirmNote;
                detailToUpdate.WasteType        = detailDto.WasteType;
                detailToUpdate.WasteCode        = detailDto.WasteCode;
                detailToUpdate.WasteName        = detailDto.WasteName;
                _context.Set<HistoryScrapDetail>().Update(detailToUpdate);
            }

            // Tính ConfirmAmount header (quy về kg)
            decimal totalConfirmKg = existingDetails.Sum(d =>
            {
                var dto = request.Details.FirstOrDefault(x => x.HistoryDetailId == d.Id);
                if (dto == null) return 0m;

                if (dto.ConfirmUnitType == ConfirmUnitType.Weight)
                    return dto.ConfirmAmount;

                return d.Weight > 0 ? dto.ConfirmAmount * d.Weight : 0m;
            });

            var totalRequest = scrap.TotalAmount ?? 0;
            var tolerance    = 0.001m;
            var confirmStatus = Math.Abs(totalConfirmKg - totalRequest) <= tolerance
                ? ConfirmationStatusType.Received
                : totalConfirmKg > totalRequest
                    ? ConfirmationStatusType.Overreceived
                    : ConfirmationStatusType.Underreceived;

            // Cập nhật bước Người nhận hàng trong RequestApproval
            var approvals = await _requestApprovalRepository.Query()
                .Where(a => a.HistoryScrapId == request.HistoryScrapId)
                .OrderBy(a => a.Step)
                .ToListAsync();

            var receiverStep = approvals.FirstOrDefault(a => a.Title == "Người nhận hàng")
                            ?? approvals.FirstOrDefault(a => a.Status == StatusScrap.InterView);

            if (receiverStep != null)
            {
                receiverStep.Status = StatusScrap.Approved;
                receiverStep.ActionBy = request.ConfirmerId;
                receiverStep.ActionDate = DateTime.Now;
                receiverStep.Comment = "Đã xác nhận nhận hàng";
                _context.Set<RequestApproval>().Update(receiverStep);

                // Chuyển sang bước Người xác nhận cuối
                var nextStep = approvals.FirstOrDefault(a => a.Step == receiverStep.Step + 1);
                if (nextStep != null)
                {
                    nextStep.Status = StatusScrap.InterView;
                    _context.Set<RequestApproval>().Update(nextStep);
                }
            }

            // Cập nhật header scrap
            scrap.Confirmer               = request.ConfirmerId;
            scrap.ConfirmDate             = DateTime.Now;
            scrap.ConfirmAmount           = totalConfirmKg;
            scrap.ConfirmationStatusType  = confirmStatus;
            scrap.Status                  = StatusScrap.WaitingForApproval;
            scrap.LastActionBy            = request.ConfirmerId;
            scrap.LastActionDate          = DateTime.Now;

            _context.Set<HistoryScrap>().Update(scrap);
            await _context.SaveChangesAsync();
        }

        public async Task ResubmitScrap(HandleScrapDto resubmitDto)
        {
            var scrap = await _historyScrapRepository.GetByIdAsync(resubmitDto.Id)
                ?? throw new Exception("Không tìm thấy phiếu Scrap");

            var approvals = await _requestApprovalRepository.Query()
                .Where(a => a.HistoryScrapId == resubmitDto.Id)
                .OrderBy(a => a.Step)
                .ToListAsync();

            var step1 = approvals.FirstOrDefault(a => a.Step == 1);
            if (step1 == null || step1.Status != StatusScrap.InterView)
            {
                throw new Exception("Phiếu không ở trạng thái cho phép người tạo chỉnh sửa và nộp lại!");
            }

            decimal totalAmount = 0;
            if (resubmitDto.HistoryDetails != null && resubmitDto.HistoryDetails.Any())
                totalAmount = resubmitDto.HistoryDetails.Sum(d => d.Quantity * d.Weight);

            scrap.Description = resubmitDto.Description;
            scrap.Responsibility = resubmitDto.Responsibility;
            scrap.SectionId = resubmitDto.SectionId;
            scrap.TotalAmount = totalAmount;
            scrap.Status = StatusScrap.WaitingForApproval;
            scrap.LastActionBy = resubmitDto.CreatedById;
            scrap.LastActionDate = DateTime.Now;

            var oldDetails = await _historyScrapDetailRepository.Query()
                .Where(d => d.ScrapHistoryId == resubmitDto.Id)
                .ToListAsync();
            _context.Set<HistoryScrapDetail>().RemoveRange(oldDetails);

            if (resubmitDto.HistoryDetails != null)
            {
                var newDetails = resubmitDto.HistoryDetails.Select(detail => new HistoryScrapDetail
                {
                    Id = Guid.NewGuid(),
                    MaterialId = detail.MaterialId,
                    ScrapHistoryId = resubmitDto.Id,
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
                    WarehouseLocation = detail.WarehouseLocation,
                    ImageUrl = detail.ImageUrl,
                }).ToList();
                await _historyScrapDetailRepository.InsertRangeAsync(newDetails);
            }

            // Step 1 trở lại Approved
            step1.Status = StatusScrap.Approved;
            step1.ActionBy = resubmitDto.CreatedById;
            step1.ActionDate = DateTime.Now;
            step1.Comment = "Đã cập nhật thông tin và nộp lại";
            _context.Set<RequestApproval>().Update(step1);

            // Step 2 trở lại InterView
            var step2 = approvals.FirstOrDefault(a => a.Step == 2);
            if (step2 != null)
            {
                step2.Status = StatusScrap.InterView;
                step2.Comment = null;
                step2.ActionBy = null;
                step2.ActionDate = null;
                _context.Set<RequestApproval>().Update(step2);
            }

            // Reset các bước sau Step 2 về Pending
            foreach (var s in approvals.Where(a => a.Step > 2))
            {
                s.Status = StatusScrap.Pending;
                s.Comment = null;
                s.ActionBy = null;
                s.ActionDate = null;
                _context.Set<RequestApproval>().Update(s);
            }

            _context.Set<HistoryScrap>().Update(scrap);
            await _context.SaveChangesAsync();
        }

        public async Task<List<RequestApprovalDto>> PreviewApprovalFlow(Guid factoryId, Guid departmentId, Guid? sectionId, Guid creatorId)
        {
            var list = new List<RequestApprovalDto>();

            // 1. Creator
            var creator = await _userRepository.GetByIdAsync(creatorId);
            list.Add(new RequestApprovalDto
            {
                Step = 1,
                Title = "Người tạo",
                DecisionMaker = creatorId,
                DecisionMakerName = creator?.FullName,
                DecisionMakerWorkDayId = creator?.WorkDayId,
                DecisionMakerEmail = creator?.Email,
                Status = StatusScrap.InterView
            });

            // 2. Flow Approvers
            var flowSteps = await _flowApprovalScrapRepository.Query()
                .Where(x => x.FactoryId == factoryId && x.DepartmentId == departmentId && x.SectionId == sectionId)
                .OrderBy(x => x.Step)
                .ToListAsync();

            if (!flowSteps.Any())
            {
                flowSteps = await _flowApprovalScrapRepository.Query()
                    .Where(x => x.FactoryId == factoryId && x.DepartmentId == departmentId && x.SectionId == null)
                    .OrderBy(x => x.Step)
                    .ToListAsync();
            }

            var approverIds = flowSteps.Select(x => x.DecisionMaker).Distinct().ToList();
            var approverUsers = await _userRepository.Query()
                .Where(u => approverIds.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id);

            int stepIndex = 2;
            foreach (var fs in flowSteps)
            {
                approverUsers.TryGetValue(fs.DecisionMaker, out var u);
                list.Add(new RequestApprovalDto
                {
                    Step = stepIndex,
                    Title = fs.Name,
                    DecisionMaker = fs.DecisionMaker,
                    DecisionMakerName = u?.FullName,
                    DecisionMakerWorkDayId = u?.WorkDayId,
                    DecisionMakerEmail = u?.Email,
                    Status = StatusScrap.Pending
                });
                stepIndex++;
            }

            // 3. Receiver
            var receiver = await _scrapReceiverRepository.Query()
                .FirstOrDefaultAsync(x => x.FactoryId == factoryId && x.IsActive);
            User? receiverUser = null;
            if (receiver != null)
            {
                receiverUser = await _userRepository.GetByIdAsync(receiver.UserId);
            }
            list.Add(new RequestApprovalDto
            {
                Step = stepIndex,
                Title = "Người nhận hàng",
                DecisionMaker = receiver?.UserId ?? Guid.Empty,
                DecisionMakerName = receiverUser?.FullName ?? (receiver == null ? "(Chưa cấu hình)" : ""),
                DecisionMakerWorkDayId = receiverUser?.WorkDayId,
                DecisionMakerEmail = receiverUser?.Email,
                Status = StatusScrap.Pending
            });
            stepIndex++;

            // 4. Final Approver
            var finalApprover = await _scrapFinalApproverRepository.Query()
                .FirstOrDefaultAsync(x => x.IsActive);
            User? finalUser = null;
            if (finalApprover != null)
            {
                finalUser = await _userRepository.GetByIdAsync(finalApprover.UserId);
            }
            list.Add(new RequestApprovalDto
            {
                Step = stepIndex,
                Title = "Người xác nhận cuối",
                DecisionMaker = finalApprover?.UserId ?? Guid.Empty,
                DecisionMakerName = finalUser?.FullName ?? (finalApprover == null ? "(Chưa cấu hình)" : ""),
                DecisionMakerWorkDayId = finalUser?.WorkDayId,
                DecisionMakerEmail = finalUser?.Email,
                Status = StatusScrap.Pending
            });

            return list;
        }

        public async Task<List<ScrapReceiverDto>> GetScrapReceivers()
        {
            var query = from r in _scrapReceiverRepository.Query()
                        join f in _factoryRepository.Query() on r.FactoryId equals f.Id
                        join u in _userRepository.Query() on r.UserId equals u.Id
                        orderby f.Name
                        select new ScrapReceiverDto
                        {
                            Id = r.Id,
                            FactoryId = r.FactoryId,
                            FactoryName = f.Name,
                            UserId = r.UserId,
                            UserWorkDayId = u.WorkDayId,
                            UserFullName = u.FullName,
                            UserEmail = u.Email,
                            Note = r.Note,
                            IsActive = r.IsActive,
                            CreatedDate = r.CreatedDate
                        };
            return await query.AsNoTracking().ToListAsync();
        }

        public async Task SaveScrapReceiver(SaveScrapReceiverDto dto)
        {
            var existingOther = await _scrapReceiverRepository.Query()
                .FirstOrDefaultAsync(x => x.FactoryId == dto.FactoryId && (dto.Id == null || x.Id != dto.Id.Value) && x.IsActive);

            if (existingOther != null)
            {
                existingOther.IsActive = false;
                existingOther.UpdatedDate = DateTime.Now;
                _context.Set<ScrapReceiver>().Update(existingOther);
            }

            if (dto.Id.HasValue && dto.Id.Value != Guid.Empty)
            {
                var item = await _scrapReceiverRepository.GetByIdAsync(dto.Id.Value)
                    ?? throw new Exception("Không tìm thấy cấu hình người nhận hàng");
                item.FactoryId = dto.FactoryId;
                item.UserId = dto.UserId;
                item.Note = dto.Note;
                item.IsActive = dto.IsActive;
                item.UpdatedDate = DateTime.Now;
                _context.Set<ScrapReceiver>().Update(item);
            }
            else
            {
                var item = new ScrapReceiver
                {
                    Id = Guid.NewGuid(),
                    FactoryId = dto.FactoryId,
                    UserId = dto.UserId,
                    Note = dto.Note,
                    IsActive = dto.IsActive,
                    CreatedDate = DateTime.Now
                };
                await _scrapReceiverRepository.InsertAsync(item);
            }
            await _context.SaveChangesAsync();
        }

        public async Task DeleteScrapReceiver(Guid id)
        {
            var item = await _scrapReceiverRepository.GetByIdAsync(id)
                ?? throw new Exception("Không tìm thấy cấu hình");
            await _scrapReceiverRepository.DeleteAsync(item);
        }

        public async Task<List<WasteItemDto>> GetAllWasteItemsAsync()
        {
            return await _wasteItemRepository.Query()
                .Where(x => x.IsActive)
                .OrderBy(x => x.OrderIndex)
                .ThenBy(x => x.Code)
                .Select(x => new WasteItemDto
                {
                    Id = x.Id,
                    Code = x.Code,
                    Name = x.Name,
                    Category = x.Category,
                    OrderIndex = x.OrderIndex
                })
                .AsNoTracking()
                .ToListAsync();
        }
    }
}
