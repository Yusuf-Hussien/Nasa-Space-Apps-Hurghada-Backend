using System.Text;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using NasaSpaceApps.Api.Common;
using NasaSpaceApps.Application.Common;
using NasaSpaceApps.Application.Interfaces;
using NasaSpaceApps.Application.Models;
using NasaSpaceApps.Application.Services;
using NasaSpaceApps.Domain.Entities;
using NasaSpaceApps.Infrastructure.Data;
using NasaSpaceApps.Infrastructure.Repositories;
using NasaSpaceApps.Infrastructure.Services;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, loggerConfiguration) =>
    loggerConfiguration
        .ReadFrom.Configuration(context.Configuration)
        .Enrich.FromLogContext()
        .WriteTo.Console());

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "NASA Space Apps Hurghada API",
        Version = "v1"
    });

    // Adds the "Authorize" button to Swagger UI. Paste the raw JWT
    // (no "Bearer " prefix) returned by the login endpoints.
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Paste the accessToken from POST /api/v1/auth/login/staff or /api/v1/auth/login/participant (without the 'Bearer ' prefix)."
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddHttpContextAccessor();
builder.Services.AddSignalR();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<NasaSpaceAppsDbContext>(options =>
{
    if (!string.IsNullOrWhiteSpace(connectionString))
    {
        options.UseSqlServer(connectionString);
    }
    else
    {
        options.UseInMemoryDatabase("NasaSpaceApps");
    }
});

builder.Services.AddScoped<INasaSpaceAppsRepository, NasaSpaceAppsRepository>();
builder.Services.AddScoped<IPasswordHasher, PasswordHasherService>();
builder.Services.AddScoped<ITokenService, JwtTokenService>();
builder.Services.AddScoped<ICurrentUser, CurrentUser>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<TeamService>();
builder.Services.AddScoped<ChallengeService>();
builder.Services.AddScoped<RequestService>();
builder.Services.AddScoped<AttendanceService>();
builder.Services.AddScoped<JudgingService>();
builder.Services.AddScoped<SponsorService>();
builder.Services.AddScoped<QuestionService>();
builder.Services.AddScoped<GameService>();
builder.Services.AddScoped<StaffService>();
builder.Services.AddScoped<SeasonMediaService>();

// CORS: allow all origins for now; restrict after deployment
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy => policy
        .AllowAnyOrigin()
        .AllowAnyHeader()
        .AllowAnyMethod());
});

var jwtOptions = JwtOptions.FromConfiguration(builder.Configuration);
builder.Services.AddSingleton(jwtOptions);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidAudience = jwtOptions.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Key))
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", p => p.RequireRole(AppRoles.Admin));
    options.AddPolicy("CoreTeamOnly", p => p.RequireRole(AppRoles.CoreTeam));
    options.AddPolicy("JudgeOnly", p => p.RequireRole(AppRoles.Judge));
    options.AddPolicy("StaffOnly", p => p.RequireRole(AppRoles.Admin, AppRoles.CoreTeam, AppRoles.Judge));
    options.AddPolicy("ParticipantOnly", p => p.RequireRole(AppRoles.Participant));
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    try
    {
        var db = scope.ServiceProvider.GetRequiredService<NasaSpaceAppsDbContext>();
        DbInitializer.Initialize(db);
    }
    catch (Exception ex)
    {
        // Log but do not crash: the app can still start and serve Swagger etc.
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("DbInitializer");
        logger.LogError(ex, "Database initialization failed. The API will start but database endpoints may not work.");
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHub<NasaSpaceApps.Api.Hubs.RequestsHub>(NasaSpaceApps.Api.Hubs.RequestsHub.Url);
app.MapHub<NasaSpaceApps.Api.Hubs.JudgingHub>(NasaSpaceApps.Api.Hubs.JudgingHub.Url);

app.Run();
