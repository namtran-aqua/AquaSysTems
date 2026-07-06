using AquaSolution.Data.Connection;
using AquaSolution.Data.Data.Entities;
using AquaSolution.Data.Data.Entities.Admin;
using AquaSolution.Data.Data.Entities.KPI;
using AquaSolution.Data.KPI.Entities;
using AquaSolution.Data.Repositories;
using AquaSolution.Shared.Enum;
using AquaSolution.Shared.Enum.KPIType;
using AquaSolution.Shared.KPI.IndexWeight;
using AquaSolution.Shared.KPI.KPIActual;
using AquaSolution.Shared.KPI.KPISubmit;
using AquaSolution.Shared.KPI.Result;
using AquaSolution.Shared.KPI.UserTask;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using ZstdSharp;

namespace AquaSolution.Server.Services.KPI.KPISubmit
{
    public class KPISubmitService : IKPISubmitService
    {
        private readonly IRepository<UserTask> _userTaskepo;
        private readonly IRepository<KPITask> _kpiTaskRepo;
        private readonly IRepository<KPIMonthlyTarget> _kpiTargeRepo;
        private readonly IRepository<KPIMonthlyActual> _KPIMonthlyActualRepo;
        private readonly IRepository<User> _userRepo;
        private readonly IRepository<Formula> _formulaRepo;
        private readonly IRepository<KPITotalScore> _kpiTotalScoreRepo;
        private readonly IRepository<KPIIndexWeight> _kpiIndexWeightRepo;
        private readonly IRepository<Position> _positionRepo;
        private readonly IRepository<KPIActualMaster> _kpiActualMasterRepo;
        private readonly IRepository<KPIRequest> _kpiRequestRepo;
        private readonly IRepository<KPIDetailScore> _kpiDetailScoreRepo;
        private readonly IRepository<RequestApprovalTask> _requestApprovalTaskRepo;
        private readonly IRepository<ApprovalFlow> _approvalFlowRepo;
        private readonly IRepository<Department> _departmentRepo;
        private readonly IRepository<Factory> _factoryRepo;
        private readonly IRepository<QuarterCalculate> _quarterCalculateRepo;

