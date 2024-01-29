using System;
using System.Linq;
using LiteDB;
using Microsoft.Extensions.Options;
namespace AlienClockAvalonia;

public class LiteDbFactory
{
    private readonly LiteDatabaseServiceOptions options;

    public LiteDbFactory(IOptions<LiteDatabaseServiceOptions> options)
    {
        this.options = options.Value ?? throw new ArgumentNullException(nameof(options));
    }

    public LiteDatabase Create()
    {
        var database = new LiteDatabase(options.ConnectionString, options.Mapper);
        if (options.DatabasePatches.Any())
        {
            try
            {
                foreach (var item in options.DatabasePatches)
                {
                    item.Invoke(database);
                }
                database.Commit();
            }
            catch (Exception e)
            {
                database.Rollback();
                throw new LiteException(e.HResult, e.Message);
            }
        }
        return database;
    }
}