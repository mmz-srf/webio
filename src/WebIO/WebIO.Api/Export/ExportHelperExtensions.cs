namespace WebIO.Api.Export;

public static class ExportHelperExtensions
{
    public static string StringJoin(this IEnumerable<string> strings, string separator = ", ")
        => string.Join(separator, strings ?? Enumerable.Empty<string>());
}