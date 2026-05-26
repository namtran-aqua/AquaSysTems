using AquaSolution.Data.Data.Entities.Admin;
using AquaSolution.Data.Data.Entities.Scraps;
using AquaSolution.Data.Repositories;
using AquaSolution.Shared.ScrapManagement.FlowApprovals;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NPOI.SS.Formula.Functions;

namespace AquaSolution.Server.Services.ScrapManagetment.FlowApprovalServices
{
    public class FlowApprovalService : IFlowApprovalService
    {
        private readonly IRepository<FlowApprovalScrap> _flowApprovalScrapRepository;
        private readonly IRepository<Department> _departmentRepository;
        private readonly IRepository<Factory> _factoryRepository;
        private readonly IRepository<User> _userRepository;

        public FlowApprovalService(
            IRepository<FlowApprovalScrap> flowApprovalScrapRepository,
            IRepository<Department> departmentRepository,
            IRepository<User> userRepository,

            IRepository<Factory> factoryRepository)
        {
            _flowApprovalScrapRepository = flowApprovalScrapRepository;
            _factoryRepository = factoryRepository;
            _departmentRepository = departmentRepository;
            _userRepository = userRepository;
        }

        public async Task<FlowApprovalResponse> GetFlowApprovalAsync(Guid departmentId, Guid factoryId)
        {
            var department = await _departmentRepository.FirstOrDefaultAsync(x => x.Id == departmentId);
            var factory = await _factoryRepository.FirstOrDefaultAsync(x => x.Id == factoryId);

            if (department == null || factory == null)
                return null;

            var steps = await _flowApprovalScrapRepository.Query()
                .Where(x => x.DepartmentId == departmentId && x.FactoryId == factoryId)
                .OrderBy(x => x.Step)
                .Select(x => new FlowApprovalScrapDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    DecisionMaker = x.DecisionMaker,
                    Description = x.Description,
                    Step = x.Step
                })
                .ToListAsync();

            return new FlowApprovalResponse
            {
                DepartmentId = departmentId,
                DepartmentName = department.Name,
                FactoryId = factoryId,
                FactoryName = factory.Name,
                Steps = steps
            };
        }

        public async Task<List<FlowApprovalResponse>> GetAllByFactoryAsync(Guid factoryId)
        {
            var factory = await _factoryRepository.FirstOrDefaultAsync(x => x.Id == factoryId);

            if (factory == null)
                return new List<FlowApprovalResponse>();

            var steps = await _flowApprovalScrapRepository.Query()
                .Where(x => x.FactoryId == factoryId)
                .OrderBy(x => x.DepartmentId)
                .ThenBy(x => x.Step)
                .ToListAsync();

            var departmentIds = steps.Select(s => s.DepartmentId).Distinct().ToList();

            var departments = await _departmentRepository.Query()
                .Where(x => departmentIds.Contains(x.Id))
                .ToListAsync();

            var result = steps
                .GroupBy(x => x.DepartmentId)
                .Select(g =>
                {
                    var department = departments.FirstOrDefault(d => d.Id == g.Key);
                    return new FlowApprovalResponse
                    {
                        DepartmentId = g.Key,
                        DepartmentName = department?.Name ?? string.Empty,
                        FactoryId = factoryId,
                        FactoryName = factory.Name,
                        Steps = g.Select(x => new FlowApprovalScrapDto
                        {
                            Id = x.Id,
                            Name = x.Name,
                            DecisionMaker = x.DecisionMaker,
                            Description = x.Description,
                            Step = x.Step
                        }).ToList()
                    };
                })
                .ToList();

            return result;
        }

        public async Task<bool> CreateFlowApprovalAsync(CreateFlowApprovalRequest request)
        {
            try
            {
                var existed = await _flowApprovalScrapRepository.AnyAsync(
                    x => x.DepartmentId == request.DepartmentId && x.FactoryId == request.FactoryId);

                if (existed)
                    return false;

                var entities = request.Steps.Select(s => new FlowApprovalScrap
                {
                    Id = Guid.NewGuid(),
                    Name = s.Name,
                    DepartmentId = request.DepartmentId,
                    FactoryId = request.FactoryId,
                    DecisionMaker = s.DecisionMaker,
                    Description = s.Description,
                    Step = s.Step
                }).ToList();

                await _flowApprovalScrapRepository.InsertRangeAsync(entities);
                await _flowApprovalScrapRepository.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateFlowStepAsync(Guid id, UpdateFlowStepRequest request)
        {
            var entity = await _flowApprovalScrapRepository.GetByIdAsync(id);

            if (entity == null)
                return false;

            entity.DecisionMaker = request.DecisionMaker;
            entity.Step = request.Step;

            return await _flowApprovalScrapRepository.UpdateAsync(entity);
        }

        public async Task<bool> DeleteFlowStepAsync(Guid id)
        {
            var entity = await _flowApprovalScrapRepository.GetByIdAsync(id);

            if (entity == null)
                return false;

            return await _flowApprovalScrapRepository.DeleteAsync(entity);
        }

        public async Task<bool> DeleteFlowApprovalAsync(Guid departmentId, Guid factoryId)
        {
            try
            {
                var entities = await _flowApprovalScrapRepository.Query()
                    .Where(x => x.DepartmentId == departmentId && x.FactoryId == factoryId)
                    .ToListAsync();

                if (!entities.Any())
                    return false;

                _flowApprovalScrapRepository.RemoveRange(entities);
                await _flowApprovalScrapRepository.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }
        public async Task<List<FlowApprovalResponse>> GetAllAsync()
        {
            var steps = await _flowApprovalScrapRepository.Query()
                .OrderBy(x => x.FactoryId)
                .ThenBy(x => x.DepartmentId)
                .ThenBy(x => x.Step)
                .ToListAsync();

            var factoryIds = steps.Select(s => s.FactoryId).Distinct().ToList();
            var departmentIds = steps.Select(s => s.DepartmentId).Distinct().ToList();
            var userIds = steps.Select(s => s.DecisionMaker).Distinct().ToList();

            var factories = await _factoryRepository.Query()
                .Where(x => factoryIds.Contains(x.Id))
                .ToListAsync();

            var departments = await _departmentRepository.Query()
                .Where(x => departmentIds.Contains(x.Id))
                .ToListAsync();

            var users = await _userRepository.Query()
                .Where(x => userIds.Contains(x.Id))
                .ToListAsync();

            var result = steps
                .GroupBy(x => new { x.FactoryId, x.DepartmentId })
                .Select(g =>
                {
                    var factory = factories.FirstOrDefault(f => f.Id == g.Key.FactoryId);
                    var department = departments.FirstOrDefault(d => d.Id == g.Key.DepartmentId);
                    return new FlowApprovalResponse
                    {
                        FactoryId = g.Key.FactoryId,
                        FactoryName = factory?.Name ?? string.Empty,
                        DepartmentId = g.Key.DepartmentId,
                        DepartmentName = department?.Name ?? string.Empty,
                        Steps = g.Select(x =>
                        {
                            var user = users.FirstOrDefault(u => u.Id == x.DecisionMaker);
                            return new FlowApprovalScrapDto
                            {
                                Id = x.Id,
                                Name = x.Name,
                                DecisionMaker = x.DecisionMaker,
                                DecisionMakerName = user?.FullName ?? string.Empty,
                                DecisionMakerWorkId = user?.WorkDayId ?? string.Empty,
                                DecisionMakerEmail = user?.Email ?? string.Empty,
                                Description = x.Description,
                                Step = x.Step
                            };
                        }).ToList()
                    };
                })
                .ToList();

            return result;
        }
    }
}