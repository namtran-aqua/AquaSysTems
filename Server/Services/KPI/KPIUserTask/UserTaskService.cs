using AquaSolution.Data.Data.Entities;
using AquaSolution.Data.KPI.Entities;
using AquaSolution.Data.Repositories;
using AquaSolution.Shared.KPI.KPIMonthlyTarget;
using AquaSolution.Shared.KPI.UserTask;

namespace AquaSolution.Server.Services.KPI.KPIUserTask
{
    public class UserTaskService : IUserTaskService
    {
        private readonly IRepository<UserTask> _userTaskRepo;
        private readonly IRepository<KPIMonthlyTarget> _KPIMonthlyTargetRepo;
        public UserTaskService(IRepository<UserTask> userTaskRepo, 
            IRepository<KPIMonthlyTarget> KPIMonthlyTargetRepo)
        {
            _userTaskRepo = userTaskRepo;
            _KPIMonthlyTargetRepo = KPIMonthlyTargetRepo;
        }

        //public async Task<bool> HandleUserTaskAndTarget(HandleUserTaskAndTargetDto HandleUserTaskAndTarget)
        //{
        //    try
        //    {
        //        //----------------------Add User Task---------------------- 
        //        if (HandleUserTaskAndTarget.HandleUserTaskDtos == null || !HandleUserTaskAndTarget.HandleUserTaskDtos.Any())
        //            return false;
        //        var userId = HandleUserTaskAndTarget.HandleUserTaskDtos.First().UserId;

        //        // Lấy danh sách taskId đã chọn, distinct
        //        var selectedTaskIds = HandleUserTaskAndTarget.HandleUserTaskDtos
        //                             .Select(x => x.TaskIds).Distinct()
        //                             .ToList();

        //        // Lấy tất cả UserTask hiện có của user
        //        var allUserTasks = await _userTaskRepo.WhereAsync(ut => ut.UserId == userId);

        //        var tasksToUpdate = new List<UserTask>();
        //        var tasksToAdd = new List<UserTask>();

        //        foreach (var userTask in allUserTasks)
        //        {
        //            var correspondingDto = HandleUserTaskAndTarget.HandleUserTaskDtos
        //                                   .FirstOrDefault(x => x.TaskIds == userTask.KPITaskId);

        //            if (selectedTaskIds.Contains(userTask.KPITaskId))
        //            {
        //                // Task được chọn, bật IsActive nếu chưa bật
        //                if (!userTask.IsActive ||
        //                    (correspondingDto != null &&
        //                    (userTask.Index != correspondingDto.Index || userTask.Weight != correspondingDto.Weight)))
        //                {
        //                    userTask.IsActive = true;
        //                    userTask.Index = correspondingDto?.Index ?? 0;
        //                    userTask.Weight = correspondingDto?.Weight ?? userTask.Weight;
        //                    userTask.CreatedDate = DateTime.Now;
        //                    tasksToUpdate.Add(userTask);
        //                }
        //                selectedTaskIds.Remove(userTask.KPITaskId);
        //            }
        //            else
        //            {
        //                if (userTask.IsActive)
        //                {
        //                    userTask.IsActive = false;
        //                    userTask.CreatedDate = DateTime.Now;
        //                    userTask.Index = correspondingDto?.Index ?? 0;
        //                    userTask.Weight = correspondingDto?.Weight ?? userTask.Weight;
        //                    tasksToUpdate.Add(userTask);
        //                }
        //            }
        //        }
        //        foreach (var taskId in selectedTaskIds)
        //        {
        //            var correspondingDto = HandleUserTaskAndTarget.HandleUserTaskDtos
        //                                     .FirstOrDefault(x => x.TaskIds == taskId);
        //            var newUserTask = new UserTask
        //            {
        //                Id = Guid.NewGuid(),
        //                UserId = userId,
        //                KPITaskId = taskId,
        //                IsActive = true,
        //                CreatedDate = DateTime.Now,
        //                Index = correspondingDto?.Index??0,
        //                Weight = correspondingDto?.Weight ?? 0m
        //            };
        //            tasksToAdd.Add(newUserTask);
        //        }

