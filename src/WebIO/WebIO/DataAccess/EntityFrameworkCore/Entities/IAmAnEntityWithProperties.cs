namespace WebIO.DataAccess.EntityFrameworkCore.Entities;

using System.Collections.Generic;

public interface IAmAnEntityWithProperties<T> where T : IAmAPropertyValueEntity
{
    List<T> Properties { get; set; }
}