namespace WebIO.Model;

using System;
using System.Collections.Generic;
using System.Linq;
using Application;
using DataAccess;
using DeviceTemplates;
using Elastic.Data;
using Microsoft.Extensions.Logging;

public class DeviceCreation
{
  private const string Smpte20227 = "Smpte20227";
  private readonly ILogger _log;
  private readonly List<DataField> _dataFields;

  private bool _useSt20227;

  public DeviceCreation(IMetadataRepository metadata, ILogger log)
  {
    _log = log;
    _dataFields = metadata.DataFields.ToList();
  }

  public Device CreateDevice(
    DeviceType deviceType,
    ModifyArgs modifyArgs,
    string deviceName,
    string comment,
    IEnumerable<InterfaceTemplateSelection> interfaces,
    bool useSt20227 = false)
  {
    using var span = Telemetry.Span();
    _useSt20227 = useSt20227;

    var device = new Device
    {
      Name = deviceName,
      DeviceType = deviceType.Name ?? throw new ArgumentNullException(nameof(deviceName)),
      Comment = comment,
      Modification = new(modifyArgs),
    };

    // set field values
    SetFieldValues(device, deviceType.FieldValues, _dataFields);

    // create interfaces
    foreach (var interfaceDefinition in interfaces)
    {
      var interfaceIndex = device.Interfaces.Count + 1;
      CreateInterface(deviceType, modifyArgs, interfaceIndex, device, interfaceDefinition);
    }

    AddFixedProperties(device);

    return device;
  }

  public void CreateInterface(
    DeviceType deviceType,
    ModifyArgs modifyArgs,
    int index,
    Device device,
    InterfaceTemplateSelection interfaceDefinition)
  {
    using var span = Telemetry.Span();

    var iface = new Interface
    {
      Index = index,
      Name = string.Format(deviceType.InterfaceNameTemplate!, index),
      InterfaceTemplate = interfaceDefinition.Template.Name,
      Comment = modifyArgs.Comment,
      Modification = new(modifyArgs),
    };
    device.Interfaces.Add(iface);
    SetFieldValues(iface, interfaceDefinition.Template.FieldValues, _dataFields);
    iface.Streams.AddRange(CreateStreams(modifyArgs, interfaceDefinition, iface));
  }

  private IEnumerable<Stream> CreateStreams(
    ModifyArgs modifyArgs,
    InterfaceTemplateSelection templateSelection,
    Interface iface)
  {
    using var span = Telemetry.Span();
    var streams = new List<Stream>();
    if (templateSelection.FlexibleStreams)
    {
      streams.AddRange(CreateFlexibleStreams(modifyArgs, iface, templateSelection, StreamDirection.Send,
        StreamType.Audio));
      streams.AddRange(CreateFlexibleStreams(modifyArgs, iface, templateSelection, StreamDirection.Receive,
        StreamType.Audio));
      streams.AddRange(CreateFlexibleStreams(modifyArgs, iface, templateSelection, StreamDirection.Send,
        StreamType.Video));
      streams.AddRange(CreateFlexibleStreams(modifyArgs, iface, templateSelection, StreamDirection.Receive,
        StreamType.Video));
      streams.AddRange(CreateFlexibleStreams(modifyArgs, iface, templateSelection, StreamDirection.Send,
        StreamType.Ancillary));
      streams.AddRange(CreateFlexibleStreams(modifyArgs, iface, templateSelection, StreamDirection.Receive,
        StreamType.Ancillary));
    }
    else
    {
      foreach (var streamTemplate in templateSelection.Template.Streams)
      {
        // streams are numbered per direction and type
        var streamIndex = iface.Streams
          .Count(s => MatchesDirectionAndType(s, streamTemplate)) + 1;
        foreach (var jj in Enumerable.Range(streamIndex, streamTemplate.Count - streamIndex + 1))
        {
          var stream = new Stream
          {
            Name = GetStreamName(streamTemplate, streamTemplate.Type, streamTemplate.Direction, jj),
            Comment = modifyArgs.Comment,
            Type = streamTemplate.Type,
            Direction = streamTemplate.Direction,
            Modification = new(modifyArgs),
          };
          streams.Add(stream);

          // set field values
          SetFieldValues(stream, streamTemplate.FieldValues, _dataFields);

          //Add fixed properties
          AddFixedProperties(stream);
        }
      }
    }

    return streams;
  }

  private static bool MatchesDirectionAndType(Stream s, StreamTemplate streamTemplate)
    => s.Direction == streamTemplate.Direction
       && s.Type == streamTemplate.Type;

  private void AddFixedProperties(IHaveProperties stream)
  {
    //smpte2022.7
    var field = new DataField(Smpte20227);
    stream.Properties[field] = _useSt20227 ? "true" : "false";
  }

