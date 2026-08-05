using AquaSolution.Shared.Administration.Sections;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AquaSolution.Server.Services.Administration.SectionService
{
    public interface ISectionService
    {
        Task<List<SectionDto>> GetListSection();
        Task<List<SectionDto>> GetSectionsByDepartment(Guid departmentId);
        Task<bool> CreatedSection(SectionDto sectionDto);
        Task<bool> UpdateSection(SectionDto sectionDto);
        Task<bool> DeleteSection(Guid sectionId);
    }
}
