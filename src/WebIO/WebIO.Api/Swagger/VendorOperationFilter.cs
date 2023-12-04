namespace WebIO.Api.Swagger;

using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

public class VendorOperationFilter : IOperationFilter
{
  public void Apply(OpenApiOperation operation, OperationFilterContext context)
  {
    operation.Extensions.Add("x-operation-name", new OpenApiString(GetMethodName(context)));
  }

  private static string GetMethodName(OperationFilterContext context)
  {
    var name = context.MethodInfo.Name;
    return char.ToLowerInvariant(name[0]) + name[1..];
  }
}