        private readonly AquaDbContext _context;
        public KPISubmitService(IRepository<UserTask> userTaskepo,
            IRepository<KPITask> kpiTaskepo,
            IRepository<KPIMonthlyTarget> kpiTargeRepo,
            IRepository<KPIMonthlyActual> KPIMonthlyActualRepo,
            IRepository<User> userRepo,
            IRepository<Formula> formulaRepo,
            IRepository<KPITotalScore> kpiTotalScoreRepo,
            IRepository<KPIIndexWeight> kpiIndexWeightRepo,
            IRepository<Position> positionRepo,
            IRepository<KPIActualMaster> kpiActualMasterRepo,
            IRepository<KPIRequest> kpiRequestRepo,
            IRepository<KPIDetailScore> kpiDetailScoreRepo,
            IRepository<RequestApprovalTask> requestApprovalTaskRepo,
            IRepository<ApprovalFlow> approvalFlowRepo,
            IRepository<Department> departmentRepo,
            IRepository<Factory> factoryRepo,
            IRepository<QuarterCalculate> quarterCalculateRepo,
        AquaDbContext context)
        {
            _userTaskepo = userTaskepo;
            _kpiTaskRepo = kpiTaskepo;
            _kpiTargeRepo = kpiTargeRepo;
            _KPIMonthlyActualRepo = KPIMonthlyActualRepo;
            _userRepo = userRepo;
            _formulaRepo = formulaRepo;
            _kpiTotalScoreRepo = kpiTotalScoreRepo;
            _kpiIndexWeightRepo = kpiIndexWeightRepo;
            _positionRepo = positionRepo;
            _kpiActualMasterRepo = kpiActualMasterRepo;
            _kpiRequestRepo = kpiRequestRepo;
            _kpiDetailScoreRepo = kpiDetailScoreRepo;
            _context = context;
            _requestApprovalTaskRepo = requestApprovalTaskRepo;
            _approvalFlowRepo = approvalFlowRepo;
            _departmentRepo = departmentRepo;
            _factoryRepo = factoryRepo;
            _quarterCalculateRepo = quarterCalculateRepo;
        }
        public async Task<List<HandleActualDto>> GetHandleKPISubmitByUserId(Guid userId, int year, int? month)
        {

            if (!month.HasValue)
            {
                return new List<HandleActualDto>();
            }

            try
            {
                var userTasks = await _userTaskepo.GetListAsync(ut => ut.UserId == userId && ut.IsActive);
                var userTaskIds = userTasks.Select(ut => ut.Id).ToList();
                var kpiTaskIds = userTasks.Select(ut => ut.KPITaskId).Distinct().ToList();
                var kpiIndexWeights = await _kpiIndexWeightRepo.GetAllAsync();
                var kpiTasks = await _kpiTaskRepo.GetListAsync(kt => kpiTaskIds.Contains(kt.Id));
                var positionIds = await _positionRepo.GetAllAsync();
                var targets = await _kpiTargeRepo.GetListAsync(t =>
                    userTaskIds.Contains(t.UserTaskId)
                    && t.Year == year
                    && t.Month.HasValue
                    && t.Month.Value == month.Value
                );
                var targetIds = targets.Select(t => t.Id).ToList();
                //var actuals = await _KPIMonthlyActualRepo.GetListAsync(a => targetIds.Contains(a.KPIMonthlyTargetId));
                //var actualTargetIds = actuals.Select(a => a.KPIMonthlyTargetId).ToHashSet();
                var users = await _userRepo.GetAllAsync();
                var formulas = await _formulaRepo.GetAllAsync();
                var distinctIndexWeights = kpiIndexWeights
                        .GroupBy(iw => new { iw.PositionType, iw.KPIIndexType })
                        .Select(g => g.First(x => x.PeriodType == PeriodType.Month))
                        .ToList();
                var result = (from target in targets
                              join userTask in userTasks on target.UserTaskId equals userTask.Id
                              join user in users on userTask.UserId equals user.Id
                              join kpiTask in kpiTasks on userTask.KPITaskId equals kpiTask.Id
                              join quarterCalculate in await _quarterCalculateRepo.GetQueryableAsync()
                                 on kpiTask.CalculatedId equals quarterCalculate.Id
                              join formula in formulas on kpiTask.FormulaId equals formula.Id
                              join position in positionIds on user.PositionId equals position.Id
                              join indexWeight in distinctIndexWeights
                                 on new { posType = position.Type, idxType = kpiTask.KPIIndexType }
                                 equals new
                                 {
                                     posType = indexWeight.PositionType,
                                     idxType = indexWeight.KPIIndexType
                                 }
                                 //where !actualTargetIds.Contains(target.Id)
                              select new HandleActualDto
                              {
                                  TaskId = userTask.KPITaskId,
                                  Month = target.Month,
                                  Year = target.Year,
                                  ActualValue = null,
                                  Index = userTask.Index,
                                  CreatedBy = userTask.UserId,
                                  UserName = users.FirstOrDefault(u => u.Id == userTask.UserId)?.FullName ?? string.Empty,
                                  CreatedDate = userTask.CreatedDate,
                                  TaskName = kpiTask.TaskName,
                                  KPIIndexType = kpiTask.KPIIndexType,
                                  KPICategory = kpiTask.KPICategory,
                                  Max = kpiTask.Max,
                                  Bottom = kpiTask.Bottom,
                                  Weight = userTask.Weight,
                                  Unit = kpiTask.Unit,
                                  IndexWeight = indexWeight.Weight,
                                  KPIFormulaType = formula.KPIFormulaType,
                                  PIC = kpiTask.PIC,
                                  DataSource = kpiTask.DataSource,
                                  Formula = formulas.FirstOrDefault(f => f.Id == kpiTask.FormulaId)?.FormulaName ?? string.Empty,
                                  TargetValue = target.TargetValue,
                                  TargetId = target.Id,
                                  Description = kpiTask.TaskDescription,
                                  CalculateMethod = kpiTask.CalculatedMdethod,
                                  calculated = quarterCalculate.QuarterCalculated,
                                  QuarterCalculateType = quarterCalculate.QuarterCalculateType,
                              }).OrderBy(x => x.Index).ToList();

                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[KPI Submit Error] {ex.Message}");
                throw;
            }
        }
        public async Task<List<YearDto>> GetKPIScoreByUserId(Guid userId, int year)
        {
            try
            {
                var userTasks = await _userTaskepo.GetListAsync(ut => ut.UserId == userId && ut.IsActive);
                var totalScores = await _kpiTotalScoreRepo.GetAllAsync();
                var userTaskIds = userTasks.Select(ut => ut.Id).ToList();
                var kpiTaskIds = userTasks.Select(ut => ut.KPITaskId).Distinct().ToList();

                var kpiIndexWeights = await _kpiIndexWeightRepo.GetAllAsync();
                var kpiTasks = await _kpiTaskRepo.GetListAsync(kt => kpiTaskIds.Contains(kt.Id));
                var positionIds = await _positionRepo.GetAllAsync();
                var targets = await _kpiTargeRepo.GetListAsync(t =>
                    userTaskIds.Contains(t.UserTaskId)
                    && t.Year == year
                    && t.Month.HasValue
                );

                var targetIds = targets.Select(t => t.Id).ToList();
                var actuals = await _KPIMonthlyActualRepo.GetListAsync(a => targetIds.Contains(a.KPITotalScoreId));
                var actualTargetIds = actuals.Select(a => a.KPITargetId).ToHashSet();

                var users = await _userRepo.GetAllAsync();
                var formulas = await _formulaRepo.GetAllAsync();

                var distinctIndexWeights = kpiIndexWeights
                    .GroupBy(iw => new { iw.PositionType, iw.KPIIndexType })
                    .Select(g => g.First())
                    .ToList();

                // Điểm tổng từ KPIActualMaster
                var masterScores = await _kpiActualMasterRepo.GetListAsync(m => m.UserId == userId && m.Year == year);

                var result = (from target in targets
                              join userTask in userTasks on target.UserTaskId equals userTask.Id
                              join user in users on userTask.UserId equals user.Id
                              join kpiTask in kpiTasks on userTask.KPITaskId equals kpiTask.Id
                              join formula in formulas on kpiTask.FormulaId equals formula.Id
                              join position in positionIds on user.PositionId equals position.Id
                              join indexWeight in distinctIndexWeights
                                  on new { posType = position.Type, idxType = kpiTask.KPIIndexType }
                                  equals new { posType = indexWeight.PositionType, idxType = indexWeight.KPIIndexType }
                              join actual in actuals on target.Id equals actual.KPITargetId
                              join totalScore in totalScores on actual.KPITotalScoreId equals totalScore.Id
                              where totalScore.Status == StatusKPIRequestType.Approved
                              select new HandleActualDto
                              {
                                  TaskId = userTask.Id,
                                  Month = target.Month,
                                  Year = target.Year,
                                  ActualValue = actual.ActualValue,
                                  Index = userTask.Index,
                                  CreatedBy = userTask.UserId,
                                  UserName = users.FirstOrDefault(u => u.Id == userTask.UserId)?.FullName ?? string.Empty,
                                  CreatedDate = userTask.CreatedDate,
                                  TaskName = kpiTask.TaskName,
                                  KPIIndexType = kpiTask.KPIIndexType,
                                  KPICategory = kpiTask.KPICategory,
                                  Max = kpiTask.Max,
                                  Bottom = kpiTask.Bottom,
                                  Weight = userTask.Weight,
                                  Unit = kpiTask.Unit,
                                  IndexWeight = indexWeight.Weight,
                                  KPIFormulaType = formula.KPIFormulaType,
                                  PIC = kpiTask.PIC,
                                  DataSource = kpiTask.DataSource,
                                  Formula = formula.FormulaName,
                                  TargetValue = target.TargetValue,
                                  KeyTaskScore = totalScore.KeyTaskScore,
                                  KPIScore = totalScore.KPIScore,
                                  OMGScore = totalScore.OMGScore,
                                  //TotaleScore = totalScore.TotaleScore,
                              }).OrderBy(x => x.Year).ThenBy(x => x.HalfYear).ThenBy(x => x.Quarter).ThenBy(x => x.Month).ToList();

                // Gộp dữ liệu thành tree
                var tree = result
                    .GroupBy(x => x.Year)
                    .Select(gYear =>
                    {
                        var yearKey = gYear.Key;
                        var yearMaster = masterScores.FirstOrDefault(m => m.Year == yearKey && m.HaflYear == null && m.Quarter == null);

                        return new YearDto
                        {
                            Year = yearKey,
                            KPIScore = yearMaster?.KPIScore ?? 0,
                            KeyTaskScore = yearMaster?.KeyTaskScore ?? 0,
                            OMGScore = yearMaster?.OMGScore ?? 0,
                            TotalScore = yearMaster?.TotaleScore ?? 0,
                            HalfYears = gYear
                                .GroupBy(x => x.HalfYear ?? 0)
                                .Select(gHalf =>
                                {
                                    var halfKey = gHalf.Key;
                                    var halfMaster = masterScores.FirstOrDefault(m => m.Year == yearKey && m.HaflYear == halfKey && m.Quarter == null);

                                    var quarters = gHalf
                                        .GroupBy(x => x.Quarter ?? 0)
                                        .Select(gQuarter =>
                                        {
                                            var quarterKey = gQuarter.Key;
                                            var quarterMaster = masterScores.FirstOrDefault(m => m.Year == yearKey && m.Quarter == quarterKey);

                                            return new QuarterDto
                                            {
                                                Quarter = quarterKey,
                                                KPIScore = quarterMaster?.KPIScore ?? 0,
                                                KeyTaskScore = quarterMaster?.KeyTaskScore ?? 0,
                                                OMGScore = quarterMaster?.OMGScore ?? 0,
                                                TotalScore = quarterMaster?.TotaleScore ?? 0,
                                                Months = gQuarter.ToList()
                                            };
                                        }).ToList();

                                    return new HalfYearDto
                                    {
                                        HalfYear = halfKey,
                                        KPIScore = halfMaster?.KPIScore ?? 0,
                                        KeyTaskScore = halfMaster?.KeyTaskScore ?? 0,
                                        OMGScore = halfMaster?.OMGScore ?? 0,
                                        TotalScore = halfMaster?.TotaleScore ?? 0,
                                        Quarters = quarters
                                    };
                                }).ToList()
                        };
                    }).ToList();

                return tree;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[KPI Tree Error] {ex.Message}");
                throw;
            }
        }
        public async Task<List<IndexWeightDto>> GetIndexWeight(PositionType positionType, PeriodType periodType)
        {
            var query = from indexWeight in await _kpiIndexWeightRepo.GetQueryableAsync()
                        join position in await _positionRepo.GetQueryableAsync()
                        on indexWeight.PositionType equals position.Type
                        where indexWeight.PositionType == positionType && indexWeight.PeriodType == periodType
                        select new IndexWeightDto
                        {
                            Weight = indexWeight.Weight,
                            PeriodType = indexWeight.PeriodType,
                            PositionType = indexWeight.PositionType,
                            KPIIndexType = indexWeight.KPIIndexType
                        };
            return query.ToList();
        }

