using AlienClock;
using AlienClockAvalonia.Models;
using LiteDB;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
namespace AlienClockAvalonia;

public interface IAlarmService
{
    void Add(Alarm alarm);
    void Remove(int id);
    void Update(int id, Alarm alarm);
    IEnumerable<Alarm> GetAlarms();
}

public class AlarmService : IAlarmService
{
    private static readonly string collection = "alarm";
    private readonly LiteDatabase _db;
    public AlarmService(LiteDatabase db)
    {
        _db = db;
    }
    public void Add(Alarm alarm)
    {
        var col = _db.GetCollection<Alarm>(collection);
        col.Insert(alarm);
    }

    public IEnumerable<Alarm> GetAlarms()
    {
        var col = _db.GetCollection<Alarm>(collection);
        var alarms = col.FindAll();
        return alarms;
    }

    public void Remove(int id)
    {
        var col = _db.GetCollection<Alarm>(collection);
        col.Delete(new BsonValue(id));
    }

    public void Update(int id, Alarm alarm)
    {
        var col = _db.GetCollection<Alarm>(collection);
        col.Update(new BsonValue(id), alarm);
    }
}
