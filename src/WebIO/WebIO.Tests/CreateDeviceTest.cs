namespace WebIO.Tests;

using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Api.Controllers.Dto;
using Api.UseCases;
using DataAccess;
using Elastic.Data;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using MockMetadata;
using Model;
using Moq;
using Xunit;

public class CreateDeviceTest
{
  private readonly MockMetadataRepository _metadataMock;
  private readonly Mock<IDeviceRepository> _repositoryMock;
  private readonly Mock<IChangeLogRepository> _changeLogMock;

  private Device? _created;
  private ChangeLogEntry? _logEntry;
  private readonly CreateDeviceUseCase _useCase;

  public CreateDeviceTest()
  {
    _metadataMock = new();

    _repositoryMock = new();
    _repositoryMock.Setup(r => r.UpsertAsync(It.IsAny<Device>(), It.IsAny<CancellationToken>()))
      .Callback<Device, CancellationToken>((d,_) => _created = d);

    _changeLogMock = new();
    _changeLogMock.Setup(r => r.Add(It.IsAny<ChangeLogEntry>()))
      .Callback<ChangeLogEntry>(e => _logEntry = e);

    _useCase = new(_metadataMock, _repositoryMock.Object, _changeLogMock.Object, new NullLogger<CreateDeviceUseCase>());
  }

  [Fact]
  public async Task ValidateFailsWithoutDeviceType()
  {
    var useCase = new CreateDeviceUseCase(_metadataMock, _repositoryMock.Object, _changeLogMock.Object,
      new NullLogger<CreateDeviceUseCase>());
    useCase.Initialize(new()
    {
      Name = "test",
      DeviceType = "test",
    }, "Tester");

    (await useCase.ValidateAsync(default))
        .Should().BeFalse();
  }

  [Fact]
  public async Task ValidateSucceedsIfDeviceConfigFound()
  {
    _useCase.Initialize(new()
    {
      Name = "test",
      DeviceType = _metadataMock.DeviceTypes.First().Name ?? string.Empty,
    }, "Tester");

    (await _useCase.ValidateAsync(default))
      .Should()
      .BeTrue();
  }

  [Fact]
  public async Task CreateDeviceWithProperName()
  {
    var deviceType = _metadataMock.MockDeviceTypes.BasicType;

    _useCase.Initialize(new()
    {
      Name = "device name test",
      DeviceType = deviceType.Name ?? string.Empty,
    }, "tester");

    (await _useCase.ValidateAsync(default))
      .Should().BeTrue();
    await _useCase.ExecuteAsync(default);

    _created.Should().NotBeNull();
    _created!.Name.Should().Be("device name test");
    _created.DeviceType.Should().Be(deviceType.Name);
    _created.Modification.Creator.Should().Be("tester");
  }

  [Fact]
  public async Task CreateDeviceWithFieldValues()
  {
    var deviceType = _metadataMock.MockDeviceTypes.BasicType;

    _useCase.Initialize(new()
    {
      Name = "device name test",
      DeviceType = deviceType.Name ?? string.Empty,
    }, "tester");

    (await _useCase.ValidateAsync(default))
      .Should().BeTrue();
    await _useCase.ExecuteAsync(default);

    _created!.Properties.All.Should().ContainKey("Driver").WhoseValue.Should().Be("Test1");
  }

  [Fact]
  public async Task CreateDeviceWithInterfaces()
  {
    var deviceType = _metadataMock.MockDeviceTypes.BasicType;

    var interfaces = deviceType.DefaultInterfaces
      .SelectMany(t => Enumerable.Repeat(t.Template, t.Count))
      .Select(t => new InterfaceTemplateSelectionDto {TemplateName = t})
      .ToList();

    _useCase.Initialize(new()
    {
      Name = "testdevice",
      DeviceType = deviceType.Name ?? string.Empty,
      Interfaces = interfaces,
    }, "tester");

    (await _useCase.ValidateAsync(default)).Should().BeTrue();
    await _useCase.ExecuteAsync(default);

    _created!.Interfaces.Should().HaveCount(2)
      .And.SatisfyRespectively(
        i => i.Name.Should().Be(string.Format(deviceType.InterfaceNameTemplate ?? string.Empty, 1)),
        i => i.Name.Should().Be(string.Format(deviceType.InterfaceNameTemplate ?? string.Empty, 2)));
  }

  [Fact]
  public async Task CreateDeviceWithSwDefinedInterfaceCount()
  {
    var deviceType = _metadataMock.MockDeviceTypes.SwDefinedInterfaces;

    _useCase.Initialize(new()
    {
      Name = "testdevice",
      DeviceType = deviceType.Name ?? string.Empty,
      Interfaces =
      {
        new InterfaceTemplateSelectionDto {TemplateName = "audioReceive"},
        new InterfaceTemplateSelectionDto {TemplateName = "audioReceive"},
        new InterfaceTemplateSelectionDto {TemplateName = "audioReceive"},
      },
    }, "tester");

    (await _useCase.ValidateAsync(default)).Should().BeTrue();
    await _useCase.ExecuteAsync(default);

    _created!.Interfaces.Should().HaveCount(3)
      .And.SatisfyRespectively(
        i => i.Name.Should().Be(string.Format(deviceType.InterfaceNameTemplate ?? string.Empty, 1)),
        i => i.Name.Should().Be(string.Format(deviceType.InterfaceNameTemplate ?? string.Empty, 2)),
        i => i.Name.Should().Be(string.Format(deviceType.InterfaceNameTemplate ?? string.Empty, 3)));

    _created.Interfaces.ForEach(i => i.InterfaceTemplate.Should().Be("audioReceive"));
  }

