using DevContextNexus.API.Data;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        options.SuppressModelStateInvalidFilter = true;
    });
builder.Services.AddOpenApi();

// Configure Settings
builder.Services.Configure<DevContextNexus.API.Configuration.GitHubSettings>(
    builder.Configuration.GetSection("GitHubSettings"));

// Register Services
builder.Services.AddHttpClient<DevContextNexus.API.Services.IGitHubService, DevContextNexus.API.Services.GitHubService>();
builder.Services.AddScoped<DevContextNexus.API.Services.IContextService, DevContextNexus.API.Services.ContextService>();
builder.Services.AddScoped<DevContextNexus.API.Services.ICloudinaryService, DevContextNexus.API.Services.CloudinaryService>();

// Register DbContext with SQL Server
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions => sqlOptions.EnableRetryOnFailure()));

var app = builder.Build();

// Automatically apply migrations on startup
using (var scope = app.Services.CreateScope())
{
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    try
    {
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        logger.LogInformation("Applying migrations...");
        db.Database.Migrate();
        logger.LogInformation("Migrations applied successfully.");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred while applying migrations.");
    }
}

// Configure the HTTP request pipeline.
app.MapOpenApi();
app.MapScalarApiReference();

// app.UseHttpsRedirection(); // Disabled to prevent POST to GET redirection issues on Railway Proxy

app.UseMiddleware<DevContextNexus.API.Middleware.ApiKeyMiddleware>();

app.UseAuthorization();

app.MapControllers();

app.Run();
