using AquaSolution.Data.Data.Entities;
using AquaSolution.Data.Repositories;
using AquaSolution.Shared.ApprovalFlows;

namespace AquaSolution.Server.Services.ApprovalFlowService
{
    public class ApprovalFlowService : IApprovalFlowService
    {
        private readonly IRepository<ApprovalFlow> _approvalFlowRepo;
        private readonly IRepository<Position> _positionRepo;
        private readonly IRepository<User> _userRepo;

        public ApprovalFlowService(IRepository<ApprovalFlow> approvalFlowRepo, 
            IRepository<Position> positionRepo,
             IRepository<User> userRepo
            )
        {
            _approvalFlowRepo = approvalFlowRepo;
            _positionRepo = positionRepo;
            _userRepo = userRepo;
        }
        public async Task<bool> CreatedApprovalFlow(ApprovalFlowDto approvalFlowDto)
        {
            var approvalFlow = new ApprovalFlow
            {
                Id = Guid.NewGuid(),
                Name = approvalFlowDto.Name,
                UserApproveId = approvalFlowDto.UserApproveId,
                PositionId = approvalFlowDto.PositionId,
                DesCription = approvalFlowDto.DesCription,
                CurrentStep = approvalFlowDto.CurrentStep,
                NextStep = approvalFlowDto.NextStep,
                System = approvalFlowDto.System,
                ApprovalSettingType = approvalFlowDto.ApprovalSettingType,
                CreatedDate = DateTime.Now,
            };
            await _approvalFlowRepo.InsertAsync(approvalFlow);
            var isSave = await _approvalFlowRepo.SaveChangesAsync();
            if (isSave == 0) return false;
            return true;
        }

        public async Task<bool> DeleteApprovalFlow(Guid approvalFlowId)
        {
           var approvalFlow = await _approvalFlowRepo.GetByIdAsync(approvalFlowId);
            if(approvalFlow == null) return false;
            return await _approvalFlowRepo.DeleteAsync(approvalFlow);
        }

        public async Task<List<ApprovalFlowDto>> GetListApprovalFlow()
        {
            var approvalFlowData = from approvalFlow in await _approvalFlowRepo.GetQueryableAsync()

                                   join position in await _positionRepo.GetQueryableAsync()
                                   on approvalFlow.PositionId equals position.Id

                                   join user in await _userRepo.GetQueryableAsync()
                                   on approvalFlow.UserApproveId equals user.Id
                                   into u1 from user in u1.DefaultIfEmpty()
                              select new ApprovalFlowDto
                              {
                                  Id = Guid.NewGuid(),
                                  Name = approvalFlow.Name,
                                  UserApproveId = approvalFlow.UserApproveId,
                                  PositionId = approvalFlow.PositionId,
                                  DesCription = approvalFlow.DesCription,
                                  CurrentStep = approvalFlow.CurrentStep,
                                  NextStep = approvalFlow.NextStep,
                                  System = approvalFlow.System,
                                  ApprovalSettingType = approvalFlow.ApprovalSettingType,
                                  CreatedDate = approvalFlow.CreatedDate,
                                  PositionName =position.Name,
                                  UserApproveName = user.FullName
                              };
            var dataReturn =  approvalFlowData.ToList();
            if (dataReturn.Count > 0)
            {
                return dataReturn;
            }
            return new List<ApprovalFlowDto>();
        }

        public async Task<bool> UpdateApprovalFlow(ApprovalFlowDto approvalFlowDto)
        {
            var approvalFlow = await _approvalFlowRepo.GetByIdAsync(approvalFlowDto.Id);
            if(approvalFlow == null) return false;
            approvalFlow.Name = approvalFlowDto.Name;
            approvalFlow.UserApproveId = approvalFlowDto.UserApproveId;
            approvalFlow.PositionId = approvalFlowDto.PositionId;
            approvalFlow.DesCription = approvalFlowDto.DesCription;
            approvalFlow.NextStep = approvalFlowDto.NextStep;
            approvalFlow.CurrentStep = approvalFlowDto.CurrentStep;
            approvalFlow.System = approvalFlowDto.System;
            approvalFlow.ApprovalSettingType = approvalFlowDto.ApprovalSettingType;
            return await _approvalFlowRepo.UpdateAsync(approvalFlow);
        }
    }
}
