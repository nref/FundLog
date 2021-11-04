using Fluxor;
using FundLog.Api.Areas.Identity;
using FundLog.Api.Data;
using FundLog.Api.Services;
using FundLog.ObjectGraph;
using Lamar.Microsoft.DependencyInjection;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System.Security.Claims;

namespace FundLog.Api
{
  public class Program
  {
    public static async Task Main(string[] args)
    {
      var builder = WebApplication.CreateBuilder(args);

      // Add services to the container.
      var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

      ICompositionRoot root = new ApiCompositionRoot(new DomainConfig
      {
        UseSignalRClient = true,
        ConnectionString = connectionString,
      });

      builder.Host.UseLamar((context, registry) => registry.IncludeRegistry(root.Registry));

      var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
      var configuration = new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json")
        .AddJsonFile($"appsettings.{env ?? "Production"}.json", true)
        .Build();

      var logger = new LoggerConfiguration()
          .ReadFrom.Configuration(configuration)
          .CreateLogger();

      builder.Host.UseSerilog(logger);

      builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(connectionString));
      builder.Services.AddDatabaseDeveloperPageExceptionFilter();
      builder.Services
        .AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
        .AddRoles<IdentityRole>()
        .AddEntityFrameworkStores<ApplicationDbContext>();
      builder.Services.AddRazorPages();
      builder.Services.AddServerSideBlazor().AddHubOptions(o => o.MaximumReceiveMessageSize = 5 * 1024 * 1024);
      builder.Services.AddScoped<AuthenticationStateProvider, RevalidatingIdentityAuthenticationStateProvider<IdentityUser>>();
      builder.Services.AddFluxor(o => o.ScanAssemblies(typeof(Program).Assembly).UseReduxDevTools());
      builder.Services.AddTransient<IEmailSender, EmailService>();

      //builder.Services.AddAuthorization();
      //builder.Services.AddAuthentication(options =>
      //{
      //  options.DefaultSignInScheme = JwtBearerDefaults.AuthenticationScheme;
      //  options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
      //  options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
      //  options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
      //})
      //  .AddJwtBearer(options =>
      //  {
      //    options.SaveToken = true;

      //    // Configure the Authority to the expected value for your authentication provider
      //    // This ensures the token is appropriately validated
      //    options.Authority = "https://localhost:5001";

      //    // We have to hook the OnMessageReceived event in order to
      //    // allow the JWT authentication handler to read the access
      //    // token from the query string when a WebSocket or 
      //    // Server-Sent Events request comes in.

      //    // Sending the access token in the query string is required due to
      //    // a limitation in Browser APIs. We restrict it to only calls to the
      //    // SignalR hub in this code.
      //    // See https://docs.microsoft.com/aspnet/core/signalr/security#access-token-logging
      //    // for more information about security considerations when using
      //    // the query string to transmit the access token.
      //    options.Events = new JwtBearerEvents
      //    {
      //      OnMessageReceived = context => {
      //      if (context.Request.Query.ContainsKey("access_token"))
      //          context.Token = context.Request.Query["access_token"];
      //        return Task.CompletedTask;
      //      }
      //    };

      //    options.TokenValidationParameters = new TokenValidationParameters()
      //    {
      //      // TODO enable JWT validation
      //      ValidateAudience = false,
      //      ValidateLifetime = false,
      //      ValidateIssuer = false,
      //      ValidateIssuerSigningKey = false,
      //      ValidateActor = false
      //    };
      //  });

      //builder.Services.AddSingleton<IUserIdProvider, EmailBasedUserIdProvider>();

      var app = builder.Build();

      // Configure the HTTP request pipeline.
      if (app.Environment.IsDevelopment())
      {
        app.UseMigrationsEndPoint();
      }
      else
      {
        app.UseExceptionHandler("/Error");
        // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
        app.UseHsts();
      }

      app.UseHttpsRedirection();

      app.UseStaticFiles();

      //app.UseSignalRQueryStringAuth();
      app.UseRouting();

      app.UseAuthentication();
      app.UseAuthorization();

      app.MapControllers();

      app.UseEndpoints(endpoints =>
      {
        app.MapHub<FundLogHub>("/fundlog");
        app.MapBlazorHub();
        app.MapFallbackToPage("/_Host");
      });

      await app.RunAsync();
    }
  }
}

public class SignalRQueryStringAuthMiddleware
{
  private readonly RequestDelegate _next;

  public SignalRQueryStringAuthMiddleware(RequestDelegate next)
  {
    _next = next;
  }

  /// <summary>
  /// Convert incomming qs auth token to a Authorization header so the rest of the chain
  /// can authorize the request correctly
  /// </summary>
  public async Task Invoke(HttpContext context)
  {
    bool haveToken = context.Request.Query.TryGetValue("access_token", out var token);

    if (haveToken)
    {
      context.Request.Headers.Add("Authorization", "Bearer " + token.First());
    }
    await _next.Invoke(context);
  }
}

public static class ApplicationBuilderExtensions
{
  public static IApplicationBuilder UseSignalRQueryStringAuth(this IApplicationBuilder builder) 
    => builder.UseMiddleware<SignalRQueryStringAuthMiddleware>();
}

public class EmailBasedUserIdProvider : IUserIdProvider
{
  public virtual string GetUserId(HubConnectionContext connection)
  {
    string id = connection.User?.FindFirst(ClaimTypes.Email)?.Value ?? "";
    return id;
  }
}