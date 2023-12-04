namespace WebIO.DataAccess.EntityFrameworkCore.Entities;

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WebIO.Elastic.Data;

public class StreamEntity : IHaveModificationInfo, IAmAnEntityWithProperties<StreamPropertyValueEntity>
{
  [Key] [Column("ID")] public Guid Id { get; init; }

  public Guid InterfaceId { get; init; }

  [MaxLength(100)] public required string Name { get; set; }

  public string? Comment { get; set; } = string.Empty;

  public StreamType Type { get; set; }

  public StreamDirection Direction { get; set; }

  public List<StreamPropertyValueEntity> Properties { get; set; } = new();

  [MaxLength(100)] public string? Creator { get; set; } = string.Empty;
  public DateTime Created { get; set; }
  [MaxLength(100)] public string? Modifier { get; set; } = string.Empty;
  public DateTime Modified { get; set; }
  public string? ModificationComment { get; set; } = string.Empty;
}