namespace WebIO.DataAccess;

public interface IConfigFileReader
{
    T ReadFromJsonFile<T>(string filename);
}