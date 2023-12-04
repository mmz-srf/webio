namespace WebIO.Model;

using System;

public class Modification
{
  /// <summary>
  /// for initial creation of 
  /// </summary>
  public Modification(ModifyArgs args)
  {
    Creator = args.Username;
    Modifier = args.Username;
    Created = args.Timestamp;
    Modified = args.Timestamp;
    Comment = args.Comment;
  }

  public Modification(
    string creator,
    DateTime created,
    string modifier,
    DateTime modified,
    string comment)
  {
    Creator = creator;
    Created = created;
    Modifier = modifier;
    Modified = modified;
    Comment = comment;
  }

  public string Creator { get; }
  public DateTime Created { get; }
  public string Modifier { get; private set; }
  public DateTime Modified { get; private set; }
  public string Comment { get; private set; }

  public void Modify(ModifyArgs args)
  {
    Modifier = args.Username;
    Modified = args.Timestamp;
    Comment = args.Comment;
  }

  public static Modification Empty
    => new(string.Empty, DateTime.MinValue, string.Empty, DateTime.MinValue, string.Empty);
}
