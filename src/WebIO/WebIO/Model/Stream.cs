namespace WebIO.Model;

using System;
using Elastic.Data;

public class Stream : IHaveProperties
{
  public Guid Id { get; set; } = Guid.NewGuid();

  public string Name { get; set; } = string.Empty;

  public string Comment { get; set; } = string.Empty;

  public StreamType Type { get; init; }

  public StreamDirection Direction { get; init; }

  public required Modification Modification { get; init; } = null!;

  public FieldValues Properties { get; init; } = new();
}
