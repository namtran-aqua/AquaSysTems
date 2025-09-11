using AquaSolution.Shared.ITSuport.RequestSuportCategory;

namespace AquaSolution.Server.Services.ITSuport.RequestSuportCategories
{
    public interface IRequestSuportCategoryService
    {
        Task<bool> CreatedAsync(RequestSuportCategoryDto requestSuportCategoryDto);
        Task<bool> DeleteAssync(Guid requestSuportCategoryId);
        Task<bool> UpdateAsync(RequestSuportCategoryDto requestSuportCategoryDto);
        Task<List<RequestSuportCategoryDto>> GetAllAsync();

    }
}
