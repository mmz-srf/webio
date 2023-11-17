namespace WebIO.Api.UseCases;

public interface IUseCase
{
    bool Validate();
    void Execute();
}