using AquaSolution.Data.Data.Entities;
using AquaSolution.Data.Repositories;
using AquaSolution.Shared.ITSuport.RequestSuportCategory;

namespace AquaSolution.Server.Services.ITSuport.RequestSuportCategories
{
    public class RequestSuportCategoryService : IRequestSuportCategoryService
    {
        private readonly IRepository<User> _userRepo;
        private readonly IRepository<RequestSuportCategory> _requestSuportCategoryRepo;

        public RequestSuportCategoryService(
               IRepository<User> userRepo,
                  IRepository<RequestSuportCategory> requestSuportCategoryRepo)
        {
            _userRepo = userRepo;
            _requestSuportCategoryRepo = requestSuportCategoryRepo;
        }

        public async Task<bool> CreatedAsync(RequestSuportCategoryDto requestSuportCategoryDto)
        {
            var requestSuportCategory = new RequestSuportCategory
            {
                Id = Guid.NewGuid(),
                TechnicianId = requestSuportCategoryDto.TechnicianId,
                Name = requestSuportCategoryDto.Name,
                CreatedDate = DateTime.Now,
                IsActive = true,
            };
            await _requestSuportCategoryRepo.InsertAsync(requestSuportCategory);
            return await _requestSuportCategoryRepo.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteAssync(Guid id)
        {
            var requestSuportCategory = await _requestSuportCategoryRepo.FirstOrDefaultAsync(x => x.Id == id);
            if (requestSuportCategory != null)
            {
                return await _requestSuportCategoryRepo.DeleteAsync(requestSuportCategory);
            }
            return false;
        }

        public async Task<List<RequestSuportCategoryDto>> GetAllAsync()
        {
            var query = from requestSuoportCategories in await _requestSuportCategoryRepo.GetQueryableAsync()
                        join user in await _userRepo.GetQueryableAsync()
                        on requestSuoportCategories.TechnicianId equals user.Id
                        select new RequestSuportCategoryDto
                        {
                            Id = requestSuoportCategories.Id,
                            Name = requestSuoportCategories.Name,
                            TechnicianId = requestSuoportCategories.TechnicianId,
                            TechnicianName = user.FullName,
                            IsActive = requestSuoportCategories.IsActive,
                            CreatedDate = requestSuoportCategories.CreatedDate
                        };
            var data = query.OrderByDescending(x => x.CreatedDate)
                    .ToList();
            if (data.Count > 0) 
            {
                return data;
            }return new List<RequestSuportCategoryDto>();
        }

        public async Task<bool> UpdateAsync(RequestSuportCategoryDto requestSuportCategoryDto)
        {
            var requestSuportCategory = await _requestSuportCategoryRepo.FirstOrDefaultAsync(
                x => x.Id == requestSuportCategoryDto.Id);
            if(requestSuportCategory == null) { return false; }
            requestSuportCategory.Name = requestSuportCategoryDto.Name;
            requestSuportCategory.TechnicianId = requestSuportCategoryDto.TechnicianId;
            requestSuportCategory.IsActive = requestSuportCategoryDto.IsActive;
            return await _requestSuportCategoryRepo.UpdateAsync(requestSuportCategory);
        }
    }
}
