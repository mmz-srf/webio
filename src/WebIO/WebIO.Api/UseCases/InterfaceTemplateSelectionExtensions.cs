namespace WebIO.Api.UseCases;

using Controllers.Dto;
using Model;
using Model.DeviceTemplates;

public static class InterfaceTemplateSelectionExtensions
{
  public static IList<InterfaceTemplateSelection> GetTemplateSelections(
    this IEnumerable<InterfaceTemplateSelectionDto> input,
    DeviceType deviceType)
  {
    if (deviceType.InterfaceStreamsFlexible)
    {
      return input
        .Select(f => new InterfaceTemplateSelection(
          deviceType.InterfaceTemplates.First(t => t.Name == f.TemplateName),
          f.GetStreamCardinality()))
        .ToList();
    }

    return input
      .Select(it => deviceType.InterfaceTemplates.First(t => t.Name == it.TemplateName))
      .Select(t => new InterfaceTemplateSelection(t))
      .ToList();
  }

  private static StreamCardinality GetStreamCardinality(this InterfaceTemplateSelectionDto dto)
    => new(dto.VideoSend, dto.AudioSend, dto.AncillarySend, dto.VideoReceive, dto.AudioReceive, dto.AncillaryReceive);
}
