using AquaSolution.Data.Connection;
using AquaSolution.Data.Data.Entities;
using AquaSolution.Data.Data.Entities.KPI;
using AquaSolution.Data.KPI.Entities;
using AquaSolution.Data.Repositories;
using AquaSolution.Shared.Enum;
using AquaSolution.Shared.Enum.KPIType;
using AquaSolution.Shared.KPI.KPIActual;
using AquaSolution.Shared.KPI.KPISubmit;
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
                 AquaDbContext context
            )
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
        }
        public async Task<List<HandleKPISubmitDto>> GetHandleKPISubmitByUserId(Guid userId, int year, int? month)
        {

            if (!month.HasValue)
            {
                return new List<HandleKPISubmitDto>();
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
                var actuals = await _KPIMonthlyActualRepo.GetListAsync(a => targetIds.Contains(a.KPIMonthlyTargetId));
                var actualTargetIds = actuals.Select(a => a.KPIMonthlyTargetId).ToHashSet();

                var users = await _userRepo.GetAllAsync();
                var formulas = await _formulaRepo.GetAllAsync();
                var distinctIndexWeights = kpiIndexWeights
                        .GroupBy(iw => new { iw.PositionType, iw.KPIIndexType })
                        .Select(g => g.First(x=>x.PeriodType == PeriodType.Month))
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
                              where !actualTargetIds.Contains(target.Id)
                              select new HandleKPISubmitDto
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
                              select new HandleKPISubmitDto
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
                                  TotaleScore = totalScore.TotaleScore,
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
        //public async Task<bool> SubmitKPIAsync(List<HandleKPISubmitDto> submitKPIDto)
        //{
        //    var returnSave = 0;
        //    var totalScoresInserted = new List<KPITotalScore>();

        //    var SubmitId = Guid.NewGuid();

        //    using (var transaction = await _context.Database.BeginTransactionAsync())
        //    {
        //        try
        //        {
        //            //------------------------------------Quarter------------------------------------
        //            var quarterDto = submitKPIDto.FirstOrDefault(x => x.Quarter.HasValue && x.Quarter > 0);
        //            if (quarterDto != null)
        //            {
        //                var KpiScore = submitKPIDto?.FirstOrDefault(x => x.KPIIndexType == KPIIndexType.KPI)?.KPIScore ??0;
        //                var KeyTaskScore = submitKPIDto?.FirstOrDefault(x => x.KPIIndexType == KPIIndexType.KeyTask)?.KeyTaskScore ??0;
        //                var OMGScore = submitKPIDto?.FirstOrDefault(x => x.KPIIndexType == KPIIndexType.OMG)?.OMGScore ??0;
        //                var requestKPIQuarter = new KPIRequest
        //                {
        //                    Id = Guid.NewGuid(),
        //                    SubmitId = SubmitId,
        //                    Title = quarterDto.HeaderTitle ?? string.Empty,
        //                    RequestStatus = StatusKPIRequestType.WaitingForApproval,
        //                    CreatedBy = quarterDto.CreatedBy,
        //                    CreatedDate = DateTime.Now,
        //                };
        //                await _kpiRequestRepo.InsertAsync(requestKPIQuarter);
        //             //   returnSave += await _kpiRequestRepo.SaveChangesAsync();

        //                var kpiTotalScoreQuarter = new KPITotalScore
        //                {
        //                    Id = Guid.NewGuid(),
        //                    SubmitId = SubmitId,
        //                    KeyTaskScore = KeyTaskScore,
        //                    KPIScore = KpiScore,
        //                    OMGScore = OMGScore,
        //                    TotaleScore = quarterDto.TotaleScore ?? 0,
        //                    Status = StatusKPIRequestType.WaitingForApproval,
        //                    Quarter = quarterDto.Quarter.Value,
        //                    Year = quarterDto.Year,
        //                    HalfYear = quarterDto.HalfYear ?? 0,
        //                    CreatedDate = DateTime.Now,
        //                    CreatedBy = quarterDto.CreatedBy,
        //                    IsActive = true,
        //                    Title = quarterDto.HeaderTitle ?? string.Empty
        //                };
        //                await _kpiTotalScoreRepo.InsertAsync(kpiTotalScoreQuarter);
        //             //   returnSave += await _kpiTotalScoreRepo.SaveChangesAsync();
        //                totalScoresInserted.Add(kpiTotalScoreQuarter);
        //            }

        //            //------------------------------------HaflYear------------------------------------
        //            var halfYearDto = submitKPIDto.FirstOrDefault(x => x.HalfYear.HasValue && x.HalfYear > 0 && !x.Quarter.HasValue);
        //            if (halfYearDto != null)
        //            {
        //                var requestKPIHalfYear = new KPIRequest
        //                {
        //                    Id = Guid.NewGuid(),
        //                    SubmitId = SubmitId,
        //                    Title = halfYearDto.HeaderTitle ?? string.Empty,
        //                    RequestStatus = StatusKPIRequestType.WaitingForApproval,
        //                    CreatedBy = halfYearDto.CreatedBy,
        //                    CreatedDate = DateTime.Now,
        //                };
        //                await _kpiRequestRepo.InsertAsync(requestKPIHalfYear);
        //              //  returnSave += await _kpiRequestRepo.SaveChangesAsync();

        //                var kpiTotalScoreHalfYear = new KPITotalScore
        //                {
        //                    Id = Guid.NewGuid(),
        //                    SubmitId = SubmitId,
        //                    KeyTaskScore = halfYearDto.KeyTaskScore ?? 0,
        //                    KPIScore = halfYearDto.KPIScore ?? 0,
        //                    OMGScore = halfYearDto.OMGScore ?? 0,
        //                    TotaleScore = halfYearDto.TotaleScore ?? 0,
        //                    Status = StatusKPIRequestType.WaitingForApproval,
        //                    HalfYear = halfYearDto.HalfYear.Value,
        //                    Year = halfYearDto.Year,
        //                    CreatedDate = DateTime.Now,
        //                    CreatedBy = halfYearDto.CreatedBy,
        //                    IsActive = true,
        //                    Title = halfYearDto.HeaderTitle ?? string.Empty
        //                };
        //                await _kpiTotalScoreRepo.InsertAsync(kpiTotalScoreHalfYear);
        //             //   returnSave += await _kpiTotalScoreRepo.SaveChangesAsync();
        //                totalScoresInserted.Add(kpiTotalScoreHalfYear);
        //            }

        //            // ---------------------------Monthly---------------------------
        //            var monthGroups = submitKPIDto
        //                .Where(x => x.Month.HasValue && x.Month > 0)
        //                .GroupBy(x => new { x.Month, x.Year })
        //                .ToList();
        //            var KpiScoreMonth = submitKPIDto?.Sum(x => x.KPIScore) ?? 0;
        //            var KeyTaskScoreMonth = submitKPIDto?.Sum(x => x.KeyTaskScore) ?? 0;
        //            var OMGScoreMonth = submitKPIDto?.Sum(x => x.OMGScore) ?? 0;
        //            foreach (var group in monthGroups)
        //            {
        //                var monthDto = group.First();
        //                var requestKPIMonth = new KPIRequest
        //                {
        //                    Id = Guid.NewGuid(),
        //                    SubmitId = SubmitId,
        //                    Title = monthDto.HeaderTitle ?? string.Empty,
        //                    RequestStatus = StatusKPIRequestType.WaitingForApproval,
        //                    CreatedBy = monthDto.CreatedBy,
        //                    CreatedDate = DateTime.Now,
        //                };
        //                await _kpiRequestRepo.InsertAsync(requestKPIMonth);
        //            //    returnSave += await _kpiRequestRepo.SaveChangesAsync();

        //                var kpiTotalScoreMonth = new KPITotalScore
        //                {
        //                    Id = Guid.NewGuid(),

        //                    SubmitId = SubmitId,
        //                    KeyTaskScore = KeyTaskScoreMonth,
        //                    KPIScore = KpiScoreMonth,
        //                    OMGScore = OMGScoreMonth,
        //                    TotaleScore = monthDto.TotaleScore ?? 0,
        //                    Status = StatusKPIRequestType.WaitingForApproval,
        //                    Month = monthDto.Month.Value,
        //                    Year = monthDto.Year,
        //                    CreatedDate = DateTime.Now,
        //                    CreatedBy = monthDto.CreatedBy,
        //                    IsActive = true,
        //                    Title = monthDto.HeaderTitle ?? string.Empty
        //                };
        //                await _kpiTotalScoreRepo.InsertAsync(kpiTotalScoreMonth);
        //              //  returnSave += await _kpiTotalScoreRepo.SaveChangesAsync();
        //                totalScoresInserted.Add(kpiTotalScoreMonth);
        //            }

        //            // Insert KPIDetailScore và KPIMonthlyActual cho từng bản ghi tháng (mapping đúng TotalScoreId)
        //            var listMonthActual = submitKPIDto.Where(x => x.Month != null).ToList();

        //            foreach (var item in listMonthActual)
        //            {
        //                var totalScore = totalScoresInserted.FirstOrDefault(ts =>
        //                    ts.Month == item.Month &&
        //                    ts.Year == item.Year);

        //                if (totalScore == null)
        //                {
        //                    continue;
        //                }

        //                var kpiDetailScore = new KPIDetailScore
        //                {
        //                    Id = Guid.NewGuid(),
        //                    TotalScoreId = totalScore.Id,
        //                    TaskId = item.TaskId,
        //                    Max = item.Max ?? 0,
        //                    Bottom = item.Bottom ?? 0,
        //                    Weight = item.Weight ?? 0,
        //                    Target = item.TargetValue ?? 0,
        //                    Achievement = item.Achiement ?? 0,
        //                    Actual = item.ActualValue ?? 0,
        //                    Score = item.Score ?? 0,
        //                    Month = item.Month,
        //                    Quarter = item.Quarter,
        //                    HalfYear = item.HalfYear,
        //                    Year = item.Year,
        //                    CreatedDate = DateTime.Now,
        //                    IsActive = true
        //                };
        //                await _kpiDetailScoreRepo.InsertAsync(kpiDetailScore);
        //              //  returnSave += await _kpiDetailScoreRepo.SaveChangesAsync();

        //                var actual = new KPIMonthlyActual
        //                {
        //                    Id = Guid.NewGuid(),
        //                    KPIMonthlyTargetId = item.TargetId,
        //                    KPITotalScoreId = totalScore.Id,
        //                    Month = item.Month,
        //                    Year = item.Year,
        //                    ActualValue = item.ActualValue,
        //                    CreatedDate = DateTime.Now,
        //                    CreatedBy = item.CreatedBy,
        //                    UpdatedDate = DateTime.Now,
        //                    UpdatedBy = item.CreatedBy
        //                };
        //                await _KPIMonthlyActualRepo.InsertAsync(actual);
        //             //   returnSave += await _KPIMonthlyActualRepo.SaveChangesAsync();
        //            }

        //            await transaction.CommitAsync();
        //        }
        //        catch
        //        {
        //            await transaction.RollbackAsync();
        //            throw; // hoặc return false nếu bạn muốn swallow exception
        //        }
        //    }

        //    return returnSave > 0;
        //}
        //-------------------------------------------------
        public async Task<bool> SubmitKPIAsync(List<HandleKPISubmitDto> submitKPIDto)
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
        private async Task InsertQuarterAsync(List<HandleKPISubmitDto> dtos, Guid submitId, List<KPITotalScore> totalScores)
        {
            var dto = dtos.FirstOrDefault(x => x.Quarter.HasValue && x.Quarter > 0);
            if (dto == null) return;

            var kpiScore = dtos.FirstOrDefault(x => x.KPIIndexType == KPIIndexType.KPI)?.KPIScore ?? 0;
            var keyTaskScore = dtos.FirstOrDefault(x => x.KPIIndexType == KPIIndexType.KeyTask)?.KeyTaskScore ?? 0;
            var omgScore = dtos.FirstOrDefault(x => x.KPIIndexType == KPIIndexType.OMG)?.OMGScore ?? 0;

            //var request = new KPIRequest
            //{
            //    Id = Guid.NewGuid(),
            //    SubmitId = submitId,
            //    Title = dto.HeaderTitle ?? "",
            //    RequestStatus = StatusKPIRequestType.WaitingForApproval,
            //    CreatedBy = dto.CreatedBy,
            //    CreatedDate = DateTime.Now
            //};
            //await _kpiRequestRepo.InsertAsync(request);
            //await _kpiRequestRepo.SaveChangesAsync();
            var score = new KPITotalScore
            {
                Id = Guid.NewGuid(),
                SubmitId = submitId,
                KeyTaskScore = keyTaskScore,
                KPIScore = kpiScore,
                OMGScore = omgScore,
                TotaleScore = dto.TotaleScore ?? 0,
                Status = StatusKPIRequestType.WaitingForApproval,
                Quarter = dto.Quarter.Value,
                Year = dto.Year,
                HalfYear = dto.HalfYear ?? 0,
                CreatedBy = dto.CreatedBy,
                CreatedDate = DateTime.Now,
                IsActive = true,
                Title = dto.HeaderTitle ?? ""
            };
            await _kpiTotalScoreRepo.InsertAsync(score);
            await _kpiTotalScoreRepo.SaveChangesAsync();
            totalScores.Add(score);
        }
        private async Task InsertHalfYearAsync(List<HandleKPISubmitDto> dtos, Guid submitId, List<KPITotalScore> totalScores)
        {
            var dto = dtos.FirstOrDefault(x => x.HalfYear.HasValue && x.HalfYear > 0 );
            if (dto == null) return;

            //var request = new KPIRequest
            //{
            //    Id = Guid.NewGuid(),
            //    SubmitId = submitId,
            //    Title = dto.HeaderTitle ?? "",
            //    RequestStatus = StatusKPIRequestType.WaitingForApproval,
            //    CreatedBy = dto.CreatedBy,
            //    CreatedDate = DateTime.Now
            //};
            //await _kpiRequestRepo.InsertAsync(request);
            //await _kpiRequestRepo.SaveChangesAsync();
            var score = new KPITotalScore
            {
                Id = Guid.NewGuid(),
                SubmitId = submitId,
                KeyTaskScore = dto.KeyTaskScore ?? 0,
                KPIScore = dto.KPIScore ?? 0,
                OMGScore = dto.OMGScore ?? 0,
                TotaleScore = dto.TotaleScore ?? 0,
                Status = StatusKPIRequestType.WaitingForApproval,
                HalfYear = dto.HalfYear.Value,
                Year = dto.Year,
                CreatedBy = dto.CreatedBy,
                CreatedDate = DateTime.Now,
                IsActive = true,
                Title = dto.HeaderTitle ?? ""
            };
            await _kpiTotalScoreRepo.InsertAsync(score);
            await _kpiTotalScoreRepo.SaveChangesAsync();
            totalScores.Add(score);
        }
        private async Task InsertMonthlyAsync(List<HandleKPISubmitDto> dtos, Guid submitId, List<KPITotalScore> totalScores)
        {
            var groups = dtos
                .Where(x => x.Month.HasValue && x.Month != 0)
                .GroupBy(x => new { x.Month, x.Year })
                .ToList();

            var kpiScore = dtos.Where(x => x.Month != null).Sum(x => x.KPIScore ?? 0);
            var keyTaskScore = dtos.Where(x=>x.Month!=null).Sum(x => x.KeyTaskScore ?? 0);
            var omgScore = dtos.Where(x => x.Month != null).Sum(x => x.OMGScore ?? 0);

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
                    TotaleScore = dto.TotaleScore ?? 0,
                    Status = StatusKPIRequestType.WaitingForApproval,
                    Month = dto.Month.Value,
                    Year = dto.Year,
                    CreatedBy = dto.CreatedBy,
                    CreatedDate = DateTime.Now,
                    IsActive = true,
                    Title = dto.HeaderTitle ?? ""
                };
                await _kpiTotalScoreRepo.InsertAsync(score);
                await _kpiTotalScoreRepo.SaveChangesAsync();
                totalScores.Add(score);
            }
        }
        private async Task InsertDetailAndActualAsync(List<HandleKPISubmitDto> dtos, List<KPITotalScore> totalScores)
        {
            var monthlyItems = dtos.Where(x => x.Month.HasValue).ToList();

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
        public async Task<List<KPITotalScoreDto>> GetKPITotalScoreQuarterByUserId(Guid userId, int year, int? quater)
        {
            var query = from totalScore in await _kpiTotalScoreRepo.GetQueryableAsync()
                        join request in await _kpiRequestRepo.GetQueryableAsync()
                        on totalScore.SubmitId equals request.SubmitId
                        where request.RequestStatus == StatusKPIRequestType.Approved
                        && totalScore.CreatedBy == userId
                        && totalScore.Year == year
                        && (quater == null || totalScore.Quarter == quater)
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
        public async Task<List<IndexWeightDto>> GetIndexWeight(PositionType positionType , PeriodType periodType)
        {
            var query = from indexWeight in await _kpiIndexWeightRepo.GetQueryableAsync()
                        join position in await _positionRepo.GetQueryableAsync()
                        on indexWeight.PositionType equals position.Type
                        where indexWeight.PositionType ==positionType && indexWeight.PeriodType == periodType
                        select new IndexWeightDto
                        {
                            Weight = indexWeight.Weight,
                            PeriodType = indexWeight.PeriodType,
                            PositionType = indexWeight.PositionType,
                            KPIIndexType = indexWeight.KPIIndexType
                        };
            return query.ToList();
        }
    }
}
