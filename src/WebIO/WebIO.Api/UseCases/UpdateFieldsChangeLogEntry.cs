namespace WebIO.Api.UseCases;

using Model;

public class UpdateFieldsChangeLogEntry : IChangeLogDetails
{
  public List<ChangeInfo> Changes { get; init; } = new();
}
