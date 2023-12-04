namespace WebIO.Api.UseCases;

using Application;
using Controllers.Dto;
using DataAccess;
using Microsoft.Extensions.Logging;
using Model;

public class DeleteDeviceUseCase : IUseCase
{
    private readonly IDeviceRepository _deviceRepository;
    private readonly IChangeLogRepository _changeLog;
    private readonly ILogger<DeleteDeviceUseCase> _log;
    private DeviceDeletedDto? _deleted;
    private string? _username;

    public DeleteDeviceUseCase(
        IDeviceRepository deviceRepository,
        IChangeLogRepository changeLog,
        ILogger<DeleteDeviceUseCase> log)
    {
        _deviceRepository = deviceRepository;
        _changeLog = changeLog;
        _log = log;
    }

    public void Initialize(DeviceDeletedDto deleted, string username)
    {
        using var span = Telemetry.Span();
        _username = username;
        _deleted = deleted;
    }


    public bool Validate()
    {
        using var span = Telemetry.Span();
        return !string.IsNullOrWhiteSpace(_username);
    }


    public void Execute()
    {
        using var span = Telemetry.Span();
        _log.LogInformation("Delete Device with Id: {DeletedId}", _deleted!.DeviceId);
        _deviceRepository.Delete(_deleted.DeviceId);

        var logEntry = new ChangeLogEntry(
            DateTime.Now,
            username: _username!,
            comment: _deleted.Comment,
            $"Deleted device { _deleted.DeviceId }",
            new DeleteDeviceChangeLogEntry());


        _changeLog.Add(logEntry);

    }
}