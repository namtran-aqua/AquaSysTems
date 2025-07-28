using AquaSolution.Data.Data.Entities;
using AquaSolution.Data.Repositories;
using AquaSolution.Shared.Position;

namespace AquaSolution.Server.Services.PositionService
{
    public class PositionService : IPositionService
    {
        private readonly IRepository<Position> _positionRepo;
        public PositionService(IRepository<Position> positionRepo)
        {
            _positionRepo = positionRepo;
        }
        public async Task<bool> CreatedPosition(PositionDto positionDto)
        {
            var position = new Position
            {
                Id = Guid.NewGuid(),
                Name = positionDto.Name,
                Code = positionDto.Code,
                Note = positionDto.Note,
                Type = positionDto.PositionType,
                DesCription = positionDto.Description,
            };
            await _positionRepo.InsertAsync(position);
            var boolReturn = await _positionRepo.SaveChangesAsync();
            if(boolReturn == 0)return false;
            return true;
        }

        public async Task<bool> DeletePosition(Guid positionId)
        {
            var position = await _positionRepo.GetByIdAsync(positionId);
            if (position == null) return false;
            return await _positionRepo.DeleteAsync(position);
        }

        public async Task<List<PositionDto>> GetListPosition()
        {
            try
            {
                var positions = from position in await _positionRepo.GetQueryableAsync()
                                  select new PositionDto
                                  {
                                      Id = position.Id,
                                      Name = position.Name,
                                      Code = position.Code,
                                      Note = position.Note,
                                      Description = position.DesCription,
                                      PositionType = position.Type,
                                  };
                var listPosition = positions.ToList();
                if (listPosition.Count == 0)
                    return new List<PositionDto>();
                return listPosition;
            }
            catch (Exception ex) 
            {
                throw ex;
            }
 
        }
        public async Task<bool> UpdatePosition(PositionDto positionDto)
        {
            var deparment = await _positionRepo.GetByIdAsync(positionDto.Id);
            if(deparment == null) return false;
            deparment.Note = positionDto.Note;
            deparment.Code = positionDto.Code;
            deparment.Name = positionDto.Name;
            deparment.DesCription = positionDto.Description;
            deparment.Type =positionDto.PositionType;
            return await _positionRepo.UpdateAsync(deparment);
        }
    }
}
