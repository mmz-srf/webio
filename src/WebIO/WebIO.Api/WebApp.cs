namespace WebIO.Api;

using Controllers.Auth;
using Export;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Web;
using Microsoft.IdentityModel.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Swagger;
using UseCases;
using static App;

public static class WebApp
{
  public static void ConfigureWebApp(WebApplication app, WebApplicationBuilder builder)
  {
    if (builder.Environment.IsDevelopment())
    {
      IdentityModelEventSource.ShowPII = true;
      app.UseDeveloperExceptionPage();
    }
    else
    {
      app.UseHsts();
      app.UseHttpsRedirection();
    }

    app.Use(async (context, next) =>
    {
      await next();

      // If there's no available file and the request doesn't contain an extension, we're probably trying to access a page.
      // Rewrite request to use app root.
      if (context.Response.StatusCode == 404 &&
          !Path.HasExtension(context.Request.Path.Value) &&
          !context.Request.Path.Value!.StartsWith("/api"))
      {
        context.Request.Path = "/index.html";
        context.Response.StatusCode = 200; // Make sure we update the status code, otherwise it returns 404
        await next();
      }
    });

    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "WebIO API V1"));

    app.UseRouting();

    app.UseAuthentication();
    app.UseAuthorization();
    app.UseDefaultFiles();
    app.UseStaticFiles();
    app.MapControllers();
  }

  public static WebApplicationBuilder CreateWebApplicationBuilder(string[] args)
  {
    var builder = WebApplication.CreateBuilder(new WebApplicationOptions
    {
      WebRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"),
      Args = args,
    });
    RegisterConfiguration(builder.Configuration, builder.Environment);
    builder.Logging.AddConfiguration(builder.Configuration);

    builder.Services
      .ConfigureServices(builder.Configuration)
      .ConfigureAsp(builder.Configuration)
      .Setup();
    
    return builder;
  }

  // ReSharper disable once UnusedMethodReturnValue.Local
  private static IServiceCollection Setup(this IServiceCollection services)
    => services
      .SetupExports()
      .SetupUseCases();

  private static IServiceCollection SetupUseCases(this IServiceCollection services)
    => services
      .AddScoped(provider => new UseCaseFactory(provider))
      .AddAllImplementationsOf<IUseCase>();

  private static IServiceCollection SetupExports(this IServiceCollection services)
    => services
      .AddTransient<IExportFactory, ExportFactory>()
      .AddAllImplementationsOf<IExport>();

  private static IServiceCollection ConfigureAsp(this IServiceCollection services, IConfiguration config)
  {
    services
      .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
      .AddMicrosoftIdentityWebApi(config);

    services.AddMvcCore()
      .AddAuthorization(
        opt => opt.AddPolicy(Claims.CanEdit, policy => policy.AddRequirements(new IsAdminRequirement())))
      .AddNewtonsoftJson(options =>
      {
        options.SerializerSettings.Converters.Add(new StringEnumConverter());
        options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
      })
      .AddApiExplorer();

    services.AddScoped<IAuthorizationHandler, AdminAuthorizationHandler>();

    services.AddSwaggerGen(c =>
    {
      c.SchemaFilter<EnumSchemaFilter>();
      c.OperationFilter<VendorOperationFilter>();
      c.CustomOperationIds(e =>
      {
        var descriptor = (ControllerActionDescriptor) e.ActionDescriptor;
        return $"{descriptor.ControllerName}.{descriptor.ActionName}";
      });
      c.SwaggerDoc("v1", new() {Title = "WebIO API", Version = "v1"});
    });

    if (config.GetValue<bool>("Tracing:Enabled"))
    {
      services.AddOpenTelemetry()
        .WithTracing(b =>
        {
          b.AddSource(AppName);
          b.AddEntityFrameworkCoreInstrumentation(o =>
          {
            o.SetDbStatementForText = true;
            o.SetDbStatementForStoredProcedure = true;
          });
          b.AddAspNetCoreInstrumentation();
          b.AddSqlClientInstrumentation();
          b.SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(AppName));
          b.AddJaegerExporter();
        });
    }

    return services;
  }
}
