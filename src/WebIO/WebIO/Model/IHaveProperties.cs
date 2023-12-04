namespace WebIO.Model;

using System;

public interface IHaveProperties
{
  Guid Id { get; }

  string Comment { get; set; }

  string Name { get; set; }

  Modification Modification { get; }

  FieldValues Properties { get; }

}
