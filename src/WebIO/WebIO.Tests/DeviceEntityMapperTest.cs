namespace WebIO.Tests;

using System;
using System.Linq;
using DataAccess.EntityFrameworkCore.Mappers;
using FluentAssertions;
using Model;
using Xunit;

public class DeviceEntityMapperTest
{
    private readonly DataField _field1 = new("prop1");
    private readonly DataField _field2 = new("prop2");
    private readonly DataField _field3 = new("prop3");

    private readonly Device _device = new()
    {
        Id = default,
        Name = "hallo",
        Modification = new(new("testCreator", DateTime.Now, "comment")),
    };

    [Fact]
    public void InitialMapping()
    {
        _device.Properties[_field1] = "value1";
            
        var entity = _device.ToEntity();
        entity.Name.Should().Be("hallo");
        entity.Creator.Should().Be("testCreator");

        entity.Properties.Should().ContainSingle();
        var entityProperty = entity.Properties.Single();
        entityProperty.Key.Should().Be("prop1");
        entityProperty.Value.Should().Be("value1");
    }    
        
    [Fact]
    public void ReloadMapping()
    {
        _device.Properties[_field1] = "value1";

        var entity = _device.ToEntity();
        var mappdedDevice = entity.ToModel();

        mappdedDevice.Id.Should().Be(_device.Id);
        mappdedDevice.Name.Should().Be("hallo");
        mappdedDevice.Modification.Creator.Should().Be("testCreator");
        mappdedDevice.Properties[_field1].Should().Be("value1");
    }

    [Fact]
    public void UpdateMapping()
    {
        _device.Properties[_field1] = "value1";
        _device.Properties[_field1] = "value2";

        var entity = _device.ToEntity();
            
        var mappdedDevice = entity.ToModel();
        mappdedDevice.Name = "hallo2";
        mappdedDevice.Modification.Modify(new("testModifier", DateTime.Now, "changed"));

        mappdedDevice.Properties[_field1] = null;
        mappdedDevice.Properties[_field2] = "newValue2";
        mappdedDevice.Properties[_field3] = "newValue3";

        mappdedDevice.SyncEntity(entity);

        entity.Name.Should().Be("hallo2");
        entity.Creator.Should().Be("testCreator");
        entity.Modifier.Should().Be("testModifier");
        entity.Properties.Should().HaveCount(2);
    }
}