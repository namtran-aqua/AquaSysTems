using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AquaSolution.Shared.Enum.KPIType;
using AquaSolution.Shared.KPI.Scope;

namespace AquaSolution.Server.Services.KPI.KPIScope
{
    public interface IKPIScopeService
    {
        Task<List<KPIScopeDto>> GetScopesAsync(int? year = null, Guid? departmentId = null, Guid? userId = null, bool? isActive = null);
        Task<KPIScopeDto> GetScopeByIdAsync(Guid id);
        Task<KPIScopeDto> CreateScopeAsync(KPIScopeCreateOrUpdateDto dto, Guid currentUserId);
        Task<KPIScopeDto> UpdateScopeAsync(KPIScopeCreateOrUpdateDto dto, Guid currentUserId);
        Task<bool> DeactivateScopeAsync(Guid scopeId, Guid currentUserId);
        List<ExemptMonthItem> SuggestExemptMonths(KPIScopeType scopeType, DateTime? effectiveDate, DateTime? startDate, DateTime? endDate);
        Task<List<KPIComplianceMatrixDto>> GetMonthlyComplianceMatrixAsync(int year, Guid? departmentId);
        Task<List<KPIExemptMonthDto>> GetUserExemptMonthsAsync(Guid userId, int year);
    }
}
