namespace WebIO.Tests.Nevion;

using System.Collections.Generic;
using System.Linq;
using Application;
using ConfigFiles;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Model.DeviceTemplates;
using Xunit;

public class ExcelMappingReaderTest
{
  private const string MappingFile = "./TestConfigFiles/streammapping.xlsx";
  private const string DeviceTypesFile = "./TestConfigFiles/devicetypes.json";

  private readonly ExcelMappingReader _reader =
    new(new JsonFileReader(new NullLogger<JsonFileReader>()).ReadFromJsonFile<List<DeviceType>>(DeviceTypesFile));

  [Fact]
  public void FindsDevices()
  {
    _reader.ReadFromFile(MappingFile).Should().NotBeEmpty();
  }

  [Fact]
  public void SetsNameCorrectly()
  {
    var result = _reader.ReadFromFile(MappingFile);
    result.First().DeviceTypeName.Should().Be("snpgateway");
  }

  [Fact]
  public void PopulatesStreamMappings()
  {
    var result = _reader.ReadFromFile(MappingFile);
    result.First().Mappings.Should().HaveCountGreaterThan(5);
  }
}
