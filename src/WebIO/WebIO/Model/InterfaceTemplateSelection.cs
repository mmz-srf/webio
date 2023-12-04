namespace WebIO.Model;

using DeviceTemplates;

public class InterfaceTemplateSelection
{
  public InterfaceTemplateSelection(InterfaceTemplate template, StreamCardinality? streams = null)
  {
    Template = template;
    Streams = streams;
  }

  public bool FlexibleStreams => Streams is not null;

  public InterfaceTemplate Template { get; }

  public StreamCardinality? Streams { get; }
}
