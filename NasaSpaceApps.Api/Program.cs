using System.Text;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using NasaSpaceApps.Api.Common;
using NasaSpaceApps.Application.Common;
using NasaSpaceApps.Application.Interfaces;
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
builder.Services.AddSwaggerGen();
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddHttpContextAccessor();

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

// CORS — allow all origins for now; restrict after deployment
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy => policy
        .AllowAnyOrigin()
        .AllowAnyHeader()
        .AllowAnyMethod());
});

var jwtKey = builder.Configuration["Jwt:Key"] ?? "ThisIsADevelopmentJwtSigningKey1234567890";
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "NasaSpaceApps";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "NasaSpaceApps.Client";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
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
    var db = scope.ServiceProvider.GetRequiredService<NasaSpaceAppsDbContext>();
    DbInitializer.Initialize(db);
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

app.Run();
