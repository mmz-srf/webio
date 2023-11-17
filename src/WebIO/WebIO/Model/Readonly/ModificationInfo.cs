namespace WebIO.Model.Readonly;

using System;

public record ModificationInfo(
  string Creator,
  DateTime Created,
  string Modifier,
  DateTime Modified,
  string Comment);