  private List<Stream> CreateFlexibleStreams(
    ModifyArgs modifyArgs,
    Interface iface,
    InterfaceTemplateSelection templateSelection,
    StreamDirection direction,
    StreamType type)
  {
    var streams = new List<Stream>();
    var count = templateSelection.Streams!.GetCount(direction, type);
    var streamIndex = iface.Streams
      .Where(s => s.Direction == direction)
      .Count(s => s.Type == type) + 1;

    var streamTemplate =
      templateSelection.Template.Streams.FirstOrDefault(i => i.Direction == direction && i.Type == type);

    foreach (var jj in Enumerable.Range(streamIndex, count - streamIndex + 1))
    {
      var stream = new Stream
      {
        Name = GetStreamName(streamTemplate!, type, direction, jj),
        Comment = modifyArgs.Comment,
        Type = type,
        Direction = direction,
        Modification = new(modifyArgs),
      };
      streams.Add(stream);

      if (streamTemplate != null)
      {
        SetFieldValues(stream, streamTemplate.FieldValues, _dataFields);
      }

      //Add fixed properties
      AddFixedProperties(stream);
    }

    return streams;
  }

  private string GetStreamName(
    StreamTemplate streamTemplate,
    StreamType type,
    StreamDirection direction,
    int streamIndex)
  {
    var nameTemplate = streamTemplate.NameTemplate;
    if (string.IsNullOrEmpty(nameTemplate))
    {
      nameTemplate = type switch
      {
        StreamType.Video when direction == StreamDirection.Send => "VIDsend_{0:0000}",
        StreamType.Audio when direction == StreamDirection.Send => "Audsend_{0:0000}",
        StreamType.Ancillary when direction == StreamDirection.Send => "ANCsend_{0:0000}",
        StreamType.Video when direction == StreamDirection.Receive => "VIDrec_{0:0000}",
        StreamType.Audio when direction == StreamDirection.Receive => "AUDrec_{0:0000}",
        StreamType.Ancillary when direction == StreamDirection.Receive => "ANCrec_{0:0000}",
        _ => "???_{0:0000}",
      };
    }

    return string.Format(nameTemplate, streamIndex);
  }

  private void SetFieldValues(
    IHaveProperties target,
    IEnumerable<TemplateFieldValue> fieldValues,
    IList<DataField> dataFields)
  {
    using var span = Telemetry.Span();
    foreach (var fieldValue in fieldValues)
    {
      SetProperty(fieldValue, target, dataFields);
    }
  }

  private void SetProperty(
    TemplateFieldValue fieldValue,
    IHaveProperties target,
    IEnumerable<DataField> fields)
  {
    var field = fields.FirstOrDefault(
      f => string.Equals(f.Key, fieldValue.FieldKey, StringComparison.OrdinalIgnoreCase));

    if (field == null)
    {
      _log.LogError("{Field} is not an valid field", fieldValue.FieldKey);
      return;
    }

    // Todo: verify selection, datatypes, etc.

    target.Properties[field] = fieldValue.Value;
  }

  public void UpdateInterface(
    Interface iface,
    ModifyArgs modifyArgs,
    InterfaceTemplateSelection templateSelection)
  {
    using var span = Telemetry.Span();
    iface.InterfaceTemplate = templateSelection.Template.Name;
    iface.Modification.Modify(modifyArgs);

    SetFieldValues(iface, templateSelection.Template.FieldValues, _dataFields);

    iface.Streams.RemoveAll(s => ShouldBeRemoved(s, templateSelection));
    var newStreams = CreateStreams(modifyArgs, templateSelection, iface);
    iface.Streams.AddRange(newStreams);
  }

  private static bool ShouldBeRemoved(Stream stream, InterfaceTemplateSelection template)
    => GetStreamIndex(stream) > GetMaxIndexForStreamTypeAndDirection(stream, template);

  private static int GetMaxIndexForStreamTypeAndDirection(Stream? stream, InterfaceTemplateSelection? template)
  {
    if (stream == null || template == null)
    {
      return int.MaxValue;
    }
      
    return stream.Type switch
    {
      StreamType.Ancillary when stream.Direction == StreamDirection.Receive
        => template.Streams?.AncillaryReceive,
      StreamType.Ancillary when stream.Direction == StreamDirection.Send
        => template.Streams?.AncillarySend,
      StreamType.Audio when stream.Direction == StreamDirection.Receive
        => template.Streams?.AudioReceive,
      StreamType.Audio when stream.Direction == StreamDirection.Send
        => template.Streams?.AudioSend,
      StreamType.Video when stream.Direction == StreamDirection.Receive
        => template.Streams?.VideoReceive,
      StreamType.Video when stream.Direction == StreamDirection.Send
        => template.Streams?.VideoSend,
      _ => null,
    } ?? int.MaxValue;
  }

  private static int GetStreamIndex(Stream stream)
  {
    var indexString = stream.Name.Split('_').LastOrDefault();
    return int.TryParse(indexString, out var idx) ? idx : 0;
  }
}