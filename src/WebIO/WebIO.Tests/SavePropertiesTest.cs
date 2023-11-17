namespace WebIO.Tests;

using System;
using System.Collections.Generic;
using Api.Controllers.Dto;
using Api.UseCases;
using ConfigFiles;
using DataAccess;
using DataAccess.Configuration;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Model;
using Moq;
using Xunit;

public class SavePropertiesTest
{
  private readonly IMetadataRepository _metadata;

  private readonly Mock<IChangeLogRepository> _changeLogMock;
  private ChangeLogEntry? _logEntry;

  public SavePropertiesTest()
  {
    _metadata = new JsonMetadataRepository(new NullLogger<JsonMetadataRepository>(),
      new JsonFileReader(new NullLogger<JsonFileReader>()));

    _changeLogMock = new();
    _changeLogMock.Setup(r => r.Add(It.IsAny<ChangeLogEntry>()))
      .Callback<ChangeLogEntry>(e => _logEntry = e);
  }

  [Fact]
  public void ValidateFailsWithEmptyChanges()
  {
    var useCase = new SaveModificationsUseCase(Mock.Of<IDeviceRepository>(), _metadata, _changeLogMock.Object,
      new NullLogger<SaveModificationsUseCase>());
    useCase.Initialize(new(), "Tester");

    useCase.Validate().Should().BeFalse();
  }

  [Fact]
  public void ValidateFailsWithoutChanges()
  {
    var useCase = new SaveModificationsUseCase(Mock.Of<IDeviceRepository>(), _metadata, _changeLogMock.Object,
      new NullLogger<SaveModificationsUseCase>());

    var changes = new PropertiesChangedSummaryDto
    {
      Comment = "bla",
    };
    useCase.Initialize(changes, "Tester");

    useCase.Validate().Should().BeFalse();
  }

  [Fact]
  public void ChangeNameProperty()
  {
    var device = new Device
    {
      Modification = new(new("original", DateTime.Now, string.Empty)),
    };
    var useCase = InitUseCase(device, "Name", "zwei", "Device");

    // Test
    useCase.Validate().Should().BeTrue();
    useCase.Execute();

    device.Should().NotBeNull();
    device.Name.Should().Be("zwei");
    device.Modification.Comment.Should().Be("new comment");
    device.Modification.Creator.Should().Be("original");
    device.Modification.Modifier.Should().Be("Tester");
  }

  [Fact]
  public void AddLogEntry()
  {
    var device = new Device
    {
      Modification = new(new("original", DateTime.Now, string.Empty)),
    };
    var useCase = InitUseCase(device, "DeviceLocation", "zwei", "Device");

    // Test
    useCase.Validate().Should().BeTrue();
    useCase.Execute();

    _logEntry.Should().NotBeNull();
    _logEntry!.FullDetails.Should().BeOfType<UpdateFieldsChangeLogEntry>()
      .Which.Changes[0].EntityType.Should().Be("Device");
    _logEntry.FullDetails.Should().BeOfType<UpdateFieldsChangeLogEntry>()
      .Which.Changes[0].NewValue.Should().Be("zwei");
    _logEntry.FullDetails.Should().BeOfType<UpdateFieldsChangeLogEntry>()
      .Which.Changes[0].Property.Should().Be("DeviceLocation");
  }

  private SaveModificationsUseCase InitUseCase(
    Device device,
    string property,
    string newValue,
    string entityType)
  {
    var mock = new Mock<IDeviceRepository>();
    mock.Setup(repo => repo.GetDevicesByIds(It.IsAny<IEnumerable<Guid>>()))
      .Returns(new List<Device> {device});

    // setup object under Test
    var changes = new PropertiesChangedSummaryDto
    {
      Comment = "new comment",
      ChangedEvents =
      {
        new PropertyChangedEventDto
        {
          Device = device.Id.ToString(),
          Property = property,
          NewValue = newValue,
          Entity = device.Id.ToString(),
          EntityType = entityType,
        },
      },
    };
    var useCase = new SaveModificationsUseCase(mock.Object, _metadata, _changeLogMock.Object,
        new NullLogger<SaveModificationsUseCase>())
      .Initialize(changes, "Tester");
    return useCase;
  }
}
