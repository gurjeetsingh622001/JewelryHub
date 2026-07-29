using System.Text;
using JewelryHub.API.Hubs;
using JewelryHub.API.Services;
using JewelryHub.Application;
using JewelryHub.Application.Common.Interfaces;
using JewelryHub.Infrastructure;
using JewelryHub.Persistence;
using JewelryHub.Persistence.Extensions;
using JewelryHub.Persistence.Seed;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.SignalR;
using Microsoft.IdentityModel.Tokens;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, configuration) =>
    configuration.ReadFrom.Configuration(context.Configuration));

// --- Layer registrations (each layer owns its own DI extension) ---
builder.Services.AddApplication();
builder.Services.AddPersistence(builder.Configuration);
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddControllers();
builder.Services.AddSignalR();
builder.Services.AddSingleton<IUserIdProvider, SubClaimUserIdProvider>();
builder.Services.AddScoped<IRealtimeNotifier, SignalRNotifier>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new() { Title = "JewelryHub API", Version = "v1" });

    var bearerScheme = new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Enter a valid JWT access token.",
    };
    options.AddSecurityDefinition("Bearer", bearerScheme);
    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        { bearerScheme, Array.Empty<string>() },
    });
});

var jwtSection = builder.Configuration.GetSection("Jwt");
builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        // Keep the raw "sub"/"email"/"role" claim names instead of ASP.NET
        // Core's default remapping to long schemas.microsoft.com URIs —
        // simpler to reason about in CurrentUserService and in the token itself.
        options.MapInboundClaims = false;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtSection["Issuer"],
            ValidateAudience = true,
            ValidAudience = jwtSection["Audience"],
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSection["Secret"]!)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30),
            RoleClaimType = System.Security.Claims.ClaimTypes.Role,
        };

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
                {
                    context.Token = accessToken;
                }
                return Task.CompletedTask;
            },
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AngularApp", policy =>
    {
        var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
            ?? new[] { "http://localhost:4200" };
        policy.WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod().AllowCredentials();
    });
});

var app = builder.Build();

// Idempotent: applies pending migrations + seeds system roles on every
// startup. Fine for this stage of the project; swap for a dedicated
// migration step in a real CI/CD pipeline once deployments matter.
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<JewelryHubDbContext>();
    await DbSeeder.SeedAsync(dbContext);
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<JewelryHub.API.Middleware.ExceptionHandlingMiddleware>();

app.UseHttpsRedirection();
app.UseCors("AngularApp");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<NotificationsHub>("/hubs/notifications");

app.Run();