        //        // Cập nhật các UserTask
        //        foreach (var userTaskToUpdate in tasksToUpdate)
        //        {
        //            var updateResult = await _userTaskRepo.UpdateAsync(userTaskToUpdate);
        //            if (!updateResult)
        //                return false;
        //        }

        //        if (tasksToAdd.Any())
        //        {
        //            await _userTaskRepo.InsertRangeAsync(tasksToAdd);
        //            await _userTaskRepo.SaveChangesAsync();
        //        }

        //        //---- Xử lý cập nhật Target ----

        //        if (HandleUserTaskAndTarget.UpdateTargetDtos == null || !HandleUserTaskAndTarget.UpdateTargetDtos.Any())
        //            return false;

        //        foreach (var targetList in HandleUserTaskAndTarget.UpdateTargetDtos)
        //        {
        //            if (targetList == null || !targetList.Any())
        //                continue;

        //            var firstDto = targetList.First();

        //            // Tìm UserTask tương ứng đang active
        //            var userTask = await _userTaskRepo
        //                .FirstOrDefaultAsync(x => x.IsActive && x.UserId == firstDto.UserId && x.KPITaskId == firstDto.TaskId);

        //            if (userTask == null)
        //                continue;

        //            // Lấy các target đã tồn tại và xóa
        //            var existingTargets = await _KPIMonthlyTargetRepo
        //                .WhereAsync(x => x.UserId == firstDto.UserId && x.UserTaskId == userTask.Id);

        //            if (existingTargets.Any())
        //            {
        //                _KPIMonthlyTargetRepo.RemoveRange(existingTargets);
        //            }

        //            // Tạo target mới từ DTO
        //            var newTargets = targetList.Select(dto => new KPIMonthlyTarget
        //            {
        //                Id = Guid.NewGuid(),
        //                UserTaskId = userTask.Id,
        //                Month = dto.Month,
        //                Year = dto.Year,
        //                TargetValue = dto.TargetValue,
        //                CreatedDate = dto.CreatedDate,
        //                UserId = dto.UserId,
        //            }).ToList();

        //            await _KPIMonthlyTargetRepo.InsertRangeAsync(newTargets);
        //        }

        //        var result = await _KPIMonthlyTargetRepo.SaveChangesAsync();

        //        return result > 0;
        //    }
        //    catch (Exception ex)
        //    {

