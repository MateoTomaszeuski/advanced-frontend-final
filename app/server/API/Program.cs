using API.Data;
using API.Hubs;
using API.Interfaces;
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
builder.Services.AddScoped<IThemeRepository, ThemeRepository>();

builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IAgentService, AgentService>();
builder.Services.AddScoped<IThemeService, ThemeService>();

builder.Services.AddHttpClient<API.Interfaces.ISpotifyAuthService, API.Services.Spotify.SpotifyAuthService>();
builder.Services.AddHttpClient<API.Interfaces.ISpotifyUserService, API.Services.Spotify.SpotifyUserService>();
builder.Services.AddHttpClient<API.Interfaces.ISpotifyPlaylistService, API.Services.Spotify.SpotifyPlaylistService>();
builder.Services.AddHttpClient<API.Interfaces.ISpotifyTrackService, API.Services.Spotify.SpotifyTrackService>();
builder.Services.AddScoped<ISpotifyService, SpotifyService>();
builder.Services.AddScoped<ISpotifyTokenService, SpotifyTokenService>();

builder.Services.AddScoped<API.Interfaces.IAgentNotificationService, API.Services.Agents.AgentNotificationService>();
builder.Services.AddScoped<API.Interfaces.IDuplicateCleanerService, API.Services.Agents.DuplicateCleanerService>();
builder.Services.AddScoped<API.Interfaces.IPlaylistCreatorService, API.Services.Agents.PlaylistCreatorService>();
builder.Services.AddScoped<API.Interfaces.IMusicDiscoveryService, API.Services.Agents.MusicDiscoveryService>();
builder.Services.AddScoped<API.Interfaces.IMusicSuggestionService, API.Services.Agents.MusicSuggestionService>();
builder.Services.AddScoped<API.Interfaces.IAgentAnalyticsService, API.Services.Agents.AgentAnalyticsService>();
builder.Services.AddScoped<API.Interfaces.IAgentHistoryService, API.Services.Agents.AgentHistoryService>();

builder.Services.AddScoped<API.Interfaces.IAIPromptBuilder, API.Services.AI.AIPromptBuilder>();
builder.Services.AddScoped<API.Interfaces.IAIResponseParser, API.Services.AI.AIResponseParser>();

builder.Services.AddScoped<API.Interfaces.IDiscoveryQueryGenerator, API.Services.Helpers.DiscoveryQueryGenerator>();
builder.Services.AddScoped<API.Interfaces.ITrackDiscoveryHelper, API.Services.Helpers.TrackDiscoveryHelper>();

builder.Services.AddScoped<API.Interfaces.IPlaylistAnalyticsService, API.Services.Spotify.PlaylistAnalyticsService>();

builder.Services.AddHttpClient<IAIService, AIService>();
builder.Services.AddHttpClient();
builder.Services.AddHttpContextAccessor();

builder.Services.AddCors(options => {
    options.AddDefaultPolicy(policy => {
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
    .AddJwtBearer(options => {
        var authority = builder.Configuration["Keycloak:Authority"]
                       ?? throw new InvalidOperationException("Keycloak:Authority is not configured");
        var metadataAddress = builder.Configuration["Keycloak:MetadataAddress"]
                             ?? throw new InvalidOperationException("Keycloak:MetadataAddress is not configured");

        options.Authority = authority;
        options.MetadataAddress = metadataAddress;
        options.RequireHttpsMetadata = true;
        options.SaveToken = true;

        options.TokenValidationParameters = new TokenValidationParameters {
            ValidateIssuer = true,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ClockSkew = TimeSpan.FromMinutes(5)
        };

        options.Events = new JwtBearerEvents {
            OnAuthenticationFailed = context => {
                var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                logger.LogError(context.Exception, "Authentication failed");
                return Task.CompletedTask;
            },
            OnTokenValidated = context => {
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

if (app.Environment.IsDevelopment()) {
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