using AquaSolution.Data.Data.Entities.Admin;
using AquaSolution.Data.Data.Entities.Scraps;
using AquaSolution.Data.Repositories;
using AquaSolution.Shared.Administration.Sections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AquaSolution.Server.Services.Administration.SectionService
{
    public class SectionService : ISectionService
    {
        private readonly IRepository<Section> _sectionRepo;
        private readonly IRepository<Department> _departmentRepo;
        private readonly IRepository<User> _userRepo;
        private readonly IRepository<FlowApprovalScrap> _flowApprovalRepo;
        private readonly IRepository<HistoryScrap> _historyScrapRepo;

        public SectionService(
            IRepository<Section> sectionRepo,
            IRepository<Department> departmentRepo,
            IRepository<User> userRepo,
            IRepository<FlowApprovalScrap> flowApprovalRepo,
            IRepository<HistoryScrap> historyScrapRepo)
        {
            _sectionRepo = sectionRepo;
            _departmentRepo = departmentRepo;
            _userRepo = userRepo;
            _flowApprovalRepo = flowApprovalRepo;
            _historyScrapRepo = historyScrapRepo;
        }

        public async Task<bool> CreatedSection(SectionDto sectionDto)
        {
            // Validate Code
            var existCode = await _sectionRepo.FirstOrDefaultAsync(x => x.Code == sectionDto.Code);
            if (existCode != null) throw new Exception("Code đã tồn tại.");

            // Validate Name in Department
            var existName = await _sectionRepo.FirstOrDefaultAsync(x => x.DepartmentId == sectionDto.DepartmentId && x.Name == sectionDto.Name);
            if (existName != null) throw new Exception("Tên khu vực đã tồn tại trong phòng ban này.");

            var section = new Section
            {
                Id = Guid.NewGuid(),
                Name = sectionDto.Name,
                Code = sectionDto.Code,
                Description = sectionDto.Description,
                DepartmentId = sectionDto.DepartmentId,
                CreatedDate = DateTime.Now
            };

            await _sectionRepo.InsertAsync(section);
            var result = await _sectionRepo.SaveChangesAsync();
            return result > 0;
        }

        public async Task<bool> DeleteSection(Guid sectionId)
        {
            var section = await _sectionRepo.GetByIdAsync(sectionId);
            if (section == null) return false;

            // Restrict delete if used
            var isUsedByUser = await _userRepo.AnyAsync(x => x.SectionId == sectionId);
            if (isUsedByUser) throw new Exception("Không thể xóa do đã có User thuộc khu vực này.");

            var isUsedByFlow = await _flowApprovalRepo.AnyAsync(x => x.SectionId == sectionId);
            if (isUsedByFlow) throw new Exception("Không thể xóa do khu vực này đang được cấu hình trong Approval Flow.");

            var isUsedByScrap = await _historyScrapRepo.AnyAsync(x => x.SectionId == sectionId);
            if (isUsedByScrap) throw new Exception("Không thể xóa do khu vực này đã phát sinh dữ liệu Scrap.");

            return await _sectionRepo.DeleteAsync(section);
        }

        public async Task<List<SectionDto>> GetListSection()
        {
            var query = await _sectionRepo.GetQueryableAsync();
            var depQuery = await _departmentRepo.GetQueryableAsync();

            var result = from s in query
                         join d in depQuery on s.DepartmentId equals d.Id into deptGroup
                         from d in deptGroup.DefaultIfEmpty()
                         orderby s.CreatedDate descending
                         select new SectionDto
                         {
                             Id = s.Id,
                             Name = s.Name,
                             Code = s.Code,
                             Description = s.Description,
                             DepartmentId = s.DepartmentId,
                             DepartmentName = d != null ? d.Name : string.Empty,
                             CreatedDate = s.CreatedDate
                         };
            
            return result.ToList();
        }

        public async Task<List<SectionDto>> GetSectionsByDepartment(Guid departmentId)
        {
            var sections = await _sectionRepo.GetListAsync(x => x.DepartmentId == departmentId);
            return sections.Select(s => new SectionDto
            {
                Id = s.Id,
                Name = s.Name,
                Code = s.Code,
                Description = s.Description,
                DepartmentId = s.DepartmentId,
                CreatedDate = s.CreatedDate
            }).OrderBy(x => x.Name).ToList();
        }

        public async Task<bool> UpdateSection(SectionDto sectionDto)
        {
            var section = await _sectionRepo.GetByIdAsync(sectionDto.Id);
            if (section == null) return false;

            // Validate Code
            var existCode = await _sectionRepo.FirstOrDefaultAsync(x => x.Code == sectionDto.Code && x.Id != sectionDto.Id);
            if (existCode != null) throw new Exception("Code đã tồn tại.");

            // Validate Name in Department
            var existName = await _sectionRepo.FirstOrDefaultAsync(x => x.DepartmentId == sectionDto.DepartmentId && x.Name == sectionDto.Name && x.Id != sectionDto.Id);
            if (existName != null) throw new Exception("Tên khu vực đã tồn tại trong phòng ban này.");

            // Validate changing department if already has data
            if (section.DepartmentId != sectionDto.DepartmentId)
            {
                var isUsedByUser = await _userRepo.AnyAsync(x => x.SectionId == sectionDto.Id);
                var isUsedByFlow = await _flowApprovalRepo.AnyAsync(x => x.SectionId == sectionDto.Id);
                var isUsedByScrap = await _historyScrapRepo.AnyAsync(x => x.SectionId == sectionDto.Id);
                
                if (isUsedByUser || isUsedByFlow || isUsedByScrap)
                {
                    throw new Exception("Không thể đổi Department do Section này đã phát sinh dữ liệu (User, Flow hoặc Scrap). Bạn cần tạo Section mới thay vì sửa đổi.");
                }
            }

            section.Name = sectionDto.Name;
            section.Code = sectionDto.Code;
            section.Description = sectionDto.Description;
            section.DepartmentId = sectionDto.DepartmentId;

            return await _sectionRepo.UpdateAsync(section);
        }
    }
}
