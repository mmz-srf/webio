namespace WebIO.DataAccess.EntityFrameworkCore.Entities;

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

[Table("DeviceProperties")]
public class DevicePropertyValueEntity : IAmAPropertyValueEntity
{
  public DeviceEntity Device { get; init; } = null!;

  [Key] [Column("ID")] public Guid Id { get; set; }
  [MaxLength(100)] public string Key { get; init; } = string.Empty;

  public string? Value { get; set; } = string.Empty!;
}
