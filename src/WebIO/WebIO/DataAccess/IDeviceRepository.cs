namespace WebIO.DataAccess;

using System;
using System.Collections.Generic;
using Model;
using Model.Readonly;

public interface IDeviceRepository
{
    Device? GetDevice(Guid deviceId);
    void Upsert(Device device);
    void Delete(Guid deviceId);

    QueryResult<DeviceInfo> GetDeviceInfos(Query query);
    int GetDeviceCount(Query query);
    QueryResult<InterfaceInfo> GetInterfaceInfos(Query query);
    int GetInterfaceCount(Query query);
    QueryResult<StreamInfo> GetStreamInfos(Query query);
    int GetStreamCount(Query query);

    bool IsDuplicateDeviceName(string deviceName, Guid ownId);
    IEnumerable<Device> GetDevicesByIds(IEnumerable<Guid> deviceIds);
}