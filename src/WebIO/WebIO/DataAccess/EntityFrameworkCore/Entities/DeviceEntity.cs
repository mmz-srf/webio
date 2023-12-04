namespace WebIO.DataAccess.EntityFrameworkCore.Entities;

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class DeviceEntity : IHaveModificationInfo, IAmAnEntityWithProperties<DevicePropertyValueEntity>
{
  [Key] [Column("ID")] public Guid Id { get; init; }

  [MaxLength(100)] public string Name { get; set; } = null!;

  [MaxLength(100)] public string DeviceType { get; init; } = null!;

  public string? Comment { get; set; } = string.Empty;

  public List<DevicePropertyValueEntity> Properties { get; set; } = new();

  [MaxLength(100)] public string? Creator { get; set; } = string.Empty;

  public DateTime Created { get; set; }

  [MaxLength(100)] public string? Modifier { get; set; } = string.Empty;

  public DateTime Modified { get; set; }

  public string? ModificationComment { get; set; } = string.Empty;
}
