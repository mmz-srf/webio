namespace WebIO.DataAccess;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Elastic;
using EntityFrameworkCore;
using Model;
using Model.Readonly;

public class DbAndElasticDeviceRepository : IDeviceRepository
{
  private readonly ElasticDeviceRepository _elastic;
  private readonly EfCoreDeviceRepository _db;

  public DbAndElasticDeviceRepository(ElasticDeviceRepository elastic, EfCoreDeviceRepository db)
  {
    _elastic = elastic;
    _db = db;
  }

  public Task<Device?> GetDeviceAsync(Guid deviceId, CancellationToken ct)
    => _elastic.GetDeviceAsync(deviceId, ct);

  public async Task UpsertAsync(Device device, CancellationToken ct)
  {
    await _db.UpsertAsync(device, ct);
    await _elastic.UpsertAsync(device, ct);
  }

  public async Task DeleteAsync(Guid deviceId, CancellationToken ct)
  {
    await _db.DeleteAsync(deviceId, ct);
    await _elastic.DeleteAsync(deviceId, ct);
  }

  public Task<QueryResult<DeviceInfo>> GetDeviceInfosAsync(Query query, CancellationToken ct)
    => _elastic.GetDeviceInfosAsync(query, ct);

  public Task<int> GetDeviceCountAsync(Query query, CancellationToken ct)
    => _elastic.GetDeviceCountAsync(query, ct);

  public Task<QueryResult<InterfaceInfo>> GetInterfaceInfosAsync(Query query, CancellationToken ct)
    => _elastic.GetInterfaceInfosAsync(query, ct);

  public Task<int> GetInterfaceCountAsync(Query query, CancellationToken ct)
    => _elastic.GetInterfaceCountAsync(query, ct);

  public Task<QueryResult<StreamInfo>> GetStreamInfosAsync(Query query, CancellationToken ct)
    => _elastic.GetStreamInfosAsync(query, ct);

  public Task<int> GetStreamCountAsync(Query query, CancellationToken ct)
    => _elastic.GetStreamCountAsync(query, ct);

  public Task<bool> IsDuplicateDeviceNameAsync(string deviceName, Guid ownId, CancellationToken ct)
    => _elastic.IsDuplicateDeviceNameAsync(deviceName, ownId, ct);

  public Task<IEnumerable<Device>> GetDevicesByIdsAsync(IEnumerable<Guid> deviceIds, CancellationToken ct)
    => _elastic.GetDevicesByIdsAsync(deviceIds, ct);
}