  [Fact]
  public async Task CreateDeviceWithInterfacesWithProperties()
  {
    var deviceType = _metadataMock.MockDeviceTypes.BasicType;

    var interfaces = deviceType.DefaultInterfaces
      .SelectMany(t => Enumerable.Repeat(t.Template, t.Count))
      .Select(t => new InterfaceTemplateSelectionDto {TemplateName = t})
      .ToList();

    _useCase.Initialize(new()
    {
      Name = "testdevice",
      DeviceType = deviceType.Name ?? string.Empty,
      Interfaces = interfaces,
    }, "tester");

    (await _useCase.ValidateAsync(default)).Should().BeTrue();
    await _useCase.ExecuteAsync(default);

    _created!.Interfaces.Should().HaveCount(2);
    foreach (var i in _created.Interfaces)
    {
      i.Properties.All.Should().ContainKey("FunctionalGroup")
        .WhoseValue.Should().Be("sendGroup");
    }
  }

  [Fact]
  public async Task CreateDeviceWithStreamsWithProperties()
  {
    var deviceType = _metadataMock.MockDeviceTypes.BasicType;

    var interfaces = deviceType.DefaultInterfaces
      .SelectMany(t => Enumerable.Repeat(t.Template, t.Count))
      .Select(t => new InterfaceTemplateSelectionDto {TemplateName = t})
      .ToList();

    _useCase.Initialize(new()
    {
      Name = "testdevice",
      DeviceType = deviceType.Name ?? string.Empty,
      Interfaces = interfaces,
    }, "tester");

    (await _useCase.ValidateAsync(default)).Should().BeTrue();
    await _useCase.ExecuteAsync(default);

    _created!.Interfaces.Should().HaveCount(2);
    var if1 = _created.Interfaces.First();
    if1.InterfaceTemplate.Should().Be("audioSend");

    // stream count
    if1.Streams.Should().HaveCount(1);
    var cardinality1 = if1.GetStreamCardinality();
    cardinality1.VideoSend.Should().Be(0);
    cardinality1.AudioSend.Should().Be(1);
    cardinality1.AncillarySend.Should().Be(0);
    cardinality1.VideoReceive.Should().Be(0);
    cardinality1.AudioReceive.Should().Be(0);
    cardinality1.AncillaryReceive.Should().Be(0);

    // stream properties
    var audSendStream = if1.GetStreams(StreamType.Audio, StreamDirection.Send).Single();
    audSendStream.Properties["Tags"].Should().Be("Tag1");
  }

  [Fact]
  public async Task CreateDeviceWithInterfacesWithFlexibleStreams()
  {
    var deviceType = _metadataMock.MockDeviceTypes.FlexibleStreams;

    _useCase.Initialize(new()
    {
      Name = "testdevice",
      DeviceType = deviceType.Name ?? string.Empty,
      Interfaces =
      {
        new InterfaceTemplateSelectionDto
        {
          TemplateName = "default",
          AudioSend = 3,
          AudioReceive = 1,
        },
        new InterfaceTemplateSelectionDto
        {
          TemplateName = "default",
          VideoSend = 3,
          VideoReceive = 1,
        },
      },
    }, "tester");

    (await _useCase.ValidateAsync(default)).Should().BeTrue();
    await _useCase.ExecuteAsync(default);
    _created!.Interfaces.Should().HaveCount(2);

    // setting of properties
    foreach (var i in _created.Interfaces)
    {
      i.Properties.All.Should().ContainKey("FunctionalGroup")
        .WhoseValue.Should().Be("sendGroup");
    }

    // interface template
    var if1 = _created.Interfaces.First();
    if1.InterfaceTemplate.Should().Be("default");

    // stream count
    if1.Streams.Should().HaveCount(4);
    var cardinality1 = if1.GetStreamCardinality();
    cardinality1.VideoSend.Should().Be(0);
    cardinality1.AudioSend.Should().Be(3);
    cardinality1.AncillarySend.Should().Be(0);
    cardinality1.VideoReceive.Should().Be(0);
    cardinality1.AudioReceive.Should().Be(1);
    cardinality1.AncillaryReceive.Should().Be(0);

    // stream properties
    var audRecStream = if1.GetStreams(StreamType.Audio, StreamDirection.Receive).Single();
    audRecStream.Properties["Tags"].Should().Be("Tag5");
  }

  [Fact]
  public async Task AddLogEntry()
  {
    var deviceType = _metadataMock.MockDeviceTypes.BasicType;

    var interfaces = deviceType.DefaultInterfaces
      .SelectMany(t => Enumerable.Repeat(t.Template, t.Count))
      .Select(t => new InterfaceTemplateSelectionDto {TemplateName = t})
      .ToList();

    _useCase.Initialize(new()
    {
      Name = "testdevice",
      Comment = "comment",
      DeviceType = deviceType.Name ?? string.Empty,
      Interfaces = interfaces,
    }, "tester");

    (await _useCase.ValidateAsync(default)).Should().BeTrue();
    await _useCase.ExecuteAsync(default);

    _logEntry.Should().NotBeNull();
    _logEntry!.Comment.Should().Be("comment");
    _logEntry.FullDetails.Should().BeOfType<CreateDeviceChangeLogEntry>()
      .Which.DeviceName.Should().Be("testdevice");
  }
}
