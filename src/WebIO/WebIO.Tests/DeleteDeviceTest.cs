namespace WebIO.Tests;

using System;
using Api.UseCases;
using DataAccess;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Model;
using Moq;
using Xunit;

public class DeleteDeviceTest
{
    private readonly Mock<IDeviceRepository> _repositoryMock;
    private readonly Mock<IChangeLogRepository> _changeLogMock;

    private ChangeLogEntry? _logEntry;


    public DeleteDeviceTest()
    {
        _repositoryMock = new();

        _changeLogMock = new();
        _changeLogMock.Setup(r => r.Add(It.IsAny<ChangeLogEntry>()))
            .Callback<ChangeLogEntry>(e => _logEntry = e);
    }


    [Fact]
    public void DeviceIsDeleted()
    {
        var device = new Device
        {
            Name = "test",
            Comment = "test",
            Modification = new("test", DateTime.Now, "test", DateTime.Now, "test"),
        };
        _repositoryMock.Setup(x => x.GetDevice(It.IsAny<Guid>())).Returns(device);

        var deletedId = Guid.Empty;
        _repositoryMock.Setup(x => x.Delete(device.Id)).Callback<Guid>(id => deletedId = id);

        var useCase = new DeleteDeviceUseCase(_repositoryMock.Object, _changeLogMock.Object, new NullLogger<DeleteDeviceUseCase>());

        useCase.Initialize(new()
        {
            DeviceId = device.Id,
            Comment = "gelöscht",
        }, "Tester");

        useCase.Validate().Should().BeTrue();
        useCase.Execute();

        deletedId.Should().Be(device.Id);
    }


    [Fact]
    public void AddLogEntry()
    {
        var device = new Device
        {
            Name = "test",
            Comment = "test",
            Modification = new("test", DateTime.Now, "test", DateTime.Now, "test"),
        };
        _repositoryMock.Setup(x => x.GetDevice(It.IsAny<Guid>())).Returns(device);

        var useCase = new DeleteDeviceUseCase(_repositoryMock.Object, _changeLogMock.Object, new NullLogger<DeleteDeviceUseCase>());
        useCase.Initialize(new()
        {
            DeviceId = device.Id,
            Comment = "gelöscht",
        }, "Tester");

        useCase.Validate().Should().BeTrue();
        useCase.Execute();

        _logEntry.Should().NotBeNull();
        //    _logEntry.Comment.Should().Be("comment");
        _logEntry!.FullDetails.Should().BeOfType<DeleteDeviceChangeLogEntry>();
        // .Which.DeviceName.Should().Be("device name test");
    }
}