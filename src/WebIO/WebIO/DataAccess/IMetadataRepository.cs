namespace WebIO.DataAccess;

using System.Collections.Generic;
using Model;
using Model.DeviceTemplates;
using Model.Display;

public interface IMetadataRepository
{
    IEnumerable<DataField> DataFields { get; }

    IEnumerable<DeviceType> DeviceTypes { get; }

    DisplayConfiguration? DisplayConfiguration { get; }

    IEnumerable<Tag> Tags { get; }

    IEnumerable<StreamDefinition> NevionStreamDefinitions { get; }
}