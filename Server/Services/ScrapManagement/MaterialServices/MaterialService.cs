using AquaSolution.Data.Repositories;
using AquaSolution.Shared.ScrapManagement.Materials;
using AquaSolution.Data.Entities.Scraps;
using AquaSolution.Data.Data.Entities.Scraps;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AquaSolution.Server.Services.ScrapManagetment.MaterialServices
{
    public class MaterialService : IMaterialService
    {
        private readonly IRepository<Material> _materialRepository;
        private readonly IRepository<Weight> _weightRepository;

        public MaterialService(
            IRepository<Material> materialRepository,
            IRepository<Weight> weightRepository)
        {
            _materialRepository = materialRepository;
            _weightRepository = weightRepository;
        }

        public async Task<bool> ImportMaterialsAsync(List<ImportExcelDto> data)
        {
            if (data == null || data.Count == 0) return false;

            var materials = new List<Material>();
            var weights = new List<Weight>();

            foreach (var item in data)
            {
                var materialId = Guid.NewGuid();

                var material = new Material
                {
                    Id = materialId,
                    BOMHead = item.BOMHead ?? string.Empty,
                    BOMDescription = item.BOMDescription ?? string.Empty,
                    Code = item.Code ?? string.Empty,
                    Name = item.Name ?? string.Empty,
                    TYPE = item.TYPE ?? string.Empty,
                    Unit = item.Unit ?? string.Empty,
                    Plant = item.Plant ?? default
                };
                materials.Add(material);

                if (item.CreatedBy != Guid.Empty)
                {
                    var weight = new Weight
                    {
                        Id = Guid.NewGuid(),
                        MaterialId = materialId,
                        WeightValue = item.WeightValue,
                        StartDate = item.StartDate,
                        EndDate = item.EndDate,
                        IsActive = item.IsActive,
                        CreatedDate = DateTime.Now,
                        CreatedBy = item.CreatedBy
                    };
                    weights.Add(weight);
                }
            }

            try
            {

                await _materialRepository.InsertRangeAsync(materials);
                await _materialRepository.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                var inner = ex.InnerException?.Message ?? ex.Message;
                throw new Exception($"[Materials] Lỗi khi lưu vào DB: {inner}", ex);
            }

            try
            {

                if (weights.Count > 0)
                {
                    await _weightRepository.InsertRangeAsync(weights);
                    await _weightRepository.SaveChangesAsync();
                }
            }
            catch (DbUpdateException ex)
            {
                var inner = ex.InnerException?.Message ?? ex.Message;
                throw new Exception($"[Weights] Lỗi khi lưu vào DB: {inner}", ex);
            }

            return true;
        }
        public async Task<List<MaterialDto>> GetMaterialsAsync()
        {
            var materials = await _materialRepository.GetAllAsync();
            var weights = await _weightRepository.GetAllAsync();

            var result = (from m in materials
                          join w in weights on m.Id equals w.MaterialId into weightGroup
                          from w in weightGroup
                              .Where(x => x.IsActive)
                              .DefaultIfEmpty()
                          select new MaterialDto
                          {
                              Id = m.Id,
                              BOMHead = m.BOMHead,
                              BOMDescription = m.BOMDescription,
                              Code = m.Code,
                              Name = m.Name,
                              TYPE = m.TYPE,
                              Unit = m.Unit,
                              Plant = m.Plant,
                              WeightId = w?.Id,
                              WeightValue = w?.WeightValue,
                              StartDate = w?.StartDate,
                              EndDate = w?.EndDate,
                              CreatedDate = w?.CreatedDate,
                              CreatedBy = w?.CreatedBy
                          }).ToList();

            return result;
        }

        public async Task<bool> UpdateMaterialWeightAsync(UpdateWeightDto updateWeight)
        {
            var now = DateTime.Now.Date;
            var materialId = updateWeight.MaterialId;
            var startDate = updateWeight.StartDate.Date;
            var weightValue = updateWeight.WeightValue;
            var userId = updateWeight.CreatedBy;

            var activeWeight = await _weightRepository.FirstOrDefaultAsync(w =>
                w.MaterialId == materialId &&
                w.IsActive);

            if (startDate > now)
            {
                if (activeWeight != null)
                {
                    activeWeight.EndDate = startDate;
                }

                var newFutureWeight = new Weight
                {
                    Id = Guid.NewGuid(),
                    MaterialId = materialId,
                    WeightValue = weightValue,
                    StartDate = startDate,
                    EndDate = null,
                    IsActive = false, // Chờ Job bật
                    CreatedDate = now,
                    CreatedBy = userId
                };
                await _weightRepository.InsertAsync(newFutureWeight);
            }
            else
            {
                if (activeWeight != null)
                {
                    activeWeight.IsActive = false;
                    activeWeight.EndDate = startDate;
                }

                var newActiveWeight = new Weight
                {
                    Id = Guid.NewGuid(),
                    MaterialId = materialId,
                    WeightValue = weightValue,
                    StartDate = startDate,
                    EndDate = null,
                    IsActive = true, // Bật ngay
                    CreatedDate = now,
                    CreatedBy = userId
                };
                await _weightRepository.InsertAsync(newActiveWeight);
            }

            await _weightRepository.SaveChangesAsync();
            return true;
        }

        public async Task ActivateScheduledWeightsAsync()
        {
            var now = DateTime.Now.Date;

            var pendingWeights = await _weightRepository.GetListAsync(w =>
                !w.IsActive &&
                w.StartDate <= now);

            if (!pendingWeights.Any()) return;

            var weightsToActivate = pendingWeights
                .GroupBy(w => w.MaterialId)
                .Select(g => g.OrderByDescending(w => w.StartDate).First())
                .ToList();

            foreach (var newWeight in weightsToActivate)
            {
                var activeWeight = await _weightRepository.FirstOrDefaultAsync(w =>
                    w.MaterialId == newWeight.MaterialId &&
                    w.IsActive);

                if (activeWeight != null)
                {
                    activeWeight.IsActive = false;
                    await _weightRepository.UpdateAsync(activeWeight);
                }

                newWeight.IsActive = true;
                await _weightRepository.UpdateAsync(newWeight);
            }

            await _weightRepository.SaveChangesAsync();
        }

        public async Task<List<WeightDto>> GetWeightByMaterial(Guid materialId)
        {
            var weights = await _weightRepository.GetListAsync(w => w.MaterialId == materialId);

            return weights.Select(w => new WeightDto
            {
                MaterialId = w.MaterialId,
                WeightId = w.Id,
                WeightValue = w.WeightValue,    
                StartDate = w.StartDate,
                EndDate = w.EndDate,
                CreatedDate = w.CreatedDate,
                CreatedBy = w.CreatedBy,
                IsActive = w.IsActive,
            }).ToList();
        }
    }
}
