namespace WebIO.Tests.MockMetadata;

using System.Collections.Generic;
using Model;

public class MockDataFields
{
    public DataField Tags { get; } = new("Tags")
    {
        FieldType = DataFieldType.Text,
        EditLevels = { FieldLevel.Stream },
    };

    public DataField FunctionalGroup { get; } = new("FunctionalGroup")
    {
        FieldType = DataFieldType.Text,
        EditLevels = { FieldLevel.Interface },
    };

    public DataField Driver { get; } = new("Driver")
    {
        EditLevels = { FieldLevel.Device },
        FieldType = DataFieldType.Selection,
        SelectableValues = { "Test1", "Test2" },
    };

    public DataField TestField { get; } = new("TestField")
    {
        EditLevels = { FieldLevel.Device },
        FieldType = DataFieldType.Text,
    };

    public IEnumerable<DataField> AllFields { get; }

    public MockDataFields()
    {
        AllFields = new[] { Tags, FunctionalGroup, Driver, TestField };
    }
}