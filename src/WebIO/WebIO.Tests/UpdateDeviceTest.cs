namespace WebIO.Tests;

using System;
using System.Linq;
using Api.Controllers.Dto;
using Api.UseCases;
using DataAccess;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using MockMetadata;
using Model;
using Model.DeviceTemplates;
using Moq;
using Xunit;

public class UpdateDeviceTest
{
  private readonly MockMetadataRepository _metadataMock;
  private readonly DeviceCreation _creation;
  private Device _device;

  private readonly UpdateDeviceUseCase _useCase;

  public UpdateDeviceTest()
  {
    Mock<IDeviceRepository> repositoryMock = new();

    _device = new()
    {
      Name = "test",
      Comment = "test",
      Modification = new("test", DateTime.Now, "test", DateTime.Now, "test"),
    };
    repositoryMock.Setup(x => x.GetDevice(It.IsAny<Guid>())).Returns(() => _device);

    Mock<IChangeLogRepository> changeLogMock = new();

    _metadataMock = new();

    _creation = new(_metadataMock, new NullLogger<UpdateDeviceTest>());

    _useCase = new(repositoryMock.Object, changeLogMock.Object, _metadataMock, new NullLogger<UpdateDeviceUseCase>());
  }

  private void CreateDevice(DeviceType deviceType)
  {
    var modification = new ModifyArgs("bob", DateTime.Now, "comment");
    var interfaces = deviceType.DefaultInterfaces
      .SelectMany(i => Enumerable.Repeat(i.Template, i.Count))
      .Select(it => deviceType.InterfaceTemplates.FirstOrDefault(t => t.Name == it))
      .Select(t => new InterfaceTemplateSelection(t!))
      .ToList();

    _device = _creation.CreateDevice(deviceType, modification, "testDevice", "comment", interfaces);
  }

  [Fact]
  public void UpdateWithNothingToDo()
  {
    var deviceType = _metadataMock.MockDeviceTypes.BasicType;

    CreateDevice(deviceType);

    var interfaces = _device.Interfaces.Select(i => i.InterfaceTemplate)
      .Select(t => new InterfaceTemplateSelectionDto {TemplateName = t ?? string.Empty})
      .ToList();

    var updateArgs = new DeviceUpdatedEventDto
    {
      DeviceId = _device.Id,
      Comment = "updated",
      Interfaces = interfaces,
    };
    _useCase.Initialize(updateArgs, "tester");

    _useCase.Validate().Should().BeTrue();
    _useCase.Execute();

    _device.Modification.Modifier.Should().NotBe("tester");
    _device.Interfaces.Should().HaveCount(2);
  }

  [Fact]
  public void ChangeInterfaceTemplate()
  {
    var deviceType = _metadataMock.MockDeviceTypes.BasicType;

    CreateDevice(deviceType);

    _useCase.Initialize(new()
    {
      DeviceId = _device.Id,
      Comment = "updated",
      Interfaces =
      {
        new InterfaceTemplateSelectionDto {TemplateName = "audioReceive"},
        new InterfaceTemplateSelectionDto {TemplateName = "audioReceive"},
      },
    }, "tester");

    _useCase.Execute();

    _device.Modification.Modifier.Should().Be("tester");
    _device.Interfaces.Should().HaveCount(2);

    _device.Interfaces[0].InterfaceTemplate.Should().Be("audioReceive");
  }

  [Fact]
  public void UseDataFieldsOfChangedTemplate()
  {
    var deviceType = _metadataMock.MockDeviceTypes.BasicType;

    CreateDevice(deviceType);
    _device.Interfaces[0].Properties[_metadataMock.MockDataFields.TestField] = "value1";
    _device.Interfaces[0].Properties[_metadataMock.MockDataFields.FunctionalGroup].Should().Be("sendGroup");

    _useCase.Initialize(new()
    {
      DeviceId = _device.Id,
      Comment = "updated",
      Interfaces =
      {
        new InterfaceTemplateSelectionDto {TemplateName = "audioReceive"},
        new InterfaceTemplateSelectionDto {TemplateName = "audioReceive"},
      },
    }, "tester");

    _useCase.Execute();

    _device.Modification.Modifier.Should().Be("tester");
    _device.Interfaces.Should().HaveCount(2);
    _device.Interfaces[0].InterfaceTemplate.Should().Be("audioReceive");

    // keep non-templated value
    _device.Interfaces[0].Properties[_metadataMock.MockDataFields.TestField].Should().Be("value1");

    // update templated value
    _device.Interfaces[0].Properties[_metadataMock.MockDataFields.FunctionalGroup].Should().Be("recGroup");
  }

  [Fact]
  public void DoNotAllowInterfacesWhenNotSwDefined()
  {
    var deviceType = _metadataMock.MockDeviceTypes.BasicType;

    CreateDevice(deviceType);

    _useCase.Initialize(new()
    {
      DeviceId = _device.Id,
      Comment = "updated",
      Interfaces =
      {
        new InterfaceTemplateSelectionDto {TemplateName = "audioSend"},
        new InterfaceTemplateSelectionDto {TemplateName = "audioSend"},
        new InterfaceTemplateSelectionDto {TemplateName = "audioReceive"},
      },
    }, "tester");

    _useCase.Execute();
    _device.Interfaces.Should().HaveCount(2);
  }

  [Fact]
  public void AddInterfaces()
  {
    var deviceType = _metadataMock.MockDeviceTypes.SwDefinedInterfaces;

    CreateDevice(deviceType);

    _useCase.Initialize(new()
    {
      DeviceId = _device.Id,
      Comment = "updated",
      Interfaces =
      {
        new InterfaceTemplateSelectionDto {TemplateName = "audioSend"},
        new InterfaceTemplateSelectionDto {TemplateName = "audioSend"},
        new InterfaceTemplateSelectionDto {TemplateName = "audioReceive"},
      },
    }, "tester");

    _useCase.Execute();

    _device.Interfaces.Should().HaveCount(3);
    _device.Interfaces[0].InterfaceTemplate.Should().Be("audioSend");
    _device.Interfaces[0].Modification.Modifier.Should().Be("bob");
    _device.Interfaces[2].InterfaceTemplate.Should().Be("audioReceive");
    _device.Interfaces[2].Modification.Modifier.Should().Be("tester");
    _device.Modification.Modifier.Should().Be("tester");
  }

  [Fact]
  public void RemoveInterfaces()
  {
    var deviceType = _metadataMock.MockDeviceTypes.SwDefinedInterfaces;

    CreateDevice(deviceType);

    _useCase.Initialize(new()
    {
      DeviceId = _device.Id,
      Comment = "updated",
      Interfaces =
      {
        new InterfaceTemplateSelectionDto {TemplateName = "audioSend"},
      },
    }, "tester");

    _useCase.Execute();

    _device.Interfaces.Should().HaveCount(1);
    _device.Interfaces[0].InterfaceTemplate.Should().Be("audioSend");
    _device.Interfaces[0].Modification.Modifier.Should().Be("bob");
    _device.Modification.Modifier.Should().Be("tester");
  }
}
