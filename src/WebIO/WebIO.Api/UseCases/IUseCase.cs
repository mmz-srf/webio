namespace WebIO.Api.UseCases;

public interface IUseCase
{
    Task<bool> ValidateAsync(CancellationToken ct);
    Task ExecuteAsync(CancellationToken ct);
}