        #region SUBMIT KPI
        public async Task<bool> SubmitKPIAsync(HandleKPISubmitDto submitKPIDto, int month)
        {
            int quarter = (month + 2) / 3;
            int halfYear = month <= 6 ? 1 : 2;

            var submitId = Guid.NewGuid();
            var userId = submitKPIDto.KPITotalScore.FirstOrDefault()?.CreatedBy ?? Guid.Empty;
            var totalScores = new List<KPITotalScore>();

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                await InsertKPIRequestAsync(submitKPIDto, submitId);
                await InsertMonthlyAsync(submitKPIDto, submitId, totalScores, month);
                await InsertQuarterAsync(submitKPIDto, submitId, totalScores, quarter);
                await InsertHalfYearAsync(submitKPIDto, submitId, totalScores, halfYear);
                await InsertDetailAndActualAsync(submitKPIDto, totalScores, month, userId);
                await CreatedRequest(submitId, totalScores);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        private async Task InsertKPIRequestAsync(HandleKPISubmitDto dto, Guid submitId)
        {
            var first = dto.HandleActual.FirstOrDefault();
            if (first == null) throw new Exception("Không có dữ liệu KPI");

            var request = new KPIRequest
            {
                Id = Guid.NewGuid(),
                SubmitId = submitId,
                Title = first.HeaderTitle ?? "",
                RequestStatus = StatusKPIRequestType.WaitingForApproval,
                CreatedBy = first.CreatedBy,
                CreatedDate = DateTime.Now
            };

            await _kpiRequestRepo.InsertAsync(request);
        }

        private async Task InsertMonthlyAsync(HandleKPISubmitDto dtos, Guid submitId, List<KPITotalScore> totalScores, int month)
        {
            var items = dtos.HandleActual.Where(x => x.Month == month).ToList();
            if (!items.Any()) return;

            var dto = dtos.KPITotalScore.FirstOrDefault(x => x.Month == month);

            var score = new KPITotalScore
            {
                Id = Guid.NewGuid(),
                SubmitId = submitId,
                KeyTaskScore = items.Sum(x => x.KeyTaskScore ?? 0),
                KPIScore = items.Sum(x => x.KPIScore ?? 0),
                OMGScore = items.Sum(x => x.OMGScore ?? 0),
                TotaleScore = dto?.TotaleScore ?? 0,
                TotalActualScore = dto?.TotalActualScore ?? 0,
                Status = StatusKPIRequestType.WaitingForApproval,
                Month = month,
                Year = items.First().Year,
                CreatedBy = items.First().CreatedBy,
                CreatedDate = DateTime.Now,
                IsActive = true,
                Title = items.First().HeaderTitle ?? "",
                kPITotalScoreType = KPITotalScoreType.Staff
            };

            await _kpiTotalScoreRepo.InsertAsync(score);
            totalScores.Add(score);
        }

        private async Task InsertQuarterAsync(HandleKPISubmitDto dtos, Guid submitId, List<KPITotalScore> totalScores, int quarter)
        {
            var dto = dtos.KPITotalScore.FirstOrDefault(x => x.Quarter == quarter);
            if (dto == null) return;

            var score = new KPITotalScore
            {
                Id = Guid.NewGuid(),
                SubmitId = submitId,
                KeyTaskScore = dto.KeyTaskScore,
                KPIScore = dto.KPIScore,
                OMGScore = dto.OMGScore,
                TotaleScore = dto.TotaleScore,
                TotalActualScore = dto.TotalActualScore,
                Status = StatusKPIRequestType.WaitingForApproval,
                Quarter = quarter,
                Year = dto.Year,
                CreatedBy = dto.CreatedBy,
                CreatedDate = DateTime.Now,
                IsActive = true,
                Title = dto.Title ?? "",
                kPITotalScoreType = KPITotalScoreType.Staff
            };

            await _kpiTotalScoreRepo.InsertAsync(score);
            totalScores.Add(score);
        }

        private async Task InsertHalfYearAsync(HandleKPISubmitDto dtos, Guid submitId, List<KPITotalScore> totalScores, int halfYear)
        {
            var dto = dtos.KPITotalScore.FirstOrDefault(x => x.HalfYear == halfYear);
            if (dto == null) return;

            var score = new KPITotalScore
            {
                Id = Guid.NewGuid(),
                SubmitId = submitId,
                KeyTaskScore = dto.KeyTaskScore,
                KPIScore = dto.KPIScore,
                OMGScore = dto.OMGScore,
                TotaleScore = dto.TotaleScore,
                TotalActualScore = dto.TotalActualScore,
                Status = StatusKPIRequestType.WaitingForApproval,
                HalfYear = halfYear,
                Year = dto.Year,
                CreatedBy = dto.CreatedBy,
                CreatedDate = DateTime.Now,
                IsActive = true,
                Title = dto.Title ?? "",
                kPITotalScoreType = KPITotalScoreType.Staff
            };

            await _kpiTotalScoreRepo.InsertAsync(score);
            totalScores.Add(score);
        }

        private async Task InsertDetailAndActualAsync(HandleKPISubmitDto dtos, List<KPITotalScore> totalScores, int submitMonth, Guid userId)
        {
            var allTaskIds = dtos.HandleActual.Select(x => x.TaskId).Distinct().ToList();

            var userTasks = await _userTaskepo.Query()
                .Where(x => allTaskIds.Contains(x.KPITaskId) && x.UserId == userId)
                .ToDictionaryAsync(x => x.KPITaskId, x => x);

            var targets = await _kpiTargeRepo.GetAllAsync();

            foreach (var item in dtos.HandleActual)
            {
                if (!userTasks.TryGetValue(item.TaskId, out var userTask))
                    continue;

                var targetId = targets
                    .Where(x => x.UserTaskId == userTask.Id && x.Year == item.Year)
                    .Select(x => x.Id)
                    .FirstOrDefault();

                var totalScore = totalScores.FirstOrDefault(x =>
                    x.Year == item.Year &&
                    (
                        (item.Month != null && x.Month == item.Month) ||
                        (item.Quarter != null && x.Quarter == item.Quarter) ||
                        (item.HalfYear != null && x.HalfYear == item.HalfYear)
                    ));

                if (totalScore == null) continue;

                var detail = new KPIDetailScore
                {
                    Id = Guid.NewGuid(),
                    TotalScoreId = totalScore.Id,
                    TaskId = item.TaskId,
                    Max = item.Max ?? 0,
                    Bottom = item.Bottom ?? 0,
                    Weight = item.Weight ?? 0,
                    Target = item.TargetValue ?? 0,
                    Achievement = item.Achiement ?? 0,
                    Actual = item.ActualValue ?? 0,
                    Score = item.Score ?? 0,
                    Month = item.Month,
                    Quarter = item.Quarter,
                    HalfYear = item.HalfYear,
                    Year = item.Year,
                    CreatedDate = DateTime.Now,
                    IsActive = true
                };

                var actual = new KPIMonthlyActual
                {
                    Id = Guid.NewGuid(),
                    KPITargetId = targetId,
                    KPITotalScoreId = totalScore.Id,
                    Month = item.Month,
                    Quarter = item.Quarter,
                    HalfYear = item.HalfYear,
                    Year = item.Year,
                    ActualValue = item.ActualValue,
                    CreatedDate = DateTime.Now,
                    CreatedBy = userId,
                    UpdatedDate = DateTime.Now
                };

                await _kpiDetailScoreRepo.InsertAsync(detail);
                await _KPIMonthlyActualRepo.InsertAsync(actual);
            }
        }

        private async Task CreatedRequest(Guid submitId, List<KPITotalScore> totalScores)
        {
            var kpi = totalScores.FirstOrDefault(x => x.Month != null);
            if (kpi == null) throw new Exception("Không tìm thấy KPI");

            var user = await _userRepo.FirstOrDefaultAsync(x => x.Id == kpi.CreatedBy);
            if (user == null) throw new Exception("Không tìm thấy user");

            var flows = await _approvalFlowRepo.Query()
                .Where(f => f.FlowApproval == user.FlowApproval && f.SystemType == SystemType.KPI)
                .OrderBy(f => f.CurrentStep)
                .ToListAsync();

            var tasks = new List<RequestApprovalTask>();

            foreach (var flow in flows)
            {
                // =========================
                // CASE 1: ASSIGNEE
                // =========================
                if (flow.ApprovalSettingType == ApprovalSettingType.Assignee)
                {
                    if (flow.DecisionMaker == null)
                        throw new Exception($"Flow step {flow.CurrentStep} chưa có DecisionMaker.");

                    var task = new RequestApprovalTask
                    {
                        Id = Guid.NewGuid(),
                        SubmitId = submitId,
                        Title = kpi.Title,
                        RequesterId = kpi.CreatedBy,
                        StatusType = flow.CurrentStep == 1
                            ? EApprovalStatusType.InReview
                            : EApprovalStatusType.Pending,
                        ApprovedBy = null,
                        ApprovalDate = null,
                        RejectBy = null,
                        RejectDate = null,
                        Comment = null,
                        Step = flow.CurrentStep ?? 1,
                        DecisionMaker = flow.DecisionMaker,
                        Month = kpi.Month ?? 0,
                        CreatedDate = DateTime.Now
                    };

                    tasks.Add(task);
                }

                // =========================
                // CASE 2: DIRECT MANAGER
                // =========================
                else if (flow.ApprovalSettingType == ApprovalSettingType.DirectManagement)
                {
                    if (user.ManagerId == null)
                        throw new Exception("User chưa có Manager.");

                    var task = new RequestApprovalTask
                    {
                        Id = Guid.NewGuid(),
                        SubmitId = submitId,
                        Title = kpi.Title,
                        RequesterId = kpi.CreatedBy,
                        StatusType = EApprovalStatusType.InReview, // luôn review ngay
                        ApprovedBy = null,
                        ApprovalDate = null,
                        RejectBy = null,
                        RejectDate = null,
                        Comment = null,
                        Step = 1, // luôn step 1
                        DecisionMaker = user.ManagerId,
                        Month = kpi.Month ?? 0,
                        CreatedDate = DateTime.Now
                    };

                    tasks.Add(task);
                }
            }

            await _requestApprovalTaskRepo.InsertRangeAsync(tasks);
        }

        #endregion
        public async Task<List<ViewKPITotalScoreDto>> GetKPITotalScoreByUserId(Guid userId)
        {
            var query = from totalScore in await _kpiTotalScoreRepo.GetQueryableAsync()
                        join user in await _userRepo.GetQueryableAsync()
                        on totalScore.CreatedBy equals user.Id
                        join manager in await _userRepo.GetQueryableAsync()
                        on user.ManagerId equals manager.Id into mgr
                        from manager in mgr.DefaultIfEmpty()
                        where totalScore.CreatedBy == userId
                        orderby totalScore.CreatedDate descending
                        select new ViewKPITotalScoreDto
                        {
                            Id = totalScore.Id,
                            SubmitId = totalScore.SubmitId,
                            KPIScore = totalScore.KPIScore,
                            KeyTaskScore = totalScore.KeyTaskScore,
                            OMGScore = totalScore.OMGScore,
                            CreatedBy = totalScore.CreatedBy,
                            Month = totalScore.Month,
                            Quarter = totalScore.Quarter,
                            HalfYear = totalScore.HalfYear,
                            Year = totalScore.Year,
                            CreatedDate = totalScore.CreatedDate,
                            Status = totalScore.Status,
                            TotaleScore = totalScore.TotaleScore ?? 0,
                            IsActive = totalScore.IsActive,
                            ManagerId = manager != null ? manager.Id : Guid.Empty,
                            ManagerName = manager != null ? manager.FullName : string.Empty,
                            Title = totalScore.Title,
                            UserName = user.FullName
                        };
            return await query.ToListAsync();
        }
        public async Task<List<ViewKPIForApprovalDto>> GetKPIForApproval()
        {
            // Load data 1 lần
            var requests = _requestApprovalTaskRepo.Query().ToList();
            var usersDict = _userRepo.Query().ToDictionary(x => x.Id);
            var flowsDict = _approvalFlowRepo.Query().ToDictionary(x => x.Id);
            var positionsDict = _positionRepo.Query().ToDictionary(x => x.Id);

            var result = new List<ViewKPIForApprovalDto>(requests.Count);

            foreach (var req in requests)
            {
                if (req == null) continue;

                // 🟩 User
                usersDict.TryGetValue(req.RequesterId, out var user);
                string userName = user?.FullName ?? string.Empty;

                // 🟩 Flow
                flowsDict.TryGetValue(req.Id, out var flow);
                Guid? decisionMaker = req.DecisionMaker ?? flow?.DecisionMaker;

                // 🟩 Position
                Guid? positionId = user?.PositionId;

                positionsDict.TryGetValue(positionId ?? Guid.Empty, out var position);
                string positionName = position?.Name ?? string.Empty;

                // 🟩 DTO
                result.Add(new ViewKPIForApprovalDto
                {
                    Id = req.Id,
                    SubmitId = req.SubmitId,
                    Step = req.Step ?? 1,
                    Title = req.Title ?? string.Empty,
                    CreatedBy = req.RequesterId,
                    CreatedByName = userName,
                    CreatedDate = req.CreatedDate,
                    EApprovalStatusType = req.StatusType,
                    Position = positionName,
                    PositionId = positionId ?? Guid.Empty,
                    DecisionMaker = decisionMaker,
                    Month = req.Month,
                    ApprovalDate = req.ApprovalDate,
                    RejectDate = req.RejectDate
                });
            }

            return result;
        }
        //public async Task<List<ViewKPIForApprovalDto>> GetKPIForApproval()
        //{
        //    var requests =  _requestApprovalTaskRepo.Query().ToList() ;
        //    var users =  _userRepo.Query().ToList() ;
        //    var flows =  _approvalFlowRepo.Query().ToList() ;
        //    var positions =  _positionRepo.Query().ToList() ;

        //    var result = new List<ViewKPIForApprovalDto>();

        //    foreach (var req in requests)
        //    {
        //        if (req == null) continue;

        //        // 🟩 Lấy user (có thể null)
        //        var user = users.FirstOrDefault(u => u.Id == req.RequesterId);
        //        string userName = user?.FullName ?? string.Empty;

        //        // 🟩 Lấy flow (có thể null)
        //        var flow = flows.FirstOrDefault(f => f.Id == req.Id);
        //        Guid? decisionMaker = req.DecisionMaker ?? flow?.DecisionMaker;

        //        // 🟩 Lấy PositionId từ flow hoặc user
        //        Guid? positionId =/* flow?.PositionId ??*/ user?.PositionId;
        //        var position = positionId != null
        //            ? positions.FirstOrDefault(p => p.Id == positionId)
        //            : null;
        //        string positionName = position?.Name ?? string.Empty;
        //        int step = req.Step ?? 1;
        //        var dto = new ViewKPIForApprovalDto
        //        {
        //            Id = req.Id,
        //            SubmitId = req.SubmitId,
        //            Step = step,
        //            Title = req.Title ?? string.Empty,
        //            CreatedBy = req.RequesterId,
        //            CreatedByName = userName,
        //            CreatedDate = req.CreatedDate,
        //            EApprovalStatusType = req.StatusType,
        //            Position = positionName,
        //            PositionId = positionId ?? Guid.Empty,
        //            DecisionMaker = decisionMaker,
        //            Month = req.Month,
        //            ApprovalDate = req.ApprovalDate,
        //            RejectDate = req.RejectDate
        //        };

        //        result.Add(dto);
        //    }

        //    return result.ToList();
        //}
        public async Task<List<ProcessApprovalDto>> GetProcessApprovalBySubmitIdAsync(Guid submitId)
        {
            // Lấy tất cả RequestApprovalTask theo SubmitId
            var requestTasks = (await _requestApprovalTaskRepo.GetAllAsync())
                                .Where(x => x.SubmitId == submitId)
                                .ToList();

            if (!requestTasks.Any())
                return new List<ProcessApprovalDto>();

            var result = new List<ProcessApprovalDto>();

            foreach (var task in requestTasks)
            {
                ProcessApprovalDto process = new ProcessApprovalDto
                {
                    RequestApprovalTaskId = task.Id,
                    ApprovalStatusType = task.StatusType,
                    Comment = task.Comment ?? string.Empty,
                    StepNumber = task.Step ?? 0
                };

                if (task.StatusType == EApprovalStatusType.Approved && task.ApprovedBy.HasValue)
                {
                    var user = await _userRepo.FirstOrDefaultAsync(u => u.Id == task.ApprovedBy.Value);
                    process.ApprovalName = user?.FullName ?? "Unknown";
                    process.ApprovalEmail = user?.Email ?? string.Empty;
                    process.ApprovalDate = task.ApprovalDate;
                }
                else if (task.StatusType == EApprovalStatusType.Rejected && task.RejectBy.HasValue)
                {
                    var user = await _userRepo.FirstOrDefaultAsync(u => u.Id == task.RejectBy.Value);
                    process.RejectedName = user?.FullName ?? "Unknown";
                    process.RejectedEmail = user?.Email ?? string.Empty;
                    process.RejectedDate = task.RejectDate;
                }
                else if (task.StatusType == EApprovalStatusType.Pending)
                {
                    var user = await _userRepo.FirstOrDefaultAsync(u => u.Id == task.DecisionMaker.Value);
                    process.PendingEmail = user?.FullName ?? "Unknown";
                    process.PendingName = user?.Email ?? string.Empty;
                }
                else if (task.StatusType == EApprovalStatusType.InReview)
                {
                    var user = await _userRepo.FirstOrDefaultAsync(u => u.Id == task.DecisionMaker.Value);
                    process.ApprovalName = user?.FullName ?? "Unknown";
                    process.ApprovalEmail = user?.Email ?? string.Empty;
                    process.ApprovalDate = task.ApprovalDate;
                }
                result.Add(process);
            }

            return result;
        }
        public async Task<ViewDetailApprovalKPI> GetDetailKPIBySubmitId(Guid submitId)
        {
            var result = new ViewDetailApprovalKPI();

            // ===================== 1. TOTAL SCORE =====================
            var totalScores = (await _kpiTotalScoreRepo.GetAllAsync())
                .Where(ts => ts.SubmitId == submitId && ts.IsActive)
                .OrderBy(ts => ts.CreatedDate)
                .ToList();

            if (!totalScores.Any())
                return result;

            var totalScoreIds = totalScores.Select(x => x.Id).ToList();

            foreach (var ts in totalScores)
            {
                result.TotalScore.Add(new TotalScoreDto
                {
                    Id = ts.Id,
                    Title = ts.Title,
                    KPIScore = ts.KPIScore,
                    KeyTaskScore = ts.KeyTaskScore,
                    OMGScore = ts.OMGScore,
                    TotaleScore = ts.TotaleScore ?? 0,
                    Month = ts.Month,
                    Quarter = ts.Quarter,
                    HalfYear = ts.HalfYear,
                    Year = ts.Year,
                    Status = ts.Status,
                    CreatedBy = ts.CreatedBy,
                    CreatedDate = ts.CreatedDate,
                    IsActive = ts.IsActive,
                });
            }

            // ===================== 2. DETAIL SCORE (FULL) =====================
            var detailScores = (await _kpiDetailScoreRepo.GetAllAsync())
                .Where(d => d.IsActive && totalScoreIds.Contains(d.TotalScoreId))
                .OrderBy(d => d.CreatedDate)
                .ToList();

            if (!detailScores.Any())
                return result;

            // ===================== 3. TASK + FORMULA =====================
            var taskIds = detailScores.Select(d => d.TaskId).Distinct().ToList();

            var tasks = (await _kpiTaskRepo.GetAllAsync())
                .Where(t => taskIds.Contains(t.Id))
                .ToList();

            var formulaIds = tasks.Select(t => t.FormulaId).Distinct().ToList();

            var formulas = (await _formulaRepo.GetAllAsync())
                .Where(f => formulaIds.Contains(f.Id))
                .ToList();
            var userTaskRepo = await _userTaskepo.GetAllAsync();
            // ===================== 4. MAP DETAIL SCORE =====================
            foreach (var d in detailScores)
            {
                var task = tasks.FirstOrDefault(t => t.Id == d.TaskId);
                if (task == null) continue;
                var index = userTaskRepo.FirstOrDefault(x => x.KPITaskId == d.TaskId).Index ?? 0;
                var formula = formulas.FirstOrDefault(f => f.Id == task.FormulaId);

                result.DetailScore.Add(new DetailScoreDto
                {
                    TotalScoreId = d.TotalScoreId,
                    TaskId = d.TaskId,

                    Month = d.Month,
                    Quarter = d.Quarter,
                    HalfYear = d.HalfYear,
                    Year = d.Year,

                    ActualValue = d.Actual,
                    CreatedDate = d.CreatedDate,

                    HeaderTitle = task.TaskName,
                    TaskName = task.TaskName,
                    Description = task.TaskDescription,
                    CalculateMethod = task.CalculatedMdethod,
                    KPIIndexType = task.KPIIndexType,
                    KPICategory = task.KPICategory,

                    Max = task.Max,
                    Bottom = task.Bottom,
                    Weight = d.Weight,
                    IndexWeight = d.Weight,
                    Index = index,
                    Unit = task.Unit,
                    PIC = task.PIC,
                    DataSource = task.DataSource,

                    Formula = formula?.FormulaName,
                    KPIFormulaType = formula.KPIFormulaType,

                    TargetValue = d.Target,
                    Achiement = d.Achievement,
                    Score = d.Score
                });
            }

            return result;
        }
        #region APPROVAL AND REJECT
        public async Task<bool> HandleKpiForApproval(ApprovalInfo approvalInfo)
        {
            var currentTask = await _requestApprovalTaskRepo
                .FirstOrDefaultAsync(x => x.Id == approvalInfo.RequestTaskId && x.SubmitId == approvalInfo.SubmitId);

            if (currentTask == null) return false;

            await UpdateCurrentTask(currentTask, approvalInfo);

            var allTasks = await _requestApprovalTaskRepo.GetListAsync(x => x.SubmitId == approvalInfo.SubmitId);
            var kpiTotalList = await _kpiTotalScoreRepo.GetListAsync(x => x.SubmitId == approvalInfo.SubmitId);
            var kpiRequest = await _kpiRequestRepo.FirstOrDefaultAsync(x => x.SubmitId == approvalInfo.SubmitId);

            if (allTasks.Any(x => x.StatusType == EApprovalStatusType.Rejected))
            {
                await HandleRejectedTasks(allTasks, kpiTotalList, kpiRequest);
                return true;
            }

            if (approvalInfo.IsApproved)
            {
                await MoveNextStep(allTasks, currentTask);
            }

            if (allTasks.All(x => x.StatusType == EApprovalStatusType.Approved))
            {
                await HandleApprovedTasks(allTasks, kpiTotalList, kpiRequest, approvalInfo);
            }

            return true;
        }
        private async Task UpdateCurrentTask(RequestApprovalTask task, ApprovalInfo approvalInfo)
        {
            if (approvalInfo.IsApproved)
            {
                task.StatusType = EApprovalStatusType.Approved;
                task.ApprovedBy = approvalInfo.DecisionMaker;
                task.ApprovalDate = DateTime.Now;
            }
            else
            {
                task.StatusType = EApprovalStatusType.Rejected;
                task.RejectBy = approvalInfo.DecisionMaker;
                task.RejectDate = DateTime.Now;
            }
            task.Comment = approvalInfo.Comments;
            await _requestApprovalTaskRepo.SaveChangesAsync();
        }
        private async Task HandleRejectedTasks(List<RequestApprovalTask> allTasks, List<KPITotalScore> kpiTotalList, KPIRequest kpiRequest)
        {
            foreach (var kpiTotal in kpiTotalList)
            {
                kpiTotal.Status = StatusKPIRequestType.Rejected;
            }
            await _kpiTotalScoreRepo.SaveChangesAsync();

            if (kpiRequest != null)
            {
                kpiRequest.RequestStatus = StatusKPIRequestType.Rejected;
                kpiRequest.RejectDate = DateTime.Now;
                var lastRejectedTask = allTasks
                    .Where(x => x.StatusType == EApprovalStatusType.Rejected)
                    .OrderByDescending(x => x.Step)
                    .FirstOrDefault();
                if (lastRejectedTask != null)
                    kpiRequest.RejectBy = lastRejectedTask.DecisionMaker;

                await _kpiRequestRepo.SaveChangesAsync();
            }
        }
        private async Task MoveNextStep(List<RequestApprovalTask> allTasks, RequestApprovalTask currentTask)
        {
            var nextTask = allTasks
                .FirstOrDefault(x => x.Step == currentTask.Step + 1 && x.StatusType == EApprovalStatusType.Pending);

            if (nextTask != null)
            {
                nextTask.StatusType = EApprovalStatusType.InReview;
                await _requestApprovalTaskRepo.SaveChangesAsync();
            }
        }
        private async Task HandleApprovedTasks(List<RequestApprovalTask> allTasks, List<KPITotalScore> kpiTotalList, KPIRequest kpiRequest, ApprovalInfo approvalInfo)
        {
            foreach (var kpiTotal in kpiTotalList)
            {
                kpiTotal.Status = StatusKPIRequestType.Approved;
            }
            await _kpiTotalScoreRepo.SaveChangesAsync();

            if (kpiRequest != null)
            {
                kpiRequest.RequestStatus = StatusKPIRequestType.Approved;
                kpiRequest.ApprovalDate = DateTime.Now;
                //var lastApprovedTask = allTasks.OrderByDescending(x => x.Step).FirstOrDefault();
                if (approvalInfo != null)
                    kpiRequest.ApprovalBy = approvalInfo.DecisionMaker;

                await _kpiRequestRepo.SaveChangesAsync();
            }
        }
        #endregion
        #region RESULT KPI
        public async Task<List<ViewResultKpiDto>> ResultAllKpi()
        {
            var query =
                from request in _kpiRequestRepo.Query()

                join totalScore in _kpiTotalScoreRepo.Query()
                    on request.SubmitId equals totalScore.SubmitId

                join user in _userRepo.Query()
                    on request.CreatedBy equals user.Id

                join approval in _userRepo.Query()
                    on request.ApprovalBy equals approval.Id

                join department in _departmentRepo.Query()
                    on user.DepartmentId equals department.Id into dept
                from department in dept.DefaultIfEmpty()

                join factory in _factoryRepo.Query()
                    on user.FactoryId equals factory.Id into fact
                from factory in fact.DefaultIfEmpty()

                where totalScore.Status == StatusKPIRequestType.Approved
                   && request.RequestStatus == StatusKPIRequestType.Approved

                select new ViewResultKpiDto
                {
                    SubmitId = request.SubmitId,
                    UserName = user.FullName,
                    Approver = approval.FullName,
                    Status = totalScore.Status,

                    // ✅ tránh SQL xử lý DateTime.Now
                    ApprovalDate = request.ApprovalDate ?? DateTime.Now,

                    Month = totalScore.Month,
                    Quarter = totalScore.Quarter,
                    HalfYear = totalScore.HalfYear,
                    Year = totalScore.Year,

                    KPIScore = totalScore.KPIScore,
                    KeyTaskScore = totalScore.KeyTaskScore,
                    OMGScore = totalScore.OMGScore,
                    TotalScroe = totalScore.TotaleScore,

                    WorkDayId = user.WorkDayId,
                    Description = request.Description,

                    DepartmentId = user.DepartmentId,
                    FactoryId = user.FactoryId,

                    Department = department != null ? department.Name : "",
                    Factory = factory != null ? factory.Name : "",

                    kPITotalScoreType = totalScore.kPITotalScoreType,
                };

            // ✅ chạy 1 query duy nhất + no tracking (rất nhanh)
            return await query.AsNoTracking().ToListAsync();
        }
        #endregion
        #region CALCULATE POINT
        public async Task<bool> CalculateQuarterPoint(HandleKPISubmitDto handleKPISubmit)
        {
            var totalScoresInserted = new List<KPITotalScore>();
            var submitId = Guid.NewGuid();

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                await CalculatePointQuarterAsync(submitId, handleKPISubmit, totalScoresInserted);
                await CalculateQuarterPointHalfYearAsync(submitId, handleKPISubmit, totalScoresInserted);
                await InsertDetailAndActualForCalculatedAsync(handleKPISubmit, totalScoresInserted);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw ex;
            }
        }

