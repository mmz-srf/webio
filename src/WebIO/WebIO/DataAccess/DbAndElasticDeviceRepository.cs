namespace WebIO.DataAccess;

using System;
using System.Collections.Generic;
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

  public Device? GetDevice(Guid deviceId)
    => _elastic.GetDevice(deviceId);

  public void Upsert(Device device)
  {
    _db.Upsert(device);
    _elastic.Upsert(device);
  }

  public void Delete(Guid deviceId)
  {
    _db.Delete(deviceId);
    _elastic.Delete(deviceId);
  }

  public QueryResult<DeviceInfo> GetDeviceInfos(Query query)
    => _elastic.GetDeviceInfos(query);

  public int GetDeviceCount(Query query)
    => _elastic.GetDeviceCount(query);

  public QueryResult<InterfaceInfo> GetInterfaceInfos(Query query)
    => _elastic.GetInterfaceInfos(query);

  public int GetInterfaceCount(Query query)
    => _elastic.GetInterfaceCount(query);

  public QueryResult<StreamInfo> GetStreamInfos(Query query)
    => _elastic.GetStreamInfos(query);

  public int GetStreamCount(Query query)
    => _elastic.GetStreamCount(query);

  public bool IsDuplicateDeviceName(string deviceName, Guid ownId)
    => _elastic.IsDuplicateDeviceName(deviceName, ownId);

  public IEnumerable<Device> GetDevicesByIds(IEnumerable<Guid> deviceIds)
    => _elastic.GetDevicesByIds(deviceIds);
}
