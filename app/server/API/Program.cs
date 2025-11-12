using API.Data;
using API.Hubs;
using API.Middleware;
using API.Repositories;
using API.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                       ?? throw new InvalidOperationException("ConnectionString 'DefaultConnection' is not configured");

builder.Services.AddSingleton<IDbConnectionFactory>(new DbConnectionFactory(connectionString));

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IConversationRepository, ConversationRepository>();
builder.Services.AddScoped<IAgentActionRepository, AgentActionRepository>();

builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IAgentService, AgentService>();
builder.Services.AddScoped<ISpotifyTokenService, SpotifyTokenService>();
builder.Services.AddHttpClient<ISpotifyService, SpotifyService>();
builder.Services.AddHttpClient<IAIService, AIService>();
builder.Services.AddHttpClient(); // Add general HttpClient factory
builder.Services.AddHttpContextAccessor();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
                             ?? new[] { "http://localhost:5173" };

        policy.WithOrigins(allowedOrigins)
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

builder.Services.AddSignalR();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var authority = builder.Configuration["Keycloak:Authority"]
                       ?? throw new InvalidOperationException("Keycloak:Authority is not configured");
        var metadataAddress = builder.Configuration["Keycloak:MetadataAddress"]
                             ?? throw new InvalidOperationException("Keycloak:MetadataAddress is not configured");

        options.Authority = authority;
        options.MetadataAddress = metadataAddress;
        options.RequireHttpsMetadata = true;
        options.SaveToken = true;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ClockSkew = TimeSpan.FromMinutes(5)
        };

        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                logger.LogError(context.Exception, "Authentication failed");
                return Task.CompletedTask;
            },
            OnTokenValidated = context =>
            {
                var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                var email = context.Principal?.FindFirst("email")?.Value ?? "unknown";
                logger.LogDebug("Token validated for user: {Email}", email);
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddControllers();
builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors();

app.UseAuthentication();
app.UseAuthorization();

app.UseMiddleware<UserContextMiddleware>();
app.MapGet("/", () => "API is running!");
app.MapControllers().RequireAuthorization();
app.MapHub<AgentHub>("/hubs/agent").RequireAuthorization();

app.Run();

public partial class Program { }
