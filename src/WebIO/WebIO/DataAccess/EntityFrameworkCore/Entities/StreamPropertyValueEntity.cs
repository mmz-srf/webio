namespace WebIO.DataAccess.EntityFrameworkCore.Entities;

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

[Table("StreamProperties")]
public class StreamPropertyValueEntity : IAmAPropertyValueEntity
{
  public StreamEntity Stream { get; init; } = null!;

  [Key] [Column("ID")] public Guid Id { get; set; }

  [MaxLength(100)] public string Key { get; init; } = string.Empty;
  public string? Value { get; set; } = string.Empty;
}
