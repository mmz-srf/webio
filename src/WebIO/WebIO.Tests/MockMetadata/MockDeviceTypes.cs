namespace WebIO.Tests.MockMetadata;

using System.Collections.Generic;
using Elastic.Data;
using Model.DeviceTemplates;

public class MockDeviceTypes
{
    public DeviceType BasicType { get; } = new()
    {
        Name = "testType",
        DisplayName = "test type",
        InterfaceCount = 2,
        InterfaceNameTemplate = "_test{0:00}",
        InterfaceStreamsFlexible = false,
        SoftwareDefinedInterfaceCount = false,
        FieldValues =
        {
            new TemplateFieldValue
            {
                Value = "Test1",
                FieldKey = "Driver",
            },
        },
        DefaultInterfaces =
        {
            new InterfaceDefinition
            {
                Template = "audioSend",
                Count = 2,
            },
        },
        InterfaceTemplates =
        {
            new InterfaceTemplate
            {
                Name = "audioSend",
                DisplayName = "1 audio send",
                FieldValues =
                {
                    new TemplateFieldValue
                    {
                        Value = "sendGroup",
                        FieldKey = "FunctionalGroup",
                    },
                },
                Streams =
                {
                    new StreamTemplate
                    {
                        Count = 1,
                        Type = StreamType.Audio,
                        Direction = StreamDirection.Send,
                        NameTemplate = "_AUDsend_{0:0000}",
                        FieldValues =
                        {
                            new TemplateFieldValue
                            {
                                Value = "Tag1",
                                FieldKey = "Tags",
                            },
                        },
                    },
                },
            },
            new InterfaceTemplate
            {
                Name = "audioReceive",
                DisplayName = "1 audio rec",
                FieldValues =
                {
                    new TemplateFieldValue
                    {
                        Value = "recGroup",
                        FieldKey = "FunctionalGroup",
                    },
                },
                Streams =
                {
                    new StreamTemplate
                    {
                        Count = 1,
                        Type = StreamType.Audio,
                        Direction = StreamDirection.Receive,
                        NameTemplate = "_AUDrec_{0:0000}",
                        FieldValues =
                        {
                            new TemplateFieldValue
                            {
                                Value = "Tag2",
                                FieldKey = "Tags",
                            },
                        },
                    },
                },
            },
        },
    };

    public DeviceType SwDefinedInterfaces { get; } = new()
    {
        Name = "testTypeSwDefined",
        DisplayName = "test type with sw defined interfaces",
        InterfaceCount = 2,
        InterfaceNameTemplate = "_test{0:00}",
        InterfaceStreamsFlexible = false,
        SoftwareDefinedInterfaceCount = true,
        DefaultInterfaces =
        {
            new InterfaceDefinition
            {
                Template = "audioSend",
                Count = 2,
            },
        },

        InterfaceTemplates =
        {
            new InterfaceTemplate
            {
                Name = "audioSend",
                DisplayName = "1 audio send",
                FieldValues =
                {
                    new TemplateFieldValue
                    {
                        Value = "sendGroup",
                        FieldKey = "FunctionalGroup",
                    },
                },
                Streams =
                {
                    new StreamTemplate
                    {
                        Count = 1,
                        Type = StreamType.Audio,
                        Direction = StreamDirection.Send,
                        NameTemplate = "_AUDsend_{0:0000}",
                        FieldValues =
                        {
                            new TemplateFieldValue
                            {
                                Value = "Tag1",
                                FieldKey = "Tags",
                            },
                        },
                    },
                },
            },
            new InterfaceTemplate
            {
                Name = "audioReceive",
                DisplayName = "1 audio rec",
                FieldValues =
                {
                    new TemplateFieldValue
                    {
                        Value = "recGroup",
                        FieldKey = "FunctionalGroup",
                    },
                },
                Streams =
                {
                    new StreamTemplate
                    {
                        Count = 1,
                        Type = StreamType.Audio,
                        Direction = StreamDirection.Receive,
                        NameTemplate = "_AUDrec_{0:0000}",
                        FieldValues =
                        {
                            new TemplateFieldValue
                            {
                                Value = "Tag2",
                                FieldKey = "Tags",
                            },
                        },
                    },
                },
            },
        },
    };

    public DeviceType FlexibleStreams { get; } = new()
    {
        Name = "testTypeFlexible",
        DisplayName = "test type",
        InterfaceCount = 2,
        InterfaceNameTemplate = "_test{0:00}",
        InterfaceStreamsFlexible = true,
        SoftwareDefinedInterfaceCount = true,
        FieldValues =
        {
            new TemplateFieldValue
            {
                Value = "Test1",
                FieldKey = "Driver",
            },
        },
        InterfaceTemplates =
        {
            new InterfaceTemplate
            {
                Name = "default",
                DisplayName = "default",
                FieldValues =
                {
                    new TemplateFieldValue
                    {
                        Value = "sendGroup",
                        FieldKey = "FunctionalGroup",
                    },
                },
                Streams =
                {
                    new StreamTemplate
                    {
                        Type = StreamType.Video,
                        Direction = StreamDirection.Send,
                        NameTemplate = "_VIDsend_{0:0000}",
                        FieldValues =
                        {
                            new TemplateFieldValue
                            {
                                Value = "Tag1",
                                FieldKey = "Tags",
                            },
                        },
                    },               
                    new StreamTemplate
                    {
                        Type = StreamType.Audio,
                        Direction = StreamDirection.Send,
                        NameTemplate = "_AUDsend_{0:0000}",
                        FieldValues =
                        {
                            new TemplateFieldValue
                            {
                                Value = "Tag2",
                                FieldKey = "Tags",
                            },
                        },
                    },                      
                    new StreamTemplate
                    {
                        Type = StreamType.Ancillary,
                        Direction = StreamDirection.Send,
                        NameTemplate = "_ANCsend_{0:0000}",
                        FieldValues =
                        {
                            new TemplateFieldValue
                            {
                                Value = "Tag3",
                                FieldKey = "Tags",
                            },
                        },
                    },        
                    new StreamTemplate
                    {
                        Type = StreamType.Video,
                        Direction = StreamDirection.Receive,
                        NameTemplate = "_VIDrec_{0:0000}",
                        FieldValues =
                        {
                            new TemplateFieldValue
                            {
                                Value = "Tag4",
                                FieldKey = "Tags",
                            },
                        },
                    },               
                    new StreamTemplate
                    {
                        Type = StreamType.Audio,
                        Direction = StreamDirection.Receive,
                        NameTemplate = "_AUDrec_{0:0000}",
                        FieldValues =
                        {
                            new TemplateFieldValue
                            {
                                Value = "Tag5",
                                FieldKey = "Tags",
                            },
                        },
                    },                      
                    new StreamTemplate
                    {
                        Type = StreamType.Ancillary,
                        Direction = StreamDirection.Receive,
                        NameTemplate = "_ANCrec_{0:0000}",
                        FieldValues =
                        {
                            new TemplateFieldValue
                            {
                                Value = "Tag6",
                                FieldKey = "Tags",
                            },
                        },
                    },
                },
            },
        },
    };

    public IEnumerable<DeviceType> AllTypes { get; }

    public MockDeviceTypes()
    {
        AllTypes = new[] { BasicType, SwDefinedInterfaces, FlexibleStreams };
    }
}