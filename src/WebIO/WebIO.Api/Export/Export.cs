namespace WebIO.Api.Export;

using System.Collections.Immutable;
using Api.Nevion;
using Application;
using Controllers;
using DataAccess;
using Microsoft.Extensions.Logging;
using Model.Display;
using Model.Readonly;

public abstract class Export
{
    private readonly ILogger _log;
    private readonly IDeviceRepository _deviceRepository;
    private readonly IMetadataRepository _metadata;
    private readonly NevionApi? _nevionApi;

    protected Export(
        IDeviceRepository deviceRepository,
        IMetadataRepository metadata,
        NevionApi? nevionApi,
        ILogger log)
    {
        _deviceRepository = deviceRepository;
        _metadata = metadata;
        _nevionApi = nevionApi;
        _log = log;
    }

    protected List<InterfaceValueGetter> GetAllInterfaces(ExportArgs exportArgs)
    {
        _log.LogDebug("Loading Interfaces");
        var query = new Query(0,int.MaxValue);

        if (!exportArgs.All && !exportArgs.SelectedDeviceIds.Any())
        {
            //Filter
            query = query.WithFilter(exportArgs.Filters, null);
        }

        var data = _deviceRepository.GetInterfaceInfos(query).Data;
        if (!exportArgs.All && exportArgs.SelectedDeviceIds.Any())
        {
            data =  data
                .Where(i => exportArgs.SelectedDeviceIds.Contains(i.DeviceId.ToString()))
                .ToImmutableList();

        }

        return data
            .Select(ii => new InterfaceValueGetter(ii, _metadata))
            .ToList();
    }

    protected List<InterfaceValueGetter> GetAllInterfaces(Dictionary<string, string> request)
    {
        _log.LogDebug("Loading Interfaces");
        var query = new Query(0,0)
            .WithFilter(request, null);

        var interfaceCount = _deviceRepository.GetInterfaceCount(query);

        query = new Query(0, interfaceCount)
            .WithSorting(null, null)
            .WithFilter(request, null);

        var interfaceInfos = _deviceRepository.GetInterfaceInfos(query)
            .Data
            .Select(ii => new InterfaceValueGetter(ii, _metadata))
            .ToList();
        return interfaceInfos;
    }

    protected List<StreamsValueGetter> GetAllStreams(ExportArgs exportArgs)
    {
        _log.LogDebug("Loading Streams");

        var query = new Query(0, int.MaxValue);

        if (!exportArgs.All && !exportArgs.SelectedDeviceIds.Any())
        {
            //Filter
            query = query.WithFilter(exportArgs.Filters, null);
        }

        var data = _deviceRepository.GetStreamInfos(query).Data;
        if (!exportArgs.All && exportArgs.SelectedDeviceIds.Any())
        {
            data = data
                .Where(i => exportArgs.SelectedDeviceIds.Contains(i.DeviceId.ToString()))
                .ToImmutableList();
        }

        return data
            .Select(s => new StreamsValueGetter(s, _metadata, _nevionApi))
            .ToList();
    }

    protected List<StreamsValueGetter> GetAllStreams(Dictionary<string, string> request)
    {
        _log.LogDebug("Loading Streams");

        var query = new Query(0, 0)
            .WithFilter(request, null);

        var streamsCount = _deviceRepository.GetInterfaceCount(query);

        query = new Query(0, streamsCount)
            .WithSorting(null, null)
            .WithFilter(request, null);

        var streamInfos = _deviceRepository.GetStreamInfos(query)
            .Data
            .Select(ii => new StreamsValueGetter(ii, _metadata, _nevionApi))
            .ToList();
        return streamInfos;
    }

    protected string GetColumnName(ColumnDefinition column)
    {
        var columnName = column.DisplayName;
        if (string.IsNullOrWhiteSpace(columnName))
        {
            columnName = _metadata.DataFields
                .FirstOrDefault(f => f.Key == column.Property)
                ?.DisplayName;
        }

        return columnName ?? string.Empty;
    }

    protected static string GetValue(ColumnDefinition column, ValueGetter row)
    {
        string value;
        if (column.StaticValue != null)
        {
            value = column.StaticValue;
        }
        else if (column.ScriptDelegate != null)
        {
            value = column.ScriptDelegate.Invoke(row) ?? string.Empty;
        }
        else
        {
            value = row.GetValue(column.Property!);
        }

        return value;
    }
}