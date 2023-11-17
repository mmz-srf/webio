namespace WebIO.DataAccess;

using Model;
using Model.Readonly;

public interface IChangeLogRepository
{
    void Add(ChangeLogEntry entry);

    QueryResult<ChangeLogEntryInfo> Query(int start, int count);
    int QueryCount();
}