namespace WebIO.ConfigFiles;

using System;
using System.IO;
using System.Linq;
using System.Reflection;
using DataAccess;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

public class JsonFileReader : IConfigFileReader
{
  private readonly ILogger<JsonFileReader> _log;

  public JsonFileReader(ILogger<JsonFileReader> log)
  {
    _log = log;
  }

  public T ReadFromJsonFile<T>(string filename)
  {
    var json = File.Exists(filename)
      ? File.ReadAllText(filename)
      : File.Exists(Path.Combine("ConfigFiles", filename))
        ? File.ReadAllText(Path.Combine("ConfigFiles", filename))
        : ReadManifestResourceFile(filename);

    if (!string.IsNullOrEmpty(json))
    {
      return JsonConvert.DeserializeObject<T>(json) ?? throw new ArgumentException("Json file is empty", filename);
    }

    _log.LogError("{Filename} not found", filename);
    throw new ArgumentException(filename, $"{filename} not found");
  }

  public static string? ReadManifestResourceFile(string filename)
  {
    var assembly = Assembly.GetExecutingAssembly();

    var resourceName = assembly
      .GetManifestResourceNames()
      .FirstOrDefault(n => n.EndsWith(filename));
    if (resourceName == null)
    {
      return null;
    }

    var stream = assembly.GetManifestResourceStream(resourceName);
    if (stream == null)
    {
      return null;
    }

    using var reader = new StreamReader(stream);
    return reader.ReadToEnd();
  }
}