        //        throw;
        //    }
        //}
        public async Task<bool> HandleUserTaskAndTarget(HandleUserTaskAndTargetDto HandleUserTaskAndTarget)
        {
            try
            {
                //----------------------Add User Task---------------------- 
                if (HandleUserTaskAndTarget.HandleUserTaskDtos == null || !HandleUserTaskAndTarget.HandleUserTaskDtos.Any())
                    return false;

                var userId = HandleUserTaskAndTarget.HandleUserTaskDtos.First().UserId;

                // Lấy danh sách taskId đã chọn, distinct
                var selectedTaskIds = HandleUserTaskAndTarget.HandleUserTaskDtos
                                     .Select(x => x.TaskIds).Distinct()
                                     .ToList();

                // Lấy tất cả UserTask hiện có của user
                var allUserTasks = await _userTaskRepo.WhereAsync(ut => ut.UserId == userId);

                var tasksToUpdate = new List<UserTask>();
                var tasksToAdd = new List<UserTask>();

                foreach (var userTask in allUserTasks)
                {
                    var correspondingDto = HandleUserTaskAndTarget.HandleUserTaskDtos
                                           .FirstOrDefault(x => x.TaskIds == userTask.KPITaskId);

                    if (selectedTaskIds.Contains(userTask.KPITaskId))
                    {
                        // Task được chọn, bật IsActive nếu chưa bật hoặc cập nhật Index/Weight
                        if (!userTask.IsActive ||
                            (correspondingDto != null &&
                            (userTask.Index != correspondingDto.Index || userTask.Weight != correspondingDto.Weight)))
                        {
                            userTask.IsActive = true;
                            userTask.Index = correspondingDto?.Index ?? 0;
                            userTask.Weight = correspondingDto?.Weight ?? userTask.Weight;
                            userTask.CreatedDate = DateTime.Now;
                            tasksToUpdate.Add(userTask);
                        }
                        selectedTaskIds.Remove(userTask.KPITaskId);
                    }
                    else
                    {
                        if (userTask.IsActive)
                        {
                            userTask.IsActive = false;
                            userTask.CreatedDate = DateTime.Now;
                            userTask.Index = correspondingDto?.Index ?? 0;
                            userTask.Weight = correspondingDto?.Weight ?? userTask.Weight;
                            tasksToUpdate.Add(userTask);
                        }
                    }
                }

                foreach (var taskId in selectedTaskIds)
                {
                    var correspondingDto = HandleUserTaskAndTarget.HandleUserTaskDtos
                                             .FirstOrDefault(x => x.TaskIds == taskId);
                    var newUserTask = new UserTask
                    {
                        Id = Guid.NewGuid(),
                        UserId = userId,
                        KPITaskId = taskId,
                        IsActive = true,
                        CreatedDate = DateTime.Now,
                        Index = correspondingDto?.Index ?? 0,
                        Weight = correspondingDto?.Weight ?? 0m
                    };
                    tasksToAdd.Add(newUserTask);
                }

                // Cập nhật UserTask
                foreach (var userTaskToUpdate in tasksToUpdate)
                {
                    var updateResult = await _userTaskRepo.UpdateAsync(userTaskToUpdate);
                    if (!updateResult)
                        return false;
                }

                if (tasksToAdd.Any())
                {
                    await _userTaskRepo.InsertRangeAsync(tasksToAdd);
                    await _userTaskRepo.SaveChangesAsync();
                }

                //---- Xử lý cập nhật Target ----
                if (HandleUserTaskAndTarget.UpdateTargetDtos == null || !HandleUserTaskAndTarget.UpdateTargetDtos.Any())
                    return false;

                foreach (var targetList in HandleUserTaskAndTarget.UpdateTargetDtos)
                {
                    if (targetList == null || !targetList.Any())
                        continue;

                    var firstDto = targetList.First();

                    // Tìm UserTask tương ứng đang active
                    var userTask = await _userTaskRepo
                        .FirstOrDefaultAsync(x => x.IsActive && x.UserId == firstDto.UserId && x.KPITaskId == firstDto.TaskId);

                    if (userTask == null)
                        continue;

                    foreach (var dto in targetList)
                    {
                        // Kiểm tra xem target đã tồn tại chưa
                        var existingTarget = await _KPIMonthlyTargetRepo
                            .FirstOrDefaultAsync(x => x.UserId == dto.UserId
                                                    && x.UserTaskId == userTask.Id
                                                    && x.Month == dto.Month
                                                    && x.Year == dto.Year);

                        if (existingTarget != null)
                        {
                            // Update TargetValue nếu đã tồn tại
                            existingTarget.TargetValue = dto.TargetValue;
                            existingTarget.CreatedDate = dto.CreatedDate;
                            await _KPIMonthlyTargetRepo.UpdateAsync(existingTarget);
                        }
                        else
                        {
                            // Chưa tồn tại -> insert mới
                            var newTarget = new KPIMonthlyTarget
                            {
                                Id = Guid.NewGuid(),
                                UserTaskId = userTask.Id,
                                Month = dto.Month,
                                Year = dto.Year,
                                TargetValue = dto.TargetValue,
                                CreatedDate = dto.CreatedDate,
                                UserId = dto.UserId
                            };
                            await _KPIMonthlyTargetRepo.InsertAsync(newTarget);
                        }
                    }
                }
                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<List<UserTaskDto>> GetListByUserIdAsync(Guid userId)
        {
            var query =from userTask in await _userTaskRepo.GetQueryableAsync()
                        where userTask.UserId == userId && userTask.IsActive
                        select new UserTaskDto
                        {
                            Id = userTask.Id,
                            UserId = userTask.UserId,
                            KPITaskId = userTask.KPITaskId,
                            IsActive = userTask.IsActive,
                            Index = userTask.Index,
                            CreatedDate = userTask.CreatedDate
                        };
            return query
                .ToList();

        }
    }
}
