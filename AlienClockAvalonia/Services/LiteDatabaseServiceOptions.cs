using LiteDB;
using System;
using System.Collections.Generic;
namespace AlienClockAvalonia;

public class LiteDatabaseServiceOptions
{
    public List<Action<LiteDatabase>> DatabasePatches { get; } = new List<Action<LiteDatabase>>();
    public LiteDatabaseServiceOptions() { }
    public LiteDatabaseServiceOptions(string connectionString)
    {
        if (string.IsNullOrEmpty(connectionString)) throw new ArgumentNullException(nameof(connectionString));

        this.ConnectionString = new ConnectionString(connectionString);
    }
    public ConnectionString ConnectionString { get; set; } = new ConnectionString();
    public BsonMapper Mapper { get; set; } = BsonMapper.Global;

    public LiteDatabaseServiceOptions AddDatabasePatch(Action<LiteDatabase> action)
    {
        DatabasePatches.Add(action);
        return this;
    }


}
