namespace WebIO.DataAccess;

public interface IConfigFileWriter
{
    void Write(string filename, string content);
}