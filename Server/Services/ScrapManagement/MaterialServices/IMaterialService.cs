using AquaSolution.Shared.ScrapManagement.Materials;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AquaSolution.Server.Services.ScrapManagetment.MaterialServices
{
    public interface IMaterialService
    {
        Task<bool> ImportMaterialsAsync(List<ImportExcelDto> importExcelDtos);
        Task<List<MaterialDto>> GetMaterialsAsync();
        Task<bool> UpdateMaterialWeightAsync(UpdateWeightDto updateWeight);
        Task ActivateScheduledWeightsAsync();
        Task<List<WeightDto>> GetWeightByMaterial(Guid materialId);
    }
}
