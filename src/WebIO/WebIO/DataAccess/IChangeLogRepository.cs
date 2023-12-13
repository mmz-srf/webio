namespace WebIO.DataAccess;

using System.Threading;
using System.Threading.Tasks;
using Model;
using Model.Readonly;

public interface IChangeLogRepository
{
    void Add(ChangeLogEntry entry);

    Task<QueryResult<ChangeLogEntryInfo>> QueryAsync(int start, int count, CancellationToken ct);
    Task<int> QueryCountAsync(CancellationToken ct);
}