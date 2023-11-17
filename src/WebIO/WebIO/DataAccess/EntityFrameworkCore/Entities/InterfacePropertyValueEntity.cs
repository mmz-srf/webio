namespace WebIO.DataAccess.EntityFrameworkCore.Entities;

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

[Table("InterfaceProperties")]
public class InterfacePropertyValueEntity : IAmAPropertyValueEntity
{
    public InterfaceEntity Interface { get; init; } = null!;

    [Key]
    [Column("ID")]
    public Guid Id { get; set; }
    [MaxLength(100)]
    public string Key { get; init; } = null!;

    public string? Value { get; set; } = null!;
}