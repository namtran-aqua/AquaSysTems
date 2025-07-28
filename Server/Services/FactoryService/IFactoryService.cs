using AquaSolution.Shared.Departments;
using AquaSolution.Shared.Factory;

namespace AquaSolution.Server.Services.FactoryService
{
    public interface IFactoryService
    {
        Task<List<FactoryDto>> GetListFactory();
        Task<bool> DeleteFactory(Guid factoryId);
        Task<bool> CreatedFactory(FactoryDto factoryDto);
        Task<bool> UpdateFactory(FactoryDto factoryDto);
    }
}
