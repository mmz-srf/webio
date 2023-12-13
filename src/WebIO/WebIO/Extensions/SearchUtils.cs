namespace WebIO.Extensions;

using System.Collections.Generic;
using System.Linq;
using Elastic.Data;

public static class SearchUtils
{
  private static readonly IEnumerable<string> DeviceFields = typeof(IndexedDevice).GetProperties().Select(p => p.Name);
  private static readonly IEnumerable<string> IfaceFields = typeof(IndexedInterface).GetProperties().Select(p => p.Name);
  private static readonly IEnumerable<string> StreamFields = typeof(IndexedStream).GetProperties().Select(p => p.Name);
  private static readonly string[] DummyFields = { "Name" };

  public static bool IsBaseTypeField(string field)
    => DeviceFields.Concat(IfaceFields).Concat(StreamFields).Concat(DummyFields).Contains(field);
}
