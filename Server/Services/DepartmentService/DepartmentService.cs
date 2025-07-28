using AquaSolution.Data.Data.Entities;
using AquaSolution.Data.Repositories;
using AquaSolution.Shared.Departments;

namespace AquaSolution.Server.Services.DepartmentService
{
    public class DepartmentService : IDepartmentService
    {
        private readonly IRepository<Department> _departmentRepo;
        public DepartmentService(IRepository<Department> departmentRepo)
        {
            _departmentRepo = departmentRepo;
        }
        public async Task<bool> CreatedDepartment(DepartmentDto departmentDto)
        {
            var department = new Department
            {
                Id = Guid.NewGuid(),
                Name = departmentDto.Name,
                Code = departmentDto.Code,
                Note = departmentDto.Note,
                DepartmentType = departmentDto.DepartmentType,
                DesCription = departmentDto.Description,
            };
            await _departmentRepo.InsertAsync(department);
            var boolReturn = await _departmentRepo.SaveChangesAsync();
            if(boolReturn == 0)return false;
            return true;
        }

        public async Task<bool> DeleteDepartment(Guid departmentId)
        {
            var department = await _departmentRepo.GetByIdAsync(departmentId);
            if (department == null) return false;
            return await _departmentRepo.DeleteAsync(department);
        }

        public async Task<List<DepartmentDto>> GetListDepartment()
        {
            try
            {
                var departments = from department in await _departmentRepo.GetQueryableAsync()
                                  select new DepartmentDto
                                  {
                                      Id = department.Id,
                                      Name = department.Name,
                                      Code = department.Code,
                                      Note = department.Note,
                                      Description = department.DesCription,
                                      DepartmentType = department.DepartmentType,
                                  };
                var listDepartment = departments.ToList();
                if (listDepartment.Count == 0)
                    return new List<DepartmentDto>();
                return listDepartment;
            }
            catch (Exception ex) 
            {
                throw ex;
            }
 
        }

        public async Task<bool> UpdateDepartment(DepartmentDto departmentDto)
        {
            var deparment = await _departmentRepo.GetByIdAsync(departmentDto.Id);
            if(deparment == null) return false;
            deparment.Note = departmentDto.Note;
            deparment.Code = departmentDto.Code;
            deparment.Name = departmentDto.Name;
            deparment.DesCription = departmentDto.Description;
            deparment.DepartmentType =departmentDto.DepartmentType;
            return await _departmentRepo.UpdateAsync(deparment);
        }
    }
}
