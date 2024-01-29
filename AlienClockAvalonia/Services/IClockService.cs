using AlienClockAvalonia.Models;
using LiteDB;
namespace AlienClockAvalonia;

public interface IClockService
{
    void Set(ClockConfig time);
    ClockConfig? Get(int id);
}

public class ClockService : IClockService
{
    private static readonly string collection = "clock_config";
    private readonly LiteDatabase _db;
    public ClockService(LiteDatabase db)
    {
        _db = db;
    }
    public void Set(ClockConfig time)
    {
        _db.GetCollection<ClockConfig>(collection).Insert(time);
    }

    public ClockConfig Get(int id)
    {
        return _db.GetCollection<ClockConfig>(collection).FindOne(i => i.Id == id);
    }
}