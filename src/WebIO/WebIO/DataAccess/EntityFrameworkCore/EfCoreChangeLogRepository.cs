namespace WebIO.DataAccess.EntityFrameworkCore;

using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Mappers;
using Microsoft.EntityFrameworkCore;
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

    public async Task<QueryResult<ChangeLogEntryInfo>> QueryAsync(int start, int count, CancellationToken ct)
    {
        var entries = (await _context.ChangeLog
            .OrderByDescending(x => x.Timestamp)
            .Skip(start)
            .Take(count)
            .Select(x => x.ToReadonly())
            .ToListAsync(ct))
            .ToImmutableList();

        return new(start, entries.Count, entries);
    }

    public Task<int> QueryCountAsync(CancellationToken ct)
        => _context.ChangeLog.CountAsync(ct);
}