        private async Task CalculatePointQuarterAsync(
            Guid submitId,
            HandleKPISubmitDto handleKPISubmit,
            List<KPITotalScore> totalScoresInserted)
        {
            try
            {
                var dto = handleKPISubmit.KPITotalScore.FirstOrDefault(x => x.Quarter.HasValue && x.Quarter > 0);
                if (dto == null) return;

                var request = new KPIRequest
                {
                    Id = Guid.NewGuid(),
                    SubmitId = submitId,
                    Title = dto.Title + " - (Admin Calculate Point Quarter)",
                    RequestStatus = StatusKPIRequestType.Approved,
                    CreatedBy = dto.CreatedBy,
                    CreatedDate = DateTime.Now,
                    Note = "Admin Calculate Point Quarter",
                    Description = "Admin Calculate Point Quarter",
                    ApprovalBy = new Guid("B3A87D42-4CD2-4882-A1EE-EAEDE1707AC6")
                };
                await _kpiRequestRepo.InsertAsync(request);

                var score = new KPITotalScore
                {
                    Id = Guid.NewGuid(),
                    SubmitId = submitId,
                    KPIScore = dto.KPIScore,
                    KeyTaskScore = dto.KeyTaskScore,
                    OMGScore = dto.OMGScore,
                    TotaleScore = dto.TotaleScore,
                    TotalActualScore = dto.TotalActualScore,
                    Status = StatusKPIRequestType.Approved,
                    Quarter = dto.Quarter.Value,
                    Year = dto.Year,
                    CreatedBy = dto.CreatedBy,
                    CreatedDate = DateTime.Now,
                    IsActive = true,
                    Title = dto.Title + " - (Admin Calculate Point Quarter)",
                    kPITotalScoreType = KPITotalScoreType.HR
                };
                await _kpiTotalScoreRepo.InsertAsync(score);

                totalScoresInserted.Add(score);

            }
            catch(Exception ex)
            {
                throw ex;
            }
            
        }

