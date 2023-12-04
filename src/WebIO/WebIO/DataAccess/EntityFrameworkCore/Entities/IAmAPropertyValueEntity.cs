namespace WebIO.DataAccess.EntityFrameworkCore.Entities;

using System;

public interface IAmAPropertyValueEntity
{
    Guid Id { get; set; }
    string Key { get; init; }
    string? Value { get; set; }
}