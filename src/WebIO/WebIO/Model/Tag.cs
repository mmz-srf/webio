namespace WebIO.Model;

using Elastic.Data;

public class Tag
{
    public required string Name { get; set; }

    public StreamType StreamType { get; set; }
}