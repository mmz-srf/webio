namespace WebIO.DataAccess;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Model;
using Model.Readonly;

public interface IDeviceRepository
{
    Task<Device?> GetDeviceAsync(Guid deviceId, CancellationToken ct);
    Task UpsertAsync(Device device, CancellationToken ct);
    Task DeleteAsync(Guid deviceId, CancellationToken ct);

    Task<QueryResult<DeviceInfo>> GetDeviceInfosAsync(Query query, CancellationToken ct);
    Task<int> GetDeviceCountAsync(Query query, CancellationToken ct);
    Task<QueryResult<InterfaceInfo>> GetInterfaceInfosAsync(Query query, CancellationToken ct);
    Task<int> GetInterfaceCountAsync(Query query, CancellationToken ct);
    Task<QueryResult<StreamInfo>> GetStreamInfosAsync(Query query, CancellationToken ct);
    Task<int> GetStreamCountAsync(Query query, CancellationToken ct);

    Task<bool> IsDuplicateDeviceNameAsync(string deviceName, Guid ownId, CancellationToken ct);
    Task<IEnumerable<Device>> GetDevicesByIdsAsync(IEnumerable<Guid> deviceIds, CancellationToken ct);
}