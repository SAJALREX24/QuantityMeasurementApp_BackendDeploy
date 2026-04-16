using System.Text;
using Microsoft.IdentityModel.Tokens;
using QuantityMeasurementAppRepositoryLayer.Data;
using Microsoft.EntityFrameworkCore;
using QuantityMeasurementAppRepositoryLayer.Interface;
using QuantityMeasurementAppRepositoryLayer.Database;
using QuantityMeasurementAppBusinessLayer.Interface;
using QuantityMeasurementAppBusinessLayer.Service;
using Microsoft.OpenApi;


var builder = WebApplication.CreateBuilder(args);

// Render provides PORT env var — bind Kestrel to 0.0.0.0:<PORT>
var port = Environment.GetEnvironmentVariable("PORT") ?? "5042";
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

// Database: Neon PostgreSQL
// Reads from ConnectionStrings__DefaultConnection env var (Render) or appsettings (local)
builder.Services.AddDbContext<UserDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// DI
builder.Services.AddScoped<IQuantityMeasurementRepository, QuantityMeasurementRepository>();
builder.Services.AddScoped<IQuantityMeasurementService, QuantityMeasurementService>();
builder.Services.AddScoped<IAuthService, QuantityMeasurementAuthService>();

builder.Services.AddOpenApi();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Paste your JWT Token here."
    });

    c.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("Bearer", document)] = []
    });
});

// JWT — can override via env vars Jwt__Key / Jwt__Issuer / Jwt__Audience on Render
var jwtKey = builder.Configuration["Jwt:Key"] ?? "THIS_IS_A_SUPER_SECRET_KEY_1234567890";
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "QuantityMeasurementApp.Api";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "QuantityMeasurementApp.Api";

builder.Services.AddAuthentication("Bearer").AddJwtBearer("Bearer", options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
    };
});
builder.Services.AddAuthorization();

// CORS — comma-separated origins from CORS_ORIGINS env var, fallback to localhost:4200
var allowedOrigins = (Environment.GetEnvironmentVariable("CORS_ORIGINS") ?? "http://localhost:4200")
    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policy => policy.WithOrigins(allowedOrigins)
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials());
});

var app = builder.Build();

// Auto-create database schema on startup (creates tables in Neon on first deploy)
// EnsureCreated reads the DbContext model and issues CREATE TABLE statements directly.
// No migration files needed.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<UserDbContext>();
    db.Database.EnsureCreated();
}

// Enable Swagger everywhere so you can test the deployed API
app.UseSwagger();
app.UseSwaggerUI();

app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Health check at root
app.MapGet("/", () => Results.Ok(new { status = "ok", service = "QuantityMeasurementApp.Api" }));

app.Run();
