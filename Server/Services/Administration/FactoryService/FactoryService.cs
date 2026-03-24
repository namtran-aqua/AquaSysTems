using AquaSolution.Data.Data.Entities.Admin;
using AquaSolution.Data.Repositories;
using AquaSolution.Shared.Factory;

namespace AquaSolution.Server.Services.Administration.FactoryService
{
    public class FactoryService : IFactoryService
    {
        private readonly IRepository<Factory> _factoryRepo;
        public FactoryService(IRepository<Factory> factoryRepo)
        {
            _factoryRepo = factoryRepo;
        }
        public async Task<bool> CreatedFactory(FactoryDto factoryDto)
        {
            var factory = new Factory
            {
                Id = Guid.NewGuid(),
                Name = factoryDto.Name,
                Code = factoryDto.Code,
                Note = factoryDto.Note,
                FactoryType = factoryDto.FactoryType,
                Description = factoryDto.Description,
            };
            await _factoryRepo.InsertAsync(factory);
            var isSave = await _factoryRepo.SaveChangesAsync();
            if (isSave == 0) return false;
            return true;
        }

        public async Task<bool> DeleteFactory(Guid factoryId)
        {
           var factory = await _factoryRepo.GetByIdAsync(factoryId);
            if(factory == null) return false;
            return await _factoryRepo.DeleteAsync(factory);
        }

        public async Task<List<FactoryDto>> GetListFactory()
        {
            var factoryData = from factory in await _factoryRepo.GetQueryableAsync()
                              orderby factory.CreatedDate descending
                              select new FactoryDto
                              {
                                  Id = factory.Id,
                                  Name = factory.Name,
                                  Code = factory.Code,
                                  Note = factory.Note,
                                  Description = factory.Description,
                                  FactoryType = factory.FactoryType,
                                  CreatedDate = factory.CreatedDate,
                              };
            var dataReturn =  factoryData.ToList();
            if (dataReturn.Count > 0)
            {
                return dataReturn;
            }
            return new List<FactoryDto>();
        }

        public async Task<bool> UpdateFactory(FactoryDto factoryDto)
        {
            var factory = await _factoryRepo.GetByIdAsync(factoryDto.Id);
            if(factory == null) return false;
            factory.Code = factoryDto.Code;
            factory.Name = factoryDto.Name;
            factory.Description = factoryDto.Description;
            factory.FactoryType = factoryDto.FactoryType;
            factory.Note = factoryDto.Note;
            return await _factoryRepo.UpdateAsync(factory);
        }
    }
}
