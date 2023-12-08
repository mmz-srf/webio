namespace WebIO.Tests.IntegrationTests;

using System;
using Cli;
using DataAccess;
using DataAccess.EntityFrameworkCore;
using Elastic.Data;
using Elastic.Hosting;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Model;
using Xunit;

public class ElasticTest
{
  private readonly Device _testObject = new()
  {
    Name = "testDevice",
    DeviceType = "testDeviceType",
    Comment = "test comment",
    Properties = new(new()),
    Interfaces = new()
    {
      new()
      {
        Name = "testInterface",
        Index = 1,
        InterfaceTemplate = "sender",
        Comment = "test comment",
        Properties = new(new()),
        Streams = new()
        {
          new()
          {
            Name = "audio send stream",
            Comment = "test comment",
            Type = StreamType.Audio,
            Direction = StreamDirection.Send,
            Modification = new("testUser", DateTime.Now, "test modifier", DateTime.Now, "test comment"),
            Properties = new(new()),
          },
        },
        Modification = new("testUser", DateTime.Now, "test modifier", DateTime.Now, "test comment"),
      },
    },
    Modification = new("testUser", DateTime.Now, "test modifier", DateTime.Now, "test comment"),
  };

  private readonly IHost _app;

  public ElasticTest()
  {
    _app = CliApp.CreateAppBuilder(Array.Empty<string>()).Build();
    _app.Services.GetRequiredService<IElasticStartup>().InitializeAllIndexes(default).GetAwaiter().GetResult();
    _app.Services.GetRequiredService<AppDbContext>().Database.EnsureCreated();
  }

  [Fact(Skip = "Broken atm")]
  public void WriteAndLoadEntity()
  {
    var repo = _app.Services.GetRequiredService<IDeviceRepository>();
    repo.Upsert(_testObject);

    var entity = repo.GetDevice(_testObject.Id);

    entity.Should().BeEquivalentTo(_testObject);
  }
}
