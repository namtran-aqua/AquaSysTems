using AquaSolution.Shared.Departments;

namespace AquaSolution.Server.Services.DepartmentService
{
    public interface IDepartmentService
    {
        Task<List<DepartmentDto>> GetListDepartment();
        Task<bool> DeleteDepartment(Guid departmentId);
        Task<bool> CreatedDepartment(DepartmentDto departmentDto);
        Task<bool> UpdateDepartment(DepartmentDto departmentDto);
    }
}
