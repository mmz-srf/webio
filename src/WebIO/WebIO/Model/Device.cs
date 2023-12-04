namespace WebIO.Model;

using System;
using System.Collections.Generic;

public class Device : IHaveProperties
{
  public Guid Id { get; set; } = Guid.NewGuid();

  public string Name { get; set; } = null!;

  public string DeviceType { get; init; } = null!;

  public string Comment { get; set; } = null!;

  public FieldValues Properties { get; init; } = new();

  public List<Interface> Interfaces { get; init; } = new();

  public Modification Modification { get; init; } = null!;
}
