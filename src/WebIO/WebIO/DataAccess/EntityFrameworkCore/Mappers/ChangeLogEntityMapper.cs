namespace WebIO.DataAccess.EntityFrameworkCore.Mappers;

using System;
using Entities;
using Model;
using Model.Readonly;
using Newtonsoft.Json;

public static class ChangeLogEntityMapper
{
  public static ChangeLogEntryEntity ToEntity(this ChangeLogEntry entry)
  {
    return new()
    {
      Id = entry.Id,
      Timestamp = entry.Timestamp,
      Username = entry.Username,
      Comment = entry.Comment,
      Summary = entry.Summary,
      FullInfo = JsonConvert.SerializeObject(entry.FullDetails),
    };
  }

  public static ChangeLogEntryInfo ToReadonly(this ChangeLogEntryEntity entry)
    => new(
      entry.Timestamp,
      entry.Username,
      entry.Comment,
      entry.Summary,
      JsonConvert.DeserializeObject(entry.FullInfo) ?? throw new InvalidOperationException("Json was empty!"));
}
