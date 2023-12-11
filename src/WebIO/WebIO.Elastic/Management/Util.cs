namespace WebIO.Elastic.Management;

using System.Text.RegularExpressions;
using Nest;
using Search;

public static partial class Util
{
  public static readonly string[] ReservedCharacters =
  {
    "\\",
    "+",
    "-",
    "&&",
    "||",
    "!",
    "(",
    ")",
    "{",
    "}",
    "[",
    "]",
    "^",
    "\"",
    "~",
    "*",
    "?",
    ":",
    "/",
  };

  public static string ToIndexName(this Type clazz)
    => clazz.Name
      .Replace("Indexed", "")
      .Replace("Idx", "")
      .Replace("Index", "")
      .PascalToKebabCase();

  private static string PascalToKebabCase(this string value)
    => string.IsNullOrEmpty(value)
      ? value
      : Regex.Replace(
          value,
          "(?<!^)([A-Z][a-z]|(?<=[a-z])[A-Z0-9])",
          "-$1",
          RegexOptions.Compiled)
        .Trim()
        .ToLower();

  public static string PrefixWith(this string value, string? prefix)
    => string.IsNullOrWhiteSpace(prefix)
      ? value
      : $"{prefix}-{value}";

  public static QueryContainer ToTextQuery<TIndexedEntity, TId>(
    QueryContainerDescriptor<TIndexedEntity> qcd,
    string text,
    IReadOnlyCollection<TextFieldSelector<TIndexedEntity>> fields)
    where TIndexedEntity : class, IIndexedEntity<TId>
  {
    var spaceFreeTexts = TextSplitter().Split(text).ToHashSet();
    spaceFreeTexts.Add(text);

    var queryContainers = new List<QueryContainer?>();
    foreach (var queryText in spaceFreeTexts.Select(spaceFreeText => Escape(spaceFreeText)))
    {
      queryContainers.Add(AddExactMatchFilter<TIndexedEntity, TId>(qcd, fields, queryText));
      queryContainers.Add(AddPrefixFilter<TIndexedEntity, TId>(qcd, fields, queryText));
      queryContainers.Add(AddInfixFilter<TIndexedEntity, TId>(qcd, fields, queryText));
      queryContainers.Add(AddSuffixFilter<TIndexedEntity, TId>(qcd, fields, queryText));
      queryContainers.Add(AddFuzzyFilter<TIndexedEntity, TId>(qcd, fields, queryText));
    }

    return CombineOr(queryContainers.Where(c => c != null)!);
  }

  private static QueryContainer? AddExactMatchFilter<TIndexedEntity, TId>(
    QueryContainerDescriptor<TIndexedEntity> qc,
    IEnumerable<TextFieldSelector<TIndexedEntity>> fields,
    string queryText) where TIndexedEntity : class, IIndexedEntity<TId>
  {
    var exactFields = FieldsOfType<TIndexedEntity, TId>(fields, TextFieldType.Exact);
    return exactFields.Any()
      ? qc.QueryString(
        s => s.Query(queryText)
          .DefaultOperator(Operator.Or)
          .Fields(GetFieldDescriptor<TIndexedEntity, TId>(exactFields)))
      : null;
  }

  private static QueryContainer? AddPrefixFilter<TIndexedEntity, TId>(
    QueryContainerDescriptor<TIndexedEntity> qc,
    IEnumerable<TextFieldSelector<TIndexedEntity>> fields,
    string queryText) where TIndexedEntity : class, IIndexedEntity<TId>
  {
    var prefixFields = FieldsOfType<TIndexedEntity, TId>(fields, TextFieldType.PrefixWildcard);
    return prefixFields.Any()
      ? qc.QueryString(
        s => s.Query($"{queryText}*")
          .AnalyzeWildcard()
          .DefaultOperator(Operator.Or)
          .Fields(GetFieldDescriptor<TIndexedEntity, TId>(prefixFields)))
      : null;
  }

  private static QueryContainer? AddInfixFilter<TIndexedEntity, TId>(
    QueryContainerDescriptor<TIndexedEntity> qc,
    IEnumerable<TextFieldSelector<TIndexedEntity>> fields,
    string queryText) where TIndexedEntity : class, IIndexedEntity<TId>
  {
    var infixFields = FieldsOfType<TIndexedEntity, TId>(fields, TextFieldType.InfixWildcard);
    return infixFields.Any()
      ? qc.QueryString(
        s => s.Query($"*{queryText}*")
          .AnalyzeWildcard()
          .DefaultOperator(Operator.Or)
          .Fields(GetFieldDescriptor<TIndexedEntity, TId>(infixFields)))
      : null;
  }

  private static QueryContainer? AddSuffixFilter<TIndexedEntity, TId>(
    QueryContainerDescriptor<TIndexedEntity> qc,
    IEnumerable<TextFieldSelector<TIndexedEntity>> fields,
    string queryText) where TIndexedEntity : class, IIndexedEntity<TId>
  {
    var suffixFields = FieldsOfType<TIndexedEntity, TId>(fields, TextFieldType.SuffixWildcard);
    return suffixFields.Any()
      ? qc.QueryString(
        s => s.Query($"*{queryText}")
          .AnalyzeWildcard()
          .DefaultOperator(Operator.Or)
          .Fields(GetFieldDescriptor<TIndexedEntity, TId>(suffixFields)))
      : null;
  }

  private static QueryContainer? AddFuzzyFilter<TIndexedEntity, TId>(
    QueryContainerDescriptor<TIndexedEntity> qc,
    IEnumerable<TextFieldSelector<TIndexedEntity>> fields,
    string queryText) where TIndexedEntity : class, IIndexedEntity<TId>
  {
    var fuzzyFields = FieldsOfType<TIndexedEntity, TId>(fields, TextFieldType.Fuzzy);
    return fuzzyFields.Any()
      ? qc.QueryString(
        s => s.Query($"{queryText}~*")
          .FuzzyMaxExpansions(100) // todo: to textfieldDescriptor
          .FuzzyPrefixLength(2)
          .Fuzziness(Fuzziness.Auto)
          .DefaultOperator(Operator.And)
          .Fields(GetFieldDescriptor<TIndexedEntity, TId>(fuzzyFields)))
      : null;
  }

  private static string Escape(string searchText, Dictionary<string, string>? overrides = null)
  {
    var reservedCharacters = ReservedCharacters;
    if (overrides != null)
    {
      reservedCharacters = reservedCharacters.ToList()
        .Where(rc => !overrides.ContainsKey(rc))
        .ToArray();

      searchText = overrides.Keys.Aggregate(searchText,
        (current, overridesKey) => current.Replace(overridesKey, overrides[overridesKey]));
    }

    return string.Join(
      " ",
      searchText.Split(reservedCharacters, StringSplitOptions.RemoveEmptyEntries));
  }

  private static Func<FieldsDescriptor<TIndexedEntity>, FieldsDescriptor<TIndexedEntity>>
    GetFieldDescriptor<TIndexedEntity, TId>(
      IEnumerable<TextFieldSelector<TIndexedEntity>> selectors)
    where TIndexedEntity : class, IIndexedEntity<TId>
    => f =>
    {
      foreach (var field in selectors)
      {
        if(field.Selector != null)
          f.Field(field.Selector, field.Boost);
        else
          f.Field(field.Name, field.Boost);
      }

      return f;
    };

  private static QueryContainer CombineOr(IEnumerable<QueryContainer> containers)
    => containers.Aggregate((current, currentContainer) => current || currentContainer)
       ?? throw new InvalidOperationException("No query container found, need at least one.");

  private static List<TextFieldSelector<TIndexedEntity>> FieldsOfType<TIndexedEntity, TId>(
    IEnumerable<TextFieldSelector<TIndexedEntity>> fields,
    TextFieldType textFieldType) where TIndexedEntity : class, IIndexedEntity<TId>
    => fields.Where(f => f.Type == textFieldType).ToList();
    [GeneratedRegex("\\s+")]
    private static partial Regex TextSplitter();
}
