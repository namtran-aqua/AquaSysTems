using AquaSolution.Shared.Position;

namespace AquaSolution.Server.Services.PositionService
{
    public interface IPositionService
    {
        Task<List<PositionDto>> GetListPosition();
        Task<bool> DeletePosition(Guid departmentId);
        Task<bool> CreatedPosition(PositionDto departmentDto);
        Task<bool> UpdatePosition(PositionDto departmentDto);
    }
}
