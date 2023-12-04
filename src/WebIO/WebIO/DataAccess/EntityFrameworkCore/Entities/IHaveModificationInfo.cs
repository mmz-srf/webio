namespace WebIO.DataAccess.EntityFrameworkCore.Entities;

using System;

public interface IHaveModificationInfo
{
  string? Creator { get; set; }
  DateTime Created { get; set; }
  string? Modifier { get; set; }
  DateTime Modified { get; set; }
  string? ModificationComment { get; set; }
}
