using KeepTrack.Models;

namespace KeepTrack.Contracts.Services;

public interface IMovementService
{
    public Total GetTotal(int year, int month);
    
    public List<Movement> GetMovements(int year, int month);
}