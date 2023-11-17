namespace WebIO.Application;

using System.Diagnostics;
using System.Runtime.CompilerServices;

public static class Telemetry
{ 
  private static readonly ActivitySource ActivitySource = new(App.AppName);

  public static Activity Span([CallerMemberName] string name = "", ActivityKind kind = ActivityKind.Internal)
    => ActivitySource.StartActivity(name, kind)!;
}