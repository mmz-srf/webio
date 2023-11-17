namespace WebIO.Api.UseCases;

public class UseCaseFactory
{
  private readonly IServiceProvider _provider;

  public UseCaseFactory(IServiceProvider provider)
  {
    _provider = provider;
  }

  public T Create<T>() where T : class, IUseCase
  {
    return _provider.GetService(typeof(T)) as T ?? throw new ArgumentException("Use case not found", typeof(T).Name);
  }
}
