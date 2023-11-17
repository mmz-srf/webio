namespace WebIO.DataAccess.EntityFrameworkCore;

using System;
using System.Linq;
using System.Linq.Expressions;
using Application;
using Microsoft.EntityFrameworkCore;
using Model;
using Model.Readonly;
using static Model.PseudoProperties;

public static class QueryExtensions
{
  public static IQueryable<DeviceInfoQueryResult> ApplyFilter(
    this IQueryable<DeviceInfoQueryResult> entities,
    Query query,
    PseudoProperties properties)
  {
    using var span = Telemetry.Span();
    var result = query.Filter.Aggregate(entities, (projects, filterItem) =>
    {
      switch (filterItem.Key)
      {
        case Name:
          return projects.Where(p => EF.Functions.Contains(p.Device.Name, $"\"{filterItem.Value}*\""));
        case Comment:
          return projects.Where(
            p => EF.Functions.Contains(p.Device.Comment ?? string.Empty, $"\"{filterItem.Value}*\""));
        case DeviceType:
          return projects.Where(p => EF.Functions.Contains(p.Device.DeviceType, $"\"{filterItem.Value}*\""));
        case Created:
        case Modified:
          return projects;
        case InterfaceCount:
          return int.TryParse(filterItem.Value, out var count)
            ? projects.Where(p => p.Denormalized.InterfaceCount == count)
            : projects;
        default:
          return properties.GetPropertyTypeFromString(filterItem.Key) switch
          {
            PropertyType.Undefined => projects,
            PropertyType.General => projects,
            PropertyType.Device => projects.ApplyDevicePropertyFilter(filterItem),
            PropertyType.Interface => projects,
            PropertyType.Stream => projects,
            _ => projects.ApplyDevicePropertyFilter(filterItem),
          };
      }
    });

    if (!string.IsNullOrWhiteSpace(query.GlobalFilter))
    {
      result = result.Where(p => EF.Functions.Contains(p.Device.Name, $"\"{query.GlobalFilter}*\"")
                                 || EF.Functions.Contains(p.Device.Comment ?? string.Empty,
                                   $"\"{query.GlobalFilter}*\"")
                                 || EF.Functions.Contains(p.Device.Modifier ?? string.Empty,
                                   $"\"{query.GlobalFilter}*\"")
                                 || EF.Functions.Contains(p.Device.Creator ?? string.Empty,
                                   $"\"{query.GlobalFilter}*\"")
                                 || p.Device.Properties.Any(prop
                                   => EF.Functions.Contains(prop.Value ?? string.Empty, $"\"{query.GlobalFilter}*\"")));
    }

    return result;
  }

  private static IQueryable<DeviceInfoQueryResult> ApplyDevicePropertyFilter(
    this IQueryable<DeviceInfoQueryResult> entities,
    QueryFilterItem filterItem)
  {
    return entities.Where(p
      => EF.Functions.Like(p.Device.Properties.First(prop => prop.Key == filterItem.Key).Value ?? string.Empty,
        $"%{filterItem.Value}%"));
  }