        private async Task CalculateQuarterPointHalfYearAsync(
            Guid submitId,
            HandleKPISubmitDto handleKPISubmit,
            List<KPITotalScore> totalScoresInserted)
        {
            try
            {
                var dto = handleKPISubmit.KPITotalScore.FirstOrDefault(x => x.HalfYear.HasValue && x.HalfYear > 0);
                if (dto == null) return;

                var score = new KPITotalScore
                {
                    Id = Guid.NewGuid(),
                    SubmitId = submitId,
                    KPIScore = dto.KPIScore,
                    KeyTaskScore = dto.KeyTaskScore,
                    OMGScore = dto.OMGScore,
                    TotaleScore = dto.TotaleScore,
                    TotalActualScore = dto.TotaleScore,
                    Status = StatusKPIRequestType.Approved,
                    HalfYear = dto.HalfYear.Value,
                    Year = dto.Year,
                    CreatedBy = dto.CreatedBy,
                    CreatedDate = DateTime.Now,
                    IsActive = true,
                    Title = dto.Title + " - (Admin Calculate Point HalfYear)",
                    kPITotalScoreType = KPITotalScoreType.HR
                };
                await _kpiTotalScoreRepo.InsertAsync(score);

                totalScoresInserted.Add(score);

            }
            catch(Exception ex)
            {
                throw ex;
            }
          
        }

        private async Task InsertDetailAndActualForCalculatedAsync(
            HandleKPISubmitDto handleKPISubmit,
            List<KPITotalScore> totalScoresInserted)
        {
            try
            {
                var allHandleActuals = handleKPISubmit.HandleActual;

                if (!allHandleActuals.Any()) return;

                var userId = handleKPISubmit.HandleActual.First().CreatedBy;

                var allTaskIds = allHandleActuals.Select(x => x.TaskId).Distinct().ToList();

                var userTasks = await _userTaskepo.Query()
                    .Where(x => allTaskIds.Contains(x.KPITaskId) && x.UserId == userId)
                    .ToDictionaryAsync(x => x.KPITaskId, x => x);

                var targets = await _kpiTargeRepo.GetAllAsync();

                foreach (var item in allHandleActuals)
                {
                    if (!userTasks.TryGetValue(item.TaskId, out var userTask))
                        continue;

                    var targetId = targets
                        .Where(x => x.UserTaskId == userTask.Id && x.Year == item.Year)
                        .Select(x => x.Id)
                        .FirstOrDefault();

                    var totalScore = totalScoresInserted.FirstOrDefault(x =>
                        x.Year == item.Year &&
                        (
                            (item.Quarter != null && x.Quarter == item.Quarter) ||
                            (item.HalfYear != null && x.HalfYear == item.HalfYear)
                        ));

                    if (totalScore == null) continue;

                    var detail = new KPIDetailScore
                    {
                        Id = Guid.NewGuid(),
                        TotalScoreId = totalScore.Id,
                        TaskId = item.TaskId,
                        Max = item.Max ?? 0,
                        Bottom = item.Bottom ?? 0,
                        Weight = item.Weight ?? 0,
                        Target = item.TargetValue ?? 0,
                        Achievement = item.Achiement ?? 0,
                        Actual = item.ActualValue ?? 0,
                        Score = item.Score ?? 0,
                        Month = item.Month,
                        Quarter = item.Quarter,
                        HalfYear = item.HalfYear,
                        Year = item.Year,
                        CreatedDate = DateTime.Now,
                        IsActive = true
                    };

                    var actual = new KPIMonthlyActual
                    {
                        Id = Guid.NewGuid(),
                        KPITargetId = targetId,
                        KPITotalScoreId = totalScore.Id,
                        Month = item.Month,
                        Quarter = item.Quarter,
                        HalfYear = item.HalfYear,
                        Year = item.Year,
                        ActualValue = item.ActualValue,
                        CreatedDate = DateTime.Now,
                        CreatedBy = userId,
                        UpdatedDate = DateTime.Now
                    };

                    await _kpiDetailScoreRepo.InsertAsync(detail);
                    await _KPIMonthlyActualRepo.InsertAsync(actual);
                }
            }
            catch(Exception ex)
            {
                throw ex;
            }
            
        }
        #endregion
        #region GET MONTH RESULT
        public async Task<List<KPITotalScoreDto>> GetKPITotalScoreByUserId(Guid userId, int year, int month)
        {
            var query = from totalScore in await _kpiTotalScoreRepo.GetQueryableAsync()
                        join request in await _kpiRequestRepo.GetQueryableAsync()
                        on totalScore.SubmitId equals request.SubmitId
                        where request.RequestStatus == StatusKPIRequestType.Approved
                        && totalScore.Status == StatusKPIRequestType.Approved
                        && totalScore.CreatedBy == userId
                        && totalScore.Year == year
                        && totalScore.Month == month
                        select new KPITotalScoreDto
                        {
                            KPIScore = totalScore.KPIScore,
                            KeyTaskScore = totalScore.KeyTaskScore,
                            OMGScore = totalScore.OMGScore,
                            CreatedBy = totalScore.CreatedBy,
                            Month = totalScore.Month,
                            Quarter = totalScore.Quarter,
                            HalfYear = totalScore.HalfYear,
                            Year = totalScore.Year,
                            CreatedDate = totalScore.CreatedDate,
                            Status = request.RequestStatus,
                            TotaleScore = totalScore.TotaleScore ?? 0,
                            IsActive = totalScore.IsActive,
                            Title = totalScore.Title
                        };
            return await query.ToListAsync();
        }
        public async Task<List<HandleActualDto>> GetResultDetail(Guid userId, int year, int month)
        {
            var query = from totalScore in await _kpiTotalScoreRepo.GetQueryableAsync()
                        join detailScore in await _kpiDetailScoreRepo.GetQueryableAsync()
                        on totalScore.Id equals detailScore.TotalScoreId
                        join task in await _kpiTaskRepo.GetQueryableAsync()
                        on detailScore.TaskId equals task.Id
                        join fomulaRepo in await _formulaRepo.GetQueryableAsync() on task.FormulaId equals fomulaRepo.Id
                        join quarterCalculate in await _quarterCalculateRepo.GetQueryableAsync()
                         on task.CalculatedId equals quarterCalculate.Id
                        where totalScore.CreatedBy == userId
                        && totalScore.Year == year
                        && totalScore.Month == month
                        && totalScore.Status == StatusKPIRequestType.Approved
                        select new HandleActualDto
                        {
                            TaskId = detailScore.TaskId,
                            Month = detailScore.Month,
                            Quarter = detailScore.Quarter,
                            HalfYear = detailScore.HalfYear,
                            Year = detailScore.Year,
                            ActualValue = detailScore.Actual,
                            Index = 0,
                            CreatedBy = totalScore.CreatedBy,
                            CreatedDate = totalScore.CreatedDate,
                            KPIIndexType = task.KPIIndexType,
                            KPICategory = task.KPICategory,
                            Max = detailScore.Max,
                            Bottom = detailScore.Bottom,
                            Weight = detailScore.Weight,
                            Unit = task.Unit,
                            TargetValue = detailScore.Target,
                            Achiement = detailScore.Achievement,
                            Score = detailScore.Score,
                            TaskName = task.TaskName,
                            QuarterCalculateType = quarterCalculate.QuarterCalculateType,
                            HeaderTitle = totalScore.Title,
                            Description = task.TaskDescription,
                            CalculateMethod = task.CalculatedMdethod,
                            Formula = fomulaRepo.FormulaName,
                            KPIFormulaType = fomulaRepo.KPIFormulaType,
                            calculated = quarterCalculate.QuarterCalculated,
                            DataSource = task.DataSource,
                            PIC = task.PIC
                        };

            return await query.ToListAsync();
        }
        #endregion
        #region GET QUARTER RESULT
        public async Task<List<HandleActualDto>> GetResultDetailByQuarter(Guid userId, int year, int quater)
        {
            var query = from totalScore in await _kpiTotalScoreRepo.GetQueryableAsync()
                        join detailScore in await _kpiDetailScoreRepo.GetQueryableAsync()
                        on totalScore.Id equals detailScore.TotalScoreId
                        join task in await _kpiTaskRepo.GetQueryableAsync()
                        on detailScore.TaskId equals task.Id
                        join fomulaRepo in await _formulaRepo.GetQueryableAsync() on task.FormulaId equals fomulaRepo.Id
                        join quarterCalculate in await _quarterCalculateRepo.GetQueryableAsync()
                         on task.CalculatedId equals quarterCalculate.Id
                        where totalScore.CreatedBy == userId
                        && totalScore.Year == year
                        && totalScore.Quarter == quater
                        && totalScore.Status == StatusKPIRequestType.Approved
                        select new HandleActualDto
                        {
                            TaskId = detailScore.TaskId,
                            Month = detailScore.Month,
                            Quarter = detailScore.Quarter,
                            HalfYear = detailScore.HalfYear,
                            Year = detailScore.Year,
                            ActualValue = detailScore.Actual,
                            Index = 0,
                            CreatedBy = totalScore.CreatedBy,
                            CreatedDate = totalScore.CreatedDate,
                            KPIIndexType = task.KPIIndexType,
                            KPICategory = task.KPICategory,
                            Max = detailScore.Max,
                            Bottom = detailScore.Bottom,
                            Weight = detailScore.Weight,
                            Unit = task.Unit,
                            TargetValue = detailScore.Target,
                            Achiement = detailScore.Achievement,
                            Score = detailScore.Score,
                            TaskName = task.TaskName,
                            QuarterCalculateType = quarterCalculate.QuarterCalculateType,
                            HeaderTitle = totalScore.Title,
                            Description = task.TaskDescription,
                            CalculateMethod = task.CalculatedMdethod,
                            Formula = fomulaRepo.FormulaName,
                            KPIFormulaType = fomulaRepo.KPIFormulaType,
                            calculated = quarterCalculate.QuarterCalculated,
                            DataSource = task.DataSource,
                            PIC = task.PIC
                        };

            return await query.ToListAsync();
        }

        public async Task<KPITotalScoreDto> GetResultTotalByQuarter(Guid userId, int year, int quarter)
        {
            var totalScores = await _kpiTotalScoreRepo.GetQueryableAsync();
            var requests = await _kpiRequestRepo.GetQueryableAsync();
            var query = from totalScore in totalScores
                        join request in requests on totalScore.SubmitId equals request.SubmitId
                        where request.RequestStatus == StatusKPIRequestType.Approved
                              && totalScore.CreatedBy == userId
                              && totalScore.Year == year
                              && totalScore.Quarter == quarter
                        select new KPITotalScoreDto
                        {
                            KPIScore = totalScore.KPIScore,
                            KeyTaskScore = totalScore.KeyTaskScore,
                            OMGScore = totalScore.OMGScore,
                            CreatedBy = totalScore.CreatedBy,
                            Month = totalScore.Month,
                            Quarter = totalScore.Quarter,
                            HalfYear = totalScore.HalfYear,
                            Year = totalScore.Year,
                            CreatedDate = totalScore.CreatedDate,
                            Status = request.RequestStatus,
                            TotaleScore = totalScore.TotaleScore ?? 0,
                            IsActive = totalScore.IsActive
                        };
            return await query.FirstOrDefaultAsync();
        }
        #endregion
    }
}
