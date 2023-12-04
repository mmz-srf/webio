namespace WebIO.DataAccess.EntityFrameworkCore.Entities;

using System;

/// <summary>
/// Redundant Properties for <see cref="InterfaceEntity"/> to speedup loading of readonly model
/// </summary>
public class InterfaceDenormalizedProperties
{
    public Guid Id { get; set; }

    public int StreamsCountVideoSend { get; set; }

    public int StreamsCountAudioSend { get; set; }

    public int StreamsCountAncillarySend { get; set; }

    public int StreamsCountVideoReceive { get; set; }

    public int StreamsCountAudioReceive { get; set; }

    public int StreamsCountAncillaryReceive { get; set; }
}