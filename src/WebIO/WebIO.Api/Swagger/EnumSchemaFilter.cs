namespace WebIO.Api.Swagger;

using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

public class EnumSchemaFilter : ISchemaFilter
{
  public void Apply(OpenApiSchema model, SchemaFilterContext context)
  {
    if (context.Type.IsEnum)
    {
      model.Type = "string";
      model.Enum.Clear();
      model.Format = null;
      Enum.GetNames(context.Type)
        .ToList()
        .ForEach(n => model.Enum.Add(new OpenApiString(n)));
    }
    else if (model.Type == "object")
    {
      foreach (var property in model.Properties)
      {
        if (property.Value.AllOf.Any())
        {
          property.Value.Reference = property.Value.AllOf.First().Reference;
          property.Value.AllOf = new List<OpenApiSchema>();
        }
      }
    }
  }
}