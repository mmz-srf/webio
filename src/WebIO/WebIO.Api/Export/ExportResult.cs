namespace WebIO.Api.Export;

public class ExportResult
{
  private ExportResult(
    bool failed,
    string? errorMessage,
    Stream? data,
    ExportFileType fileType,
    string filename)
  {
    Failed = failed;
    ErrorMessage = errorMessage ?? string.Empty;
    Data = data;
    FileType = fileType;
    Filename = filename;
  }

  public static ExportResult Create(
    Stream data,
    ExportFileType fileType,
    string filename)
  {
    return new(
      false,
      null,
      data,
      fileType,
      filename);
  }

  public static ExportResult Error(string errorMessage)
  {
    return new(true,
      errorMessage,
      null,
      default,
      string.Empty);
  }

  public bool Failed { get; }

  public string ErrorMessage { get; }

  public Stream? Data { get; }

  public ExportFileType? FileType { get; }

  public string Filename { get; }
}
