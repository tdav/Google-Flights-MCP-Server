using GoogleFlightsApi.Data;
using GoogleFlightsApi.Logging;
using GoogleFlightsApi.Middleware;
using GoogleFlightsApi.Services;
using Microsoft.EntityFrameworkCore;
using Serilog;

internal class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        SerilogConfiguration.ConfigureSerilog(builder);

        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
            {
                Title = "Google Flights MCP Server API",
                Version = "v1",
                Description = "API for searching Google Flights with search history tracking"
            });
        });

        builder.Services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy =>
            {
                if (builder.Environment.IsDevelopment())
                {
                    policy.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader();
                }
                else
                {
                    policy.WithOrigins(builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>())
                          .AllowAnyMethod()
                          .AllowAnyHeader();
                }
            });
        });

        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
        builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(connectionString));


        builder.Services.AddHttpClient<IFlightSearchService, FlightSearchService>();
        builder.Services.AddScoped<IClientTrackingService, ClientTrackingService>();
        builder.Services.AddScoped<ISearchHistoryService, SearchHistoryService>();

        var app = builder.Build();

        // Apply migrations automatically (for development)
        if (app.Environment.IsDevelopment())
        {
            using var scope = app.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            try
            {
                if (db.Database.IsRelational())
                {
                    db.Database.Migrate();
                }
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Could not apply migrations");
            }
        }

        // Configure the HTTP request pipeline
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "Google Flights API v1");
                options.RoutePrefix = "swagger";
            });
        }

        app.UseSerilogRequestLogging();
        app.UseRequestLogging();

        app.UseCors();
        app.UseHttpsRedirection();

        app.UseAuthorization();

        app.MapControllers();

        // Root endpoint
        app.MapGet("/", () => new
        {
            name = "Google Flights MCP Server - Web API",
            version = "1.0.0",
            description = "API for searching Google Flights with search history tracking",
            endpoints = new
            {
                swagger = "/swagger",
                health = "/api/health",
                search = "/api/flights/search",
                history = "/api/history/my",
                allHistory = "/api/history/all"
            }
        }).WithName("Root").WithOpenApi();

        try
        {
            Log.Information("Starting Google Flights API");
            app.Run();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Application terminated unexpectedly");
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }
}