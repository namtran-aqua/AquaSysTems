   using AquaSolution.Data.Connection;
using AquaSolution.Data.Data.Entities;
using AquaSolution.Data.Data.Entities.KPI;
using AquaSolution.Data.KPI.Entities;
using AquaSolution.Data.Repositories;
using AquaSolution.Shared.Enum;
using AquaSolution.Shared.Enum.KPIType;
using AquaSolution.Shared.KPI.KPIActual;
using AquaSolution.Shared.KPI.KPISubmit;
using AquaSolution.Shared.KPI.Result;
using AquaSolution.Shared.KPI.UserTask;
using Microsoft.EntityFrameworkCore;

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
            IRepository<Department>departmentRepo,
            IRepository<Factory> factoryRepo,
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
                                  OwnerName = users.FirstOrDefault(u => u.Id == kpiTask.OwnerId)?.FullName ?? string.Empty,
                                  DataSource = kpiTask.DataSource,
                                  Formula = formulas.FirstOrDefault(f => f.Id == kpiTask.FormulaId)?.FormulaName ?? string.Empty,
                                  TargetValue = target.TargetValue,
                                  TargetId = target.Id
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
                var actuals = await _KPIMonthlyActualRepo.GetListAsync(a => targetIds.Contains(a.KPIMonthlyTargetId));
                var actualTargetIds = actuals.Select(a => a.KPIMonthlyTargetId).ToHashSet();

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
                              join actual in actuals on target.Id equals actual.KPIMonthlyTargetId
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
                                  OwnerName = users.FirstOrDefault(u => u.Id == kpiTask.OwnerId)?.FullName ?? string.Empty,
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
        public async Task<List<KPITotalScoreDto>> GetKPITotalScoreByUserId(Guid userId, int year, int? month)
        {
            var query = from totalScore in await _kpiTotalScoreRepo.GetQueryableAsync()
                        join request in await _kpiRequestRepo.GetQueryableAsync()
                        on totalScore.SubmitId equals request.SubmitId
                        where request.RequestStatus == StatusKPIRequestType.Approved
                        && totalScore.CreatedBy == userId
                        && totalScore.Year == year
                        && (month == null || totalScore.Month == month)
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
            return await query.ToListAsync();
        }
        public async Task<KPITotalScoreDto> GetKPITotalScoreQuarterByUserId(Guid userId, int year, int? quarter)
        {
            var totalScores = await _kpiTotalScoreRepo.GetQueryableAsync();
            var requests = await _kpiRequestRepo.GetQueryableAsync();
            var query = from totalScore in totalScores
                        join request in requests on totalScore.SubmitId equals request.SubmitId
                        where request.RequestStatus == StatusKPIRequestType.Approved
                              && totalScore.CreatedBy == userId
                              && totalScore.Year == year
                              && (quarter == null || totalScore.Quarter == quarter)
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
        public async Task<List<HandleActualDto>> GetApprovedOMG(Guid userId, int year, int month)
        {
            var query = from totalScore in await _kpiTotalScoreRepo.GetQueryableAsync()
                        join detailScore in await _kpiDetailScoreRepo.GetQueryableAsync()
                        on totalScore.Id equals detailScore.TotalScoreId
                        join task in await _kpiTaskRepo.GetQueryableAsync()
                        on detailScore.TaskId equals task.Id
                        where totalScore.CreatedBy == userId
                        && totalScore.Year == year
                        && totalScore.Month == month
                        select new HandleActualDto
                        {
                            TaskId = detailScore.TaskId,
                            Month = detailScore.Month,
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
                            Score = detailScore.Score
                        };

            return await query.ToListAsync();
        }
        #region Submit KPI
        public async Task<bool> SubmitKPIAsync(HandleKPISubmitDto submitKPIDto)
        {
            var returnSave = 0;
            var totalScoresInserted = new List<KPITotalScore>();
            var SubmitId = Guid.NewGuid();

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    await InsertMonthlyAsync(submitKPIDto, SubmitId, totalScoresInserted);
                    await InsertQuarterAsync(submitKPIDto, SubmitId, totalScoresInserted);
                    await InsertHalfYearAsync(submitKPIDto, SubmitId, totalScoresInserted);
                    await InsertDetailAndActualAsync(submitKPIDto, totalScoresInserted);
                    await CreatedRequest(SubmitId, totalScoresInserted);
                    await transaction.CommitAsync();
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }

            return true; // hoặc có thể return true luôn nếu bạn không dùng returnSave nữa
        }
        private async Task InsertMonthlyAsync(HandleKPISubmitDto dtos, Guid submitId, List<KPITotalScore> totalScores)
        {
            var groups = dtos.HandleActual
                .Where(x => x.Month.HasValue && x.Month != 0)
                .GroupBy(x => new { x.Month, x.Year })
                .ToList();

            var kpiScore = dtos.HandleActual.Where(x => x.Month != null).Sum(x => x.KPIScore ?? 0);
            var keyTaskScore = dtos.HandleActual.Where(x => x.Month != null).Sum(x => x.KeyTaskScore ?? 0);
            var omgScore = dtos.HandleActual.Where(x => x.Month != null).Sum(x => x.OMGScore ?? 0);

            foreach (var group in groups)
            {
                var dto = group.First();

                var request = new KPIRequest
                {
                    Id = Guid.NewGuid(),
                    SubmitId = submitId,
                    Title = dto.HeaderTitle ?? "",
                    RequestStatus = StatusKPIRequestType.WaitingForApproval,
                    CreatedBy = dto.CreatedBy,
                    CreatedDate = DateTime.Now
                };
                await _kpiRequestRepo.InsertAsync(request);

                var score = new KPITotalScore
                {
                    Id = Guid.NewGuid(),
                    SubmitId = submitId,
                    KeyTaskScore = keyTaskScore,
                    KPIScore = kpiScore,
                    OMGScore = omgScore,
                    TotaleScore = keyTaskScore + kpiScore + omgScore,
                    Status = StatusKPIRequestType.WaitingForApproval,
                    Month = dto.Month.Value,
                    Year = dto.Year,
                    CreatedBy = dto.CreatedBy,
                    CreatedDate = DateTime.Now,
                    IsActive = true,
                    Title = dto.HeaderTitle ?? "",
                    kPITotalScoreType = KPITotalScoreType.Staff
                };
                await _kpiTotalScoreRepo.InsertAsync(score);
                await _kpiTotalScoreRepo.SaveChangesAsync();
                totalScores.Add(score);
            }
        }
        private async Task InsertQuarterAsync(HandleKPISubmitDto dtos, Guid submitId, List<KPITotalScore> totalScores)
        {
            var dto = dtos.KPITotalScore.FirstOrDefault(x => x.Quarter.HasValue && x.Quarter > 0);
            if (dto == null) return;

            var kpiScore = dto.KPIScore;
            var keyTaskScore = dto.KeyTaskScore;
            var omgScore = dto.OMGScore;
            var score = new KPITotalScore
            {
                Id = Guid.NewGuid(),
                SubmitId = submitId,
                KeyTaskScore = keyTaskScore,
                KPIScore = kpiScore,
                OMGScore = omgScore,
                TotaleScore = dto.TotaleScore,
                Status = StatusKPIRequestType.WaitingForApproval,
                Quarter = dto.Quarter.Value,
                Year = dto.Year,
                CreatedBy = dto.CreatedBy,
                CreatedDate = DateTime.Now,
                IsActive = true,
                Title = dto.Title ?? "",
                kPITotalScoreType = KPITotalScoreType.Staff
            };
            await _kpiTotalScoreRepo.InsertAsync(score);
            await _kpiTotalScoreRepo.SaveChangesAsync();
            totalScores.Add(score);
        }
        private async Task InsertHalfYearAsync(HandleKPISubmitDto dtos, Guid submitId, List<KPITotalScore> totalScores)
        {
            var dto = dtos.KPITotalScore.FirstOrDefault(x => x.HalfYear.HasValue && x.HalfYear > 0);
            if (dto == null) return;
            var score = new KPITotalScore
            {
                Id = Guid.NewGuid(),
                SubmitId = submitId,
                KeyTaskScore = dto.KeyTaskScore,
                KPIScore = dto.KPIScore,
                OMGScore = dto.OMGScore,
                TotaleScore = dto.TotaleScore,
                Status = StatusKPIRequestType.WaitingForApproval,
                HalfYear = dto.HalfYear.Value,
                Year = dto.Year,
                CreatedBy = dto.CreatedBy,
                CreatedDate = DateTime.Now,
                IsActive = true,
                Title = dto.Title ?? "",
                kPITotalScoreType = KPITotalScoreType.Staff
            };
            await _kpiTotalScoreRepo.InsertAsync(score);
            await _kpiTotalScoreRepo.SaveChangesAsync();
            totalScores.Add(score);
        }
        private async Task InsertDetailAndActualAsync(HandleKPISubmitDto dtos, List<KPITotalScore> totalScores)
        {
            var monthlyItems = dtos.HandleActual.Where(x => x.Month.HasValue).ToList();

            foreach (var item in monthlyItems)
            {
                var totalScore = totalScores.FirstOrDefault(ts =>
                    ts.Month == item.Month && ts.Year == item.Year);

                if (totalScore == null)
                    continue;

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
                await _kpiDetailScoreRepo.InsertAsync(detail);
                await _kpiDetailScoreRepo.SaveChangesAsync();
                var actual = new KPIMonthlyActual
                {
                    Id = Guid.NewGuid(),
                    KPIMonthlyTargetId = item.TargetId,
                    KPITotalScoreId = totalScore.Id,
                    Month = item.Month,
                    Year = item.Year,
                    ActualValue = item.ActualValue,
                    CreatedDate = DateTime.Now,
                    CreatedBy = item.CreatedBy,
                    UpdatedDate = DateTime.Now,
                    UpdatedBy = item.CreatedBy
                };
                await _KPIMonthlyActualRepo.InsertAsync(actual);
                await _KPIMonthlyActualRepo.SaveChangesAsync();
            }
        }
        //private async Task CreatedRequest(Guid submitId, List<KPITotalScore> totalScores)
        //{
        //    var kpi = totalScores.FirstOrDefault(x => x.Month != null);
        //    if (kpi == null)
        //        throw new Exception("Không tìm thấy KPI tương ứng.");

        //    var user = await _userRepo.FirstOrDefaultAsync(u => u.Id == kpi.CreatedBy);
        //    if (user == null)
        //        throw new Exception("Không tìm thấy thông tin người tạo KPI.");

        //    if (user.PositionId == null)
        //        throw new Exception("Người tạo KPI chưa được gán vị trí (Position).");

        //    var positionId = user.PositionId.Value;
        //    var approvalFlows = await _approvalFlowRepo.GetAllAsync();
        //    var filteredFlows = approvalFlows
        //        .Where(f => f.PositionId == positionId)
        //        .OrderBy(f => f.CurrentStep)
        //        .ToList();

        //    if (!filteredFlows.Any())
        //        throw new Exception("Không tìm thấy flow phê duyệt cho vị trí này.");

        //    var requestTasks = new List<RequestApprovalTask>();

        //    foreach (var flow in filteredFlows)
        //    {
        //        var task = new RequestApprovalTask
        //        {
        //            Id = Guid.NewGuid(),
        //            SubmitId = submitId,
        //            Title = kpi.Title,
        //            RequesterId = kpi.CreatedBy,
        //            StatusType = flow.CurrentStep == 1
        //                ? EApprovalStatusType.InReview
        //                : EApprovalStatusType.Pending,
        //            ApprovedBy = null,
        //            ApprovalDate = null,
        //            RejectBy = null,
        //            RejectDate = null,
        //            Comment = null,
        //            Step = flow.CurrentStep,
        //            DecisionMaker = flow.DecisionMaker,
        //            Month = kpi.Month ?? 0,
        //            CreatedDate = DateTime.Now
        //        };

        //        requestTasks.Add(task);
        //    }

        //    // 5️⃣ Lưu các request vào DB
        //    await _requestApprovalTaskRepo.InsertRangeAsync(requestTasks);
        //    await _requestApprovalTaskRepo.SaveChangesAsync();
        //}
        private async Task CreatedRequest(Guid submitId, List<KPITotalScore> totalScores)
        {
            var kpi = totalScores.FirstOrDefault(x => x.Month != null);
            if (kpi == null)
                throw new Exception("Không tìm thấy KPI tương ứng.");
            var user = await _userRepo.FirstOrDefaultAsync(u => u.Id == kpi.CreatedBy);
            if (user == null)
                throw new Exception("Không tìm thấy thông tin người tạo KPI.");

            if (user.PositionId == null)
                throw new Exception("Người tạo KPI chưa được gán vị trí (Position).");

            var positionId = user.PositionId.Value;
            var approvalFlows = await _approvalFlowRepo.GetAllAsync();
            var filteredFlows = approvalFlows
                .Where(f => f.PositionId == positionId && f.SystemType ==SystemType.KPI)
                .OrderBy(f => f.CurrentStep)
                .ToList();

            if (!filteredFlows.Any())
                throw new Exception("Không tìm thấy flow phê duyệt cho vị trí này.");

            var requestTasks = new List<RequestApprovalTask>();
            //var flowType = filteredFlows.First().ApprovalSettingType;

            //if (flowType == ApprovalSettingType.Assignee)
            //{
            //    foreach (var flow in filteredFlows)
            //    {
            //        var task = new RequestApprovalTask
            //        {
            //            Id = Guid.NewGuid(),
            //            SubmitId = submitId,
            //            Title = kpi.Title,
            //            RequesterId = kpi.CreatedBy,
            //            StatusType = flow.CurrentStep == 1
            //                ? EApprovalStatusType.InReview
            //                : EApprovalStatusType.Pending,
            //            ApprovedBy = null,
            //            ApprovalDate = null,
            //            RejectBy = null,
            //            RejectDate = null,
            //            Comment = null,
            //            Step = flow.CurrentStep ?? 1,
            //            DecisionMaker = flow.DecisionMaker,
            //            Month = kpi.Month ?? 0,
            //            CreatedDate = DateTime.Now
            //        };

            //        requestTasks.Add(task);
            //    }
            //}
            //else if (flowType == ApprovalSettingType.DirectManagement)
            //{
            //    if (user.ManagerId == null)
            //        throw new Exception("Người tạo KPI chưa có Manager được gán.");

            //    var directRequest = new RequestApprovalTask
            //    {
            //        Id = Guid.NewGuid(),
            //        SubmitId = submitId,
            //        Title = kpi.Title,
            //        RequesterId = kpi.CreatedBy,
            //        StatusType = EApprovalStatusType.InReview,
            //        ApprovedBy = null,
            //        ApprovalDate = null,
            //        RejectBy = null,
            //        RejectDate = null,
            //        Comment = null,
            //        Step = 1,
            //        DecisionMaker = user.ManagerId, 
            //        Month = kpi.Month ?? 0,
            //        CreatedDate = DateTime.Now
            //    };
            //    requestTasks.Add(directRequest);
            //}
            foreach (var flow in filteredFlows)
            {
                if (flow.ApprovalSettingType == ApprovalSettingType.Assignee)
                {
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

                    requestTasks.Add(task);
                }
                else if (flow.ApprovalSettingType == ApprovalSettingType.DirectManagement)
                {
                    if (user.ManagerId == null)
                        throw new Exception("Người tạo KPI chưa có Manager được gán.");

                    var directRequest = new RequestApprovalTask
                    {
                        Id = Guid.NewGuid(),
                        SubmitId = submitId,
                        Title = kpi.Title,
                        RequesterId = kpi.CreatedBy,
                        StatusType = EApprovalStatusType.InReview,
                        ApprovedBy = null,
                        ApprovalDate = null,
                        RejectBy = null,
                        RejectDate = null,
                        Comment = null,
                        Step = 1,
                        DecisionMaker = user.ManagerId,
                        Month = kpi.Month ?? 0,
                        CreatedDate = DateTime.Now
                    };
                    requestTasks.Add(directRequest);
                }
            }

            await _requestApprovalTaskRepo.InsertRangeAsync(requestTasks);
            await _requestApprovalTaskRepo.SaveChangesAsync();
        }
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
            var requests = await _requestApprovalTaskRepo.GetAllAsync() ?? new List<RequestApprovalTask>();
            var users = await _userRepo.GetAllAsync() ?? new List<User>();
            var flows = await _approvalFlowRepo.GetAllAsync() ?? new List<ApprovalFlow>();
            var positions = await _positionRepo.GetAllAsync() ?? new List<Position>();

            var result = new List<ViewKPIForApprovalDto>();

            foreach (var req in requests)
            {
                if (req == null) continue;

                // 🟩 Lấy user (có thể null)
                var user = users.FirstOrDefault(u => u.Id == req.RequesterId);
                string userName = user?.FullName ?? string.Empty;

                // 🟩 Lấy flow (có thể null)
                var flow = flows.FirstOrDefault(f => f.Id == req.Id);
                Guid? decisionMaker = req.DecisionMaker ?? flow?.DecisionMaker;

                // 🟩 Lấy PositionId từ flow hoặc user
                Guid? positionId = flow?.PositionId ?? user?.PositionId;
                var position = positionId != null
                    ? positions.FirstOrDefault(p => p.Id == positionId)
                    : null;
                string positionName = position?.Name ?? string.Empty;
                int step = req.Step ?? 1;
                var dto = new ViewKPIForApprovalDto
                {
                    Id = req.Id,
                    SubmitId = req.SubmitId,
                    Step = step,
                    Title = req.Title ?? string.Empty,
                    CreatedBy = req.RequesterId,
                    CreatedByName = userName,
                    CreatedDate = req.CreatedDate,
                    EApprovalStatusType = req.StatusType,
                    Position = positionName,
                    PositionId = positionId ?? Guid.Empty,
                    DecisionMaker = decisionMaker,
                    Month = req.Month,
                };

                result.Add(dto);
            }

            return result.ToList();
        }
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

            // 1. Lấy tất cả TotalScore theo submitId, full (month/quarter/half)
            var totalScores = (await _kpiTotalScoreRepo.GetAllAsync())
                               .Where(ts => ts.SubmitId == submitId && ts.IsActive)
                               .OrderBy(ts => ts.CreatedDate) // cũ nhất lên trên
                               .ToList();

            if (!totalScores.Any())
                return result;

            var totalScoreIds = totalScores.Select(ts => ts.Id).ToList();

            // 2. Lấy tất cả DetailScore nhưng chỉ cho các TotalScore có Month != null
            var detailScoresAll = (await _kpiDetailScoreRepo.GetAllAsync())
                                .Where(d => totalScoreIds.Contains(d.TotalScoreId) && d.IsActive)
                                .ToList();

            // Lọc TotalScore có Month != null để lấy DetailScore
            var totalScoresWithMonth = totalScores.Where(ts => ts.Month.HasValue).Select(ts => ts.Id).ToList();
            var detailScores = detailScoresAll.Where(d => totalScoresWithMonth.Contains(d.TotalScoreId)).ToList();

            // 3. Lấy tất cả task và owner liên quan cho DetailScore
            var taskIds = detailScores.Select(d => d.TaskId).Distinct().ToList();
            var tasks = (await _kpiTaskRepo.GetAllAsync()).Where(t => taskIds.Contains(t.Id)).ToList();

            var ownerIds = tasks.Select(t => t.OwnerId).Distinct().ToList();
            var owners = (await _userRepo.GetAllAsync()).Where(u => ownerIds.Contains(u.Id)).ToList();

            var formulaIds = tasks.Select(t => t.FormulaId).Distinct().ToList();
            var formulas = (await _formulaRepo.GetAllAsync()).Where(f => formulaIds.Contains(f.Id)).ToList();

            // 4. Map TotalScore full
            foreach (var ts in totalScores)
            {
                result.TotalScore.Add(new TotalScoreDto
                {
                    Id = ts.Id,
                    Title = ts.Title,
                    KPIScore = ts.KPIScore,
                    KeyTaskScore = ts.KeyTaskScore,
                    OMGScore = ts.OMGScore,
                    CreatedBy = ts.CreatedBy,
                    Month = ts.Month,
                    Quarter = ts.Quarter,
                    HalfYear = ts.HalfYear,
                    Year = ts.Year,
                    CreatedDate = ts.CreatedDate,
                    Status = ts.Status,
                    TotaleScore = ts.TotaleScore ?? 0,
                    IsActive = ts.IsActive
                });
            }

            // 5. Map DetailScore chỉ cho TotalScore có Month != null
            foreach (var d in detailScores)
            {
                var task = tasks.FirstOrDefault(t => t.Id == d.TaskId);
                if (task == null) continue;

                var owner = owners.FirstOrDefault(u => u.Id == task.OwnerId);
                var formula = formulas.First(f => f.Id == task.FormulaId);

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
                    Unit = task.Unit,
                    OwnerName = owner?.FullName,
                    DataSource = task.DataSource,
                    Formula = formula?.FormulaName,
                    TargetValue = d.Target,
                    Achiement = d.Achievement,
                    Score = d.Score,
                    KPIFormulaType = formula.KPIFormulaType,
                    IndexWeight = d.Weight
                });
            }

            return result;
        }
        #endregion
        #region Approval or Rejected
        //public async Task<bool> HandleKpiForApproval(ApprovalInfo approvalInfo)
        //{
        //    var currentTask = await _requestApprovalTaskRepo
        //        .FirstOrDefaultAsync(x => x.Id == approvalInfo.RequestTaskId && x.SubmitId == approvalInfo.SubmitId);

        //    if (currentTask == null)
        //        return false;

        //    if (approvalInfo.IsApproved)
        //    {
        //        currentTask.StatusType = EApprovalStatusType.Approval;
        //        currentTask.ApprovedBy = approvalInfo.DecisionMaker;
        //        currentTask.ApprovalDate = DateTime.Now;
        //        currentTask.Comment = approvalInfo.Comments;
        //    }
        //    else
        //    {
        //        currentTask.StatusType = EApprovalStatusType.Rejected;
        //        currentTask.RejectBy = approvalInfo.DecisionMaker;
        //        currentTask.RejectDate = DateTime.Now;
        //        currentTask.Comment = approvalInfo.Comments;
        //    }
        //    await _requestApprovalTaskRepo.SaveChangesAsync();
        //    var allTasks = await _requestApprovalTaskRepo.GetListAsync(x => x.SubmitId == approvalInfo.SubmitId);
        //    if (allTasks.Any(x => x.StatusType == EApprovalStatusType.Rejected))
        //    {
        //        var kpiTotalList = await _kpiTotalScoreRepo.GetListAsync(x => x.SubmitId == approvalInfo.SubmitId);
        //        if (kpiTotalList != null && kpiTotalList.Any())
        //        {
        //            foreach (var kpiTotal in kpiTotalList)
        //            {
        //                kpiTotal.Status = StatusKPIRequestType.Rejected;
        //            }

        //            await _kpiTotalScoreRepo.SaveChangesAsync();
        //        }
        //        var kpiRequest = await _kpiRequestRepo.FirstOrDefaultAsync(x => x.SubmitId == approvalInfo.SubmitId);
        //        if (kpiRequest != null)
        //        {
        //            kpiRequest.RequestStatus = StatusKPIRequestType.Rejected;
        //            kpiRequest.RejectDate = DateTime.Now;
        //            var lastStepTask = allTasks.OrderByDescending(x => x.Step).FirstOrDefault();
        //            if (lastStepTask != null)
        //            {
        //                kpiRequest.RejectBy = lastStepTask.DecisionMaker;
        //            }
        //        }
        //        await _kpiRequestRepo.SaveChangesAsync();
        //        return true;
        //    }
        //    if (approvalInfo.IsApproved)
        //    {
        //        var nextTask = allTasks
        //            .FirstOrDefault(x => x.Step == currentTask.Step + 1 && x.StatusType == EApprovalStatusType.Pending);

        //        if (nextTask != null)
        //        {
        //            nextTask.StatusType = EApprovalStatusType.InReview; 
        //            await _requestApprovalTaskRepo.SaveChangesAsync();
        //        }
        //    }
        //    if (allTasks.All(x => x.StatusType == EApprovalStatusType.Approval))
        //    {
        //        var kpiTotalList = await _kpiTotalScoreRepo.GetListAsync(x => x.SubmitId == approvalInfo.SubmitId);
        //        if (kpiTotalList != null && kpiTotalList.Any())
        //        {
        //            foreach (var kpiTotal in kpiTotalList)
        //            {
        //                kpiTotal.Status = StatusKPIRequestType.Approval;
        //            }

        //            await _kpiTotalScoreRepo.SaveChangesAsync();
        //        }
        //        var kpiRequest = await _kpiRequestRepo.FirstOrDefaultAsync(x => x.SubmitId == approvalInfo.SubmitId);
        //        if (kpiRequest != null)
        //        {
        //            kpiRequest.RequestStatus = StatusKPIRequestType.Approval;
        //            kpiRequest.ApprovalDate = DateTime.Now;
        //            var lastStepTask = allTasks.OrderByDescending(x => x.Step).FirstOrDefault();
        //            if (lastStepTask != null)
        //            {
        //                kpiRequest.ApprovalBy = lastStepTask.DecisionMaker;
        //            }

        //        }    

        //        await _kpiRequestRepo.SaveChangesAsync();
        //    }
        //    return true;
        //}
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
        #region Result KPI
        public async Task<List<ViewResultKpiDto>> ResultAllKpi()
        {
            var query = from request in await _kpiRequestRepo.GetQueryableAsync()
                        join totalScore in await _kpiTotalScoreRepo.GetQueryableAsync()
                        on request.SubmitId equals totalScore.SubmitId

                        join user in await _userRepo.GetQueryableAsync()
                        on request.CreatedBy equals user.Id

                        join department in await _departmentRepo.GetQueryableAsync()
                        on user.DepartmentId equals department.Id
                        into dept from department in dept.DefaultIfEmpty()

                        join factory in await _factoryRepo.GetQueryableAsync()
                        on user.FactoryId equals factory.Id
                        into fact from factory in fact.DefaultIfEmpty()

                        join approval in await _userRepo.GetQueryableAsync()
                        on request.ApprovalBy equals approval.Id

                        where totalScore.Status == StatusKPIRequestType.Approved 
                        && request.RequestStatus == StatusKPIRequestType.Approved

                        select new ViewResultKpiDto
                        {
                            SubmitId = request.SubmitId,
                            UserName = user.FullName,
                            Approver = approval.FullName,
                            Status = totalScore.Status,
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
                            Description =request.Description,
                            DepartmentId =user.DepartmentId,
                            FactoryId = user.FactoryId,
                            Department =department.Name,
                            Factory = factory.Name,
                            kPITotalScoreType = totalScore.kPITotalScoreType,
                        };
            var result = query.ToList();
            if(result.Count > 0)
            {
                return result;
            }
            return new List<ViewResultKpiDto>();
        }
        #endregion
        #region CalculateQuarterPoints
        public  async Task<bool> CalculateQuarterPoint(List<CalculateQuarterPointDto> calculateQuarterPoint)
        {
            var returnSave = 0;
            var totalScoresInserted = new List<KPITotalScore>();
            var SubmitId = Guid.NewGuid();
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    await CalculatePointQuarterAsync(SubmitId, calculateQuarterPoint);
                    await CalculateQuarterPointHalfYearAsync(SubmitId, calculateQuarterPoint);
    
                    await transaction.CommitAsync();
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }

            return true; // 

        }
        private async Task CalculatePointQuarterAsync( Guid submitId, List<CalculateQuarterPointDto> calculateQuarterPoint)
        {
            var dto = calculateQuarterPoint.FirstOrDefault(x => x.Quarter.HasValue && x.Quarter > 0);
            if (dto == null) return;

            var kpiScore = dto.KPIScore;
            var keyTaskScore = dto.KeyTaskScore;
            var omgScore = dto.OMGScore;
            var request = new KPIRequest
            {
                Id = Guid.NewGuid(),
                SubmitId = submitId,
                Title = dto.Title + "-(HR Calculate Point Quarter)" ?? "",
                RequestStatus = StatusKPIRequestType.Approved,
                CreatedBy = dto.CreatedBy,
                CreatedDate = DateTime.Now,
                ApprovalBy = dto.ApprovedBy,
                ApprovalDate = DateTime.Now,
                Note = "HR Calculate Point Quarter",
                Description = "HR Calculate Point Quarter"
            };
            await _kpiRequestRepo.InsertAsync(request);
            var score = new KPITotalScore
            {
                Id = Guid.NewGuid(),
                SubmitId = submitId,
                KeyTaskScore = keyTaskScore,
                KPIScore = kpiScore,
                OMGScore = omgScore,
                TotaleScore = dto.TotaleScore,
                Status = StatusKPIRequestType.Approved,
                Quarter = dto.Quarter.Value,
                Year = dto.Year,
                CreatedBy = dto.CreatedBy,
                CreatedDate = DateTime.Now,
                IsActive = true,
                Title = dto.Title+"-(HR Calculate Point Quarter)" ?? "",
                kPITotalScoreType = KPITotalScoreType.HR
            };
            await _kpiTotalScoreRepo.InsertAsync(score);
            await _kpiTotalScoreRepo.SaveChangesAsync();
        }
        private async Task CalculateQuarterPointHalfYearAsync(Guid submitId, List<CalculateQuarterPointDto> calculateQuarterPoint)
        {
            var dto = calculateQuarterPoint.FirstOrDefault(x => x.HalfYear.HasValue && x.HalfYear > 0);
            if (dto == null) return;
            var score = new KPITotalScore
            {
                Id = Guid.NewGuid(),
                SubmitId = submitId,
                KeyTaskScore = dto.KeyTaskScore,
                KPIScore = dto.KPIScore,
                OMGScore = dto.OMGScore,
                TotaleScore = dto.TotaleScore,
                Status = StatusKPIRequestType.Approved,
                HalfYear = dto.HalfYear.Value,
                Year = dto.Year,
                CreatedBy = dto.CreatedBy,
                CreatedDate = DateTime.Now,
                IsActive = true,
                Title = dto.Title + "-(HR Calculate Point HalfYear)" ?? "",
                kPITotalScoreType = KPITotalScoreType.HR
            };
            await _kpiTotalScoreRepo.InsertAsync(score);
            await _kpiTotalScoreRepo.SaveChangesAsync();
        }
        #endregion

    }
}
