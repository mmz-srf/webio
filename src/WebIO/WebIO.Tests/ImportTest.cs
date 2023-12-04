namespace WebIO.Tests;

using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Cli.ExcelImport;
using ConfigFiles;
using DataAccess;
using DataAccess.Configuration;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Model;
using Moq;
using Xunit;

public class ImportTest
{
    private ExcelImport _import;

    public ImportTest()
    {
        List<Device> savedDevices = new();
        var metadataRepo = new JsonMetadataRepository(new NullLogger<JsonMetadataRepository>(), new JsonFileReader(new NullLogger<JsonFileReader>()));
        var deviceRepoMock = new Mock<IDeviceRepository>();
        deviceRepoMock.Setup(r => r.Upsert(It.IsAny<Device>())).Callback<Device>(d => savedDevices.Add(d));
        _import = new(deviceRepoMock.Object, metadataRepo, new NullLogger<ExcelImport>());

    }

    [Fact]
    public void ReadFilesWithWildcardPath()
    {
        var files = Directory.GetFiles(@".\ImportTestData\files", @"VGW*.*");
        files.Should()
            .HaveCount(11);
        
        var files2 = Directory.GetFiles(@".\ImportTestData", @"files\VGW*2.*");
        files2.Should()
            .HaveCount(3);
    }

    // [Fact]
    // public void ImportDoesNotFail()
    // {
    //     var filename = GetImportManifestFilename("import.json");
    //     filename.Should().NotBeNullOrWhiteSpace();
    //     File.Exists(filename).Should().BeTrue();
    //
    //     _import.Import(filename);
    //     _savedDevices.Should().HaveCount(i => i > 0);
    // }
    //
    // [Fact]
    // public void ImportSnp()
    // {
    //     var filename = GetImportManifestFilename("importSnpOnly.json");
    //     filename.Should().NotBeNullOrWhiteSpace();
    //     File.Exists(filename).Should().BeTrue();
    //
    //     _import.Import(filename);
    //     _savedDevices.Should().HaveCount(5);
    //
    //     var vgw701 = _savedDevices.FirstOrDefault(d => d.Name.StartsWith("VGW-701"));
    //     vgw701.Should().NotBeNull();
    //
    //     // check properties
    //     vgw701.Properties["FqdnRealtimeA"].Should().Be("tpco-megw-vgw701.rta.st-net.media.int");
    //
    //     // check interface templates
    //     vgw701.Interfaces.Should().HaveCount(32);
    //     vgw701.Interfaces.Should().NotContain(i => i.Index == 0);
    //
    //     var sdi8 = vgw701.Interfaces.SingleOrDefault(i => i.Index == 8);
    //     sdi8.Should().NotBeNull();
    //     sdi8.InterfaceTemplate.Should().Be("receive");
    //
    //     var sdi9 = vgw701.Interfaces.SingleOrDefault(i => i.Index == 9);
    //     sdi9.Should().NotBeNull();
    //     sdi9.InterfaceTemplate.Should().Be("send");
    //
    //     sdi9.Properties["WorkplaceUsage"].Should().Be("Regisseur");
    //     var sdi9Streams = sdi9.GetStreamCardinality();
    //     sdi9Streams.VideoSend.Should().Be(1);
    //     sdi9Streams.VideoReceive.Should().Be(0);
    //     sdi9Streams.AudioSend.Should().Be(16);
    //     sdi9Streams.AudioReceive.Should().Be(0);
    //     sdi9Streams.AncillarySend.Should().Be(4);
    //     sdi9Streams.AncillaryReceive.Should().Be(0);
    //
    //     var sdi9VidSend = sdi9.GetStreams(StreamType.Video, StreamDirection.Send).Single();
    //     sdi9VidSend.Properties["Label1"].Should().Be("PM01-01 PGM");
    // }

    private string GetImportManifestFilename(string filename)
    {
        var l = Assembly.GetExecutingAssembly().Location;
        var dir = new DirectoryInfo(l);

        // find test project directory
        while (dir != null 
               && dir.Name != "WebIO.Tests")
        {
            dir = dir.Parent;
        }

        dir.Should().NotBeNull();

        return Path.Combine(dir!.ToString(), "ImportTestData", filename);
    }
}