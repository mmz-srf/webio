namespace WebIO.Api.Export;

public class ExportColumn<T>
{
    public ExportColumn(string headerName, Func<T, string> data)
    {
        HeaderName = headerName;
        GetData = data;
    }

    public string HeaderName { get; }
    public Func<T, string> GetData { get; }
}