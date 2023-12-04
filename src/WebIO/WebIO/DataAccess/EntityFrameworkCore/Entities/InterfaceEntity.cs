namespace WebIO.DataAccess.EntityFrameworkCore.Entities;

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class InterfaceEntity : IHaveModificationInfo, IAmAnEntityWithProperties<InterfacePropertyValueEntity>
{
  [Key] [Column("ID")] public Guid Id { get; init; }

  public Guid DeviceId { get; init; }

  [MaxLength(100)] public required string Name { get; set; }

  public int Index { get; init; }

  [MaxLength(100)] public required string InterfaceTemplate { get; set; }

  public string? Comment { get; set; } = string.Empty;

  public List<InterfacePropertyValueEntity> Properties { get; set; } = new();

  [MaxLength(100)] public string? Creator { get; set; } = string.Empty;
  public DateTime Created { get; set; }
  [MaxLength(100)] public string? Modifier { get; set; } = string.Empty;
  public DateTime Modified { get; set; }
  public string? ModificationComment { get; set; } = string.Empty;
}
