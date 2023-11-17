namespace WebIO.Elastic.Management;

public interface IIndexedEntity<T>
{
  public T Id { get; init; }
}