  public static IQueryable<InterfaceInfoQueryResult> ApplyFilter(
    this IQueryable<InterfaceInfoQueryResult> entities,
    Query query,
    PseudoProperties properties)
  {
    var result = query.Filter.Aggregate(entities, (projects, filterItem) =>
    {
      return filterItem.Key switch
      {
        DeviceName => projects.Where(p
          => EF.Functions.Contains(p.Device.Name, $"\"{filterItem.Value}*\"")),
        Name => projects.Where(p
          => EF.Functions.Contains(p.Interface.Name, $"\"{filterItem.Value}*\"")),
        Comment => projects.Where(p
          => EF.Functions.Contains(p.Interface.Comment ?? string.Empty, $"\"{filterItem.Value}*\"")),
        DeviceType => projects.Where(p
          => EF.Functions.Contains(p.Device.DeviceType, $"\"{filterItem.Value}*\"")),
        Created => projects,
        Modified => projects,
        StreamsCountVideoSend => int.TryParse(filterItem.Value, out var count)
          ? projects.Where(p => p.Denormalized.StreamsCountVideoSend == count)
          : entities,
        StreamsCountAudioSend => int.TryParse(filterItem.Value, out var count)
          ? projects.Where(p => p.Denormalized.StreamsCountAudioSend == count)
          : entities,
        StreamsCountAncillarySend => int.TryParse(filterItem.Value, out var count)
          ? projects.Where(p => p.Denormalized.StreamsCountAncillarySend == count)
          : entities,
        StreamsCountVideoReceive => int.TryParse(filterItem.Value, out var count)
          ? projects.Where(p => p.Denormalized.StreamsCountVideoReceive == count)
          : entities,
        StreamsCountAudioReceive => int.TryParse(filterItem.Value, out var count)
          ? projects.Where(p => p.Denormalized.StreamsCountAudioReceive == count)
          : entities,
        StreamsCountAncillaryReceive => int.TryParse(filterItem.Value, out var count)
          ? projects.Where(p => p.Denormalized.StreamsCountAncillaryReceive == count)
          : entities,
        _ => properties.GetPropertyTypeFromString(filterItem.Key) switch
        {
          PropertyType.Undefined => projects,
          PropertyType.General => projects,
          PropertyType.Device => projects.ApplyDevicePropertyFilter(filterItem),
          PropertyType.Interface => projects.ApplyInterfacePropertyFilter(filterItem),
          PropertyType.Stream => projects,
          _ => projects.ApplyInterfacePropertyFilter(filterItem),
        },
      };
    });
    if (!string.IsNullOrWhiteSpace(query.GlobalFilter))
    {
      result = result.Where(p => EF.Functions.Contains(p.Interface.Name, $"\"{query.GlobalFilter}*\"")
                                 || EF.Functions.Contains(p.Device.Name, $"\"{query.GlobalFilter}*\"")
                                 || EF.Functions.Contains(p.Interface.Comment ?? string.Empty,
                                   $"\"{query.GlobalFilter}*\"")
                                 || EF.Functions.Contains(p.Interface.Modifier ?? string.Empty,
                                   $"\"{query.GlobalFilter}*\"")
                                 || EF.Functions.Contains(p.Interface.Creator ?? string.Empty,
                                   $"\"{query.GlobalFilter}*\"")
                                 || p.Interface.Properties.Any(prop
                                   => EF.Functions.Contains(prop.Value ?? string.Empty, $"\"{query.GlobalFilter}*\"")));
    }

    return result;
  }

  private static IQueryable<InterfaceInfoQueryResult> ApplyInterfacePropertyFilter(
    this IQueryable<InterfaceInfoQueryResult> entities,
    QueryFilterItem filterItem)
    => entities.Where(p => EF.Functions.Contains(
      p.Interface.Properties.First(prop => prop.Key == filterItem.Key).Value ?? string.Empty,
      $"\"{filterItem.Value}*\""));

  private static IQueryable<InterfaceInfoQueryResult> ApplyDevicePropertyFilter(
    this IQueryable<InterfaceInfoQueryResult> entities,
    QueryFilterItem filterItem)
    => entities.Where(p => EF.Functions.Contains(
      p.Device.Properties.First(prop => prop.Key == filterItem.Key).Value ?? string.Empty,
      $"\"{filterItem.Value}*\""));

  public static IQueryable<StreamInfoQueryResult> ApplyFilter(
    this IQueryable<StreamInfoQueryResult> entities,
    Query query,
    PseudoProperties properties)
  {
    var result = query.Filter.Aggregate(entities, (projects, filterItem) =>
    {
      return filterItem.Key switch
      {
        DeviceName => projects.Where(p
          => EF.Functions.Contains(p.Device.Name, $"\"{filterItem.Value}*\"")),
        InterfaceName => projects.Where(p
          => EF.Functions.Contains(p.Interface.Name, $"\"{filterItem.Value}*\"")),
        Name => projects.Where(p
          => EF.Functions.Contains(p.Stream.Name, $"\"{filterItem.Value}*\"")),
        CompositeName => entities.ApplyCompositeNameFilter(filterItem.Value),
        Comment => projects.Where(p
          => EF.Functions.Contains(p.Stream.Comment ?? string.Empty, $"\"{filterItem.Value}*\"")),
        DeviceType => projects.Where(p
          => EF.Functions.Contains(p.Device.DeviceType, $"\"{filterItem.Value}*\"")),
        FullQualifiedDomainNameRealTimeA => projects.ApplyDevicePropertyFilter(filterItem),
        Created => projects,
        Modified => projects,
        _ => properties.GetPropertyTypeFromString(filterItem.Key) switch
        {
          PropertyType.Undefined => projects,
          PropertyType.General => projects,
          PropertyType.Device => projects.ApplyDevicePropertyFilter(filterItem),
          PropertyType.Interface => projects.ApplyInterfacePropertyFilter(filterItem),
          PropertyType.Stream => projects.ApplyStreamPropertyFilter(filterItem),
          _ => projects.ApplyStreamPropertyFilter(filterItem),
        },
      };
    });

    if (!string.IsNullOrWhiteSpace(query.GlobalFilter))
    {
      result = result.Where(p => EF.Functions.Contains(p.Stream.Name, $"\"{query.GlobalFilter}*\"")
                                 || EF.Functions.Contains(p.Interface.Name, $"\"{query.GlobalFilter}*\"")
                                 || EF.Functions.Contains(p.Device.Name, $"\"{query.GlobalFilter}*\"")
                                 || EF.Functions.Contains(p.Stream.Comment ?? string.Empty,
                                   $"\"{query.GlobalFilter}*\"")
                                 || EF.Functions.Contains(p.Stream.Modifier ?? string.Empty,
                                   $"\"{query.GlobalFilter}*\"")
                                 || EF.Functions.Contains(p.Stream.Creator ?? string.Empty,
                                   $"\"{query.GlobalFilter}*\"")
                                 || p.Stream.Properties.Any(prop
                                   => EF.Functions.Contains(prop.Value ?? string.Empty, $"\"{query.GlobalFilter}*\""))
                                 || p.Interface.Properties.Any(prop
                                   => EF.Functions.Contains(prop.Value ?? string.Empty, $"\"{query.GlobalFilter}*\""))
                                 || p.Device.Properties.Any(prop
                                   => EF.Functions.Contains(prop.Value ?? string.Empty, $"\"{query.GlobalFilter}*\""))
      );
    }

    return result;
  }

  private static IQueryable<StreamInfoQueryResult> ApplyStreamPropertyFilter(
    this IQueryable<StreamInfoQueryResult> entities,
    QueryFilterItem filterItem)
    => entities.Where(p => EF.Functions.Contains(
      p.Stream.Properties.First(prop => prop.Key == filterItem.Key).Value ?? string.Empty,
      $"\"{filterItem.Value}*\""));

  private static IQueryable<StreamInfoQueryResult> ApplyInterfacePropertyFilter(
    this IQueryable<StreamInfoQueryResult> entities,
    QueryFilterItem filterItem)
    => entities.Where(p => EF.Functions.Contains(
      p.Interface.Properties.First(prop => prop.Key == filterItem.Key).Value ?? string.Empty,
      $"\"{filterItem.Value}*\""));

  private static IQueryable<StreamInfoQueryResult> ApplyDevicePropertyFilter(
    this IQueryable<StreamInfoQueryResult> entities,
    QueryFilterItem filterItem)
    => entities.Where(p => EF.Functions.Contains(
      p.Device.Properties.First(prop => prop.Key == filterItem.Key).Value ?? string.Empty,
      $"\"{filterItem.Value}*\""));

  private static IQueryable<StreamInfoQueryResult> ApplyCompositeNameFilter(
    this IQueryable<StreamInfoQueryResult> entities,
    string value)
  {
    var pattern = $"\"{value}*\"";
    return entities.Where(s =>
      EF.Functions.Contains(s.Stream.Name, pattern) ||
      EF.Functions.Contains(s.Interface.Name, pattern) ||
      EF.Functions.Contains(s.Device.Name, pattern)
    );
  }

  public static IOrderedQueryable<DeviceInfoQueryResult> ApplySorting(
    this IQueryable<DeviceInfoQueryResult> entities,
    Query query)
  {
    using var span = Telemetry.Span();
    if (string.IsNullOrWhiteSpace(query.Order) || string.IsNullOrWhiteSpace(query.Sort))
      return ApplySortOrder(entities, e => e.Device.Name, SortOrder.Ascending);

    var sorts = query.Sort.Split(',', StringSplitOptions.RemoveEmptyEntries);
    var sortOrders = query.Order.Split(',', StringSplitOptions.RemoveEmptyEntries);

    if (sorts.Length == 0) return ApplySortOrder(entities, e => e.Device.Name, SortOrder.Ascending);

    var sortedEntities = Array.Empty<DeviceInfoQueryResult>().AsQueryable().Order();
    for (var i = 0; i < sorts.Length; i++)
    {
      var j = i;
      var sortOrder = GetSortOrder(sortOrders[i]);
      if (i == 0)
      {
        sortedEntities = sorts[i] switch
        {
          Name => ApplySortOrder(entities, e => e.Device.Name, sortOrder),
          DeviceType => ApplySortOrder(entities, e => e.Device.DeviceType, sortOrder),
          Comment => ApplySortOrder(entities, e => e.Device.Comment, sortOrder),
          Created => ApplySortOrder(entities, e => e.Device.Created, sortOrder),
          Modified => ApplySortOrder(entities, e => e.Device.Modified, sortOrder),
          InterfaceCount => ApplySortOrder(entities, e => e.Denormalized.InterfaceCount, sortOrder),
          _ => ApplySortOrder(entities, e => e.Device.Properties.First(p => p.Key == sorts[j]).Value, sortOrder),
        };
      }

      sortedEntities = sorts[i] switch
      {
        Name => ApplyThenSortOrder(sortedEntities, e => e.Device.Name, sortOrder),
        DeviceType => ApplyThenSortOrder(sortedEntities, e => e.Device.DeviceType, sortOrder),
        Comment => ApplyThenSortOrder(sortedEntities, e => e.Device.Comment, sortOrder),
        Created => ApplyThenSortOrder(sortedEntities, e => e.Device.Created, sortOrder),
        Modified => ApplyThenSortOrder(sortedEntities, e => e.Device.Modified, sortOrder),
        InterfaceCount => ApplyThenSortOrder(sortedEntities, e => e.Denormalized.InterfaceCount, sortOrder),
        _ => ApplyThenSortOrder(sortedEntities, e => e.Device.Properties.First(p => p.Key == sorts[j]).Value,
          sortOrder),
      };
    }

    return sortedEntities;
  }

  public static IOrderedQueryable<InterfaceInfoQueryResult> ApplySorting(
    this IQueryable<InterfaceInfoQueryResult> entities,
    Query query)
  {
    if (string.IsNullOrWhiteSpace(query.Order) || string.IsNullOrWhiteSpace(query.Sort))
      return ApplySortOrder(entities, e => e.Interface.Index, SortOrder.Ascending);

    var sorts = query.Sort.Split(',', StringSplitOptions.RemoveEmptyEntries);
    var sortOrders = query.Order.Split(',', StringSplitOptions.RemoveEmptyEntries);

    if (sorts.Length == 0) return ApplySortOrder(entities, e => e.Interface.Index, SortOrder.Ascending);

    var sortedEntities = Array.Empty<InterfaceInfoQueryResult>().AsQueryable().Order();

    for (var i = 0; i < sorts.Length; i++)
    {
      var j = i;
      var sortOrder = GetSortOrder(sortOrders[i]);
      if (i == 0)
      {
        sortedEntities = sorts[i] switch
        {
          Name => ApplySortOrder(entities, e => e.Interface.Name, sortOrder),
          DeviceName => ApplySortOrder(entities, e => e.Device.Name, sortOrder),
          DeviceType => ApplySortOrder(entities, e => e.Device.DeviceType, sortOrder),
          Comment => ApplySortOrder(entities, e => e.Interface.Comment, sortOrder),
          Created => ApplySortOrder(entities, e => e.Interface.Created, sortOrder),
          Modified => ApplySortOrder(entities, e => e.Interface.Modified, sortOrder),
          StreamsCountVideoSend => ApplySortOrder(entities, e => e.Denormalized.StreamsCountVideoSend, sortOrder),
          StreamsCountAudioSend => ApplySortOrder(entities, e => e.Denormalized.StreamsCountAudioSend, sortOrder),
          StreamsCountAncillarySend => ApplySortOrder(entities, e => e.Denormalized.StreamsCountAncillarySend,
            sortOrder),
          StreamsCountVideoReceive => ApplySortOrder(entities, e => e.Denormalized.StreamsCountVideoReceive, sortOrder),
          StreamsCountAudioReceive => ApplySortOrder(entities, e => e.Denormalized.StreamsCountAudioReceive, sortOrder),
          StreamsCountAncillaryReceive => ApplySortOrder(entities, e => e.Denormalized.StreamsCountAncillaryReceive,
            sortOrder),
          _ => ApplySortOrder(entities, e => e.Interface.Properties.First(p => p.Key == sorts[j]).Value, sortOrder),
        };
      }

      sortedEntities = sorts[i] switch
      {
        Name => ApplyThenSortOrder(sortedEntities, e => e.Interface.Name, sortOrder),
        DeviceName => ApplyThenSortOrder(sortedEntities, e => e.Device.Name, sortOrder),
        DeviceType => ApplyThenSortOrder(sortedEntities, e => e.Device.DeviceType, sortOrder),
        Comment => ApplyThenSortOrder(sortedEntities, e => e.Interface.Comment, sortOrder),
        Created => ApplyThenSortOrder(sortedEntities, e => e.Interface.Created, sortOrder),
        Modified => ApplyThenSortOrder(sortedEntities, e => e.Interface.Modified, sortOrder),
        StreamsCountVideoSend => ApplyThenSortOrder(sortedEntities, e => e.Denormalized.StreamsCountVideoSend,
          sortOrder),
        StreamsCountAudioSend => ApplyThenSortOrder(sortedEntities, e => e.Denormalized.StreamsCountAudioSend,
          sortOrder),
        StreamsCountAncillarySend => ApplyThenSortOrder(sortedEntities, e => e.Denormalized.StreamsCountAncillarySend,
          sortOrder),
        StreamsCountVideoReceive => ApplyThenSortOrder(sortedEntities, e => e.Denormalized.StreamsCountVideoReceive,
          sortOrder),
        StreamsCountAudioReceive => ApplyThenSortOrder(sortedEntities, e => e.Denormalized.StreamsCountAudioReceive,
          sortOrder),
        StreamsCountAncillaryReceive => ApplyThenSortOrder(sortedEntities,
          e => e.Denormalized.StreamsCountAncillaryReceive, sortOrder),
        _ => ApplyThenSortOrder(sortedEntities, e => e.Interface.Properties.First(p => p.Key == sorts[j]).Value,
          sortOrder),
      };
    }

    return sortedEntities;
  }

  public static IOrderedQueryable<StreamInfoQueryResult> ApplySorting(
    this IQueryable<StreamInfoQueryResult> entities,
    Query query)
  {
    if (string.IsNullOrWhiteSpace(query.Order) || string.IsNullOrWhiteSpace(query.Sort))
      return ApplySortOrder(entities, e => e.Stream.Name, SortOrder.Ascending);

    var sorts = query.Sort.Split(',', StringSplitOptions.RemoveEmptyEntries);
    var sortOrders = query.Order.Split(',', StringSplitOptions.RemoveEmptyEntries);

    if (sorts.Length == 0) return ApplySortOrder(entities, e => e.Interface.Index, SortOrder.Ascending);

    var sortedEntities = Array.Empty<StreamInfoQueryResult>().AsQueryable().Order();

    for (var i = 0; i < sorts.Length; i++)
    {
      var j = i;
      var sortOrder = GetSortOrder(sortOrders[i]);
      if (i == 0)
      {
        sortedEntities = sorts[i] switch
        {
          Name => ApplySortOrder(entities, e => e.Stream.Name, sortOrder),
          InterfaceName => ApplySortOrder(entities, e => e.Interface.Name, sortOrder),
          DeviceName => ApplySortOrder(entities, e => e.Device.Name, sortOrder),
          DeviceType => ApplySortOrder(entities, e => e.Device.DeviceType, sortOrder),
          FullQualifiedDomainNameRealTimeA => ApplySortOrder(entities,
            e => e.Device.Properties.First(p => p.Key == sorts[j]).Value, sortOrder),
          Driver => ApplySortOrder(entities, e => e.Device.Properties.First(p => p.Key == sorts[j]).Value, sortOrder),
          CompositeName => ApplyCompositeNameOrder(entities, sortOrder),
          Comment => ApplySortOrder(entities, e => e.Stream.Comment, sortOrder),
          Created => ApplySortOrder(entities, e => e.Stream.Created, sortOrder),
          Modified => ApplySortOrder(entities, e => e.Stream.Modified, sortOrder),
          _ => ApplySortOrder(entities, e => e.Stream.Properties.First(p => p.Key == sorts[j]).Value, sortOrder),
        };
      }
      else
      {
        sortedEntities = sorts[i] switch
        {
          Name => ApplyThenSortOrder(sortedEntities, e => e.Stream.Name, sortOrder),
          InterfaceName => ApplyThenSortOrder(sortedEntities, e => e.Interface.Name, sortOrder),
          DeviceName => ApplyThenSortOrder(sortedEntities, e => e.Device.Name, sortOrder),
          DeviceType => ApplyThenSortOrder(sortedEntities, e => e.Device.DeviceType, sortOrder),
          FullQualifiedDomainNameRealTimeA => ApplyThenSortOrder(sortedEntities,
            e => e.Device.Properties.First(p => p.Key == sorts[j]).Value, sortOrder),
          Driver => ApplyThenSortOrder(sortedEntities, e => e.Device.Properties.First(p => p.Key == sorts[j]).Value,
            sortOrder),
          CompositeName => ApplyCompositeNameOrder(sortedEntities, sortOrder),
          Comment => ApplyThenSortOrder(sortedEntities, e => e.Stream.Comment, sortOrder),
          Created => ApplyThenSortOrder(sortedEntities, e => e.Stream.Created, sortOrder),
          Modified => ApplyThenSortOrder(sortedEntities, e => e.Stream.Modified, sortOrder),
          _ => ApplyThenSortOrder(sortedEntities, e => e.Stream.Properties.First(p => p.Key == sorts[j]).Value,
            sortOrder),
        };
      }
    }

    return sortedEntities;
  }

  private static IOrderedQueryable<StreamInfoQueryResult> ApplyCompositeNameOrder(
    this IQueryable<StreamInfoQueryResult> entities,
    SortOrder sortOrder)
  {
    return ApplySortOrder(entities, e => e.Device.Name, sortOrder)
      .ApplyApplyCompositeNameOrderInterfaceAndStreamName(sortOrder);
  }

  private static IOrderedQueryable<StreamInfoQueryResult> ApplyCompositeNameOrder(
    this IOrderedQueryable<StreamInfoQueryResult> entities,
    SortOrder sortOrder)
  {
    return ApplyThenSortOrder(entities, e => e.Device.Name, sortOrder)
      .ApplyApplyCompositeNameOrderInterfaceAndStreamName(sortOrder);
  }

  private static IOrderedQueryable<StreamInfoQueryResult> ApplyApplyCompositeNameOrderInterfaceAndStreamName(
    this IOrderedQueryable<StreamInfoQueryResult> entities,
    SortOrder sortOrder)
  {
    var result = ApplyThenSortOrder(entities, e => e.Interface.Name, sortOrder);
    ApplyThenSortOrder(result, e => e.Stream.Name, sortOrder);

    return result;
  }

  private static SortOrder GetSortOrder(string sortOrder)
  {
    var order = SortOrder.None;
    if (string.Equals(sortOrder, "asc", StringComparison.OrdinalIgnoreCase)
        || string.Equals(sortOrder, "ascending", StringComparison.OrdinalIgnoreCase))
    {
      order = SortOrder.Ascending;
    }

    if (string.Equals(sortOrder, "desc", StringComparison.OrdinalIgnoreCase)
        || string.Equals(sortOrder, "descending", StringComparison.OrdinalIgnoreCase))
    {
      order = SortOrder.Descending;
    }

    return order;
  }

  private static IOrderedQueryable<TEntity> ApplySortOrder<TEntity, TKey>(
    IQueryable<TEntity> entities,
    Expression<Func<TEntity, TKey>> keySelector,
    SortOrder order)
    => order switch
    {
      SortOrder.Ascending => entities.OrderBy(keySelector),
      SortOrder.Descending => entities.OrderByDescending(keySelector),
      _ => throw new NotImplementedException(),
    };

  private static IOrderedQueryable<TEntity> ApplyThenSortOrder<TEntity, TKey>(
    IOrderedQueryable<TEntity> entities,
    Expression<Func<TEntity, TKey>> keySelector,
    SortOrder order)
    => order switch
    {
      SortOrder.Ascending => entities.ThenBy(keySelector),
      SortOrder.Descending => entities.ThenByDescending(keySelector),
      _ => throw new NotImplementedException(),
    };
}
