namespace WebIO.DataAccess.EntityFrameworkCore;

using System.Collections.Immutable;
using System.Linq;
using Mappers;
using Model;
using Model.Readonly;

public class EfCoreChangeLogRepository : IChangeLogRepository
{
    private readonly AppDbContext _context;

    public EfCoreChangeLogRepository(AppDbContext context)
    {
        _context = context;
    }

    public void Add(ChangeLogEntry entry)
    {
        _context.ChangeLog.Add(entry.ToEntity());
        _context.SaveChanges();
    }

    public QueryResult<ChangeLogEntryInfo> Query(int start, int count)
    {
        var entries = _context.ChangeLog
            .OrderByDescending(x => x.Timestamp)
            .Skip(start)
            .Take(count)
            .Select(x => x.ToReadonly())
            .ToImmutableList();

        return new(start, entries.Count, entries);
    }

    public int QueryCount()
    {
        return _context.ChangeLog.Count();
    }
}