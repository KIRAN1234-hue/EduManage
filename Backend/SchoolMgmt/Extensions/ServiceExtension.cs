using FluentValidation;
using Hangfire;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using SchoolMgmt.Data;
using SchoolMgmt.Entities;
using SchoolMgmt.Filters;
using SchoolMgmt.Repositories.Implementations;
using SchoolMgmt.Repositories.Interfaces;
using SchoolMgmt.Services;
using SchoolMgmt.Services.Implementations;
using SchoolMgmt.Services.Interfaces;
using SchoolMgmt.Settings;
using System.Text;

namespace SchoolMgmt.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddDatabase(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                sql => sql.MigrationsAssembly("SchoolMgmt")
            )
        );
        return services;
    }

    public static IServiceCollection AddIdentityConfiguration(
        this IServiceCollection services)
    {
        services.AddIdentityCore<ApplicationUser>(options =>
        {
            options.Password.RequiredLength = 8;
            options.Password.RequireDigit = true;
            options.Password.RequireUppercase = true;
            options.Password.RequireNonAlphanumeric = true;

            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
            options.Lockout.MaxFailedAccessAttempts = 5;
            options.Lockout.AllowedForNewUsers = true;

            options.User.RequireUniqueEmail = true;
        })
        .AddRoles<IdentityRole<Guid>>()       
        .AddEntityFrameworkStores<AppDbContext>()
        .AddDefaultTokenProviders();

        return services;
    }

    public static IServiceCollection AddJwtAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Bind JwtSettings section to the class — inject anywhere with IOptions<JwtSettings>
        services.Configure<JwtSettings>(configuration.GetSection("JwtSettings"));

        var jwtSettings = configuration.GetSection("JwtSettings").Get<JwtSettings>()!;

        
        services.AddAuthentication(options =>
        {
            // Every request must have a JWT Bearer token by default
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtSettings.Issuer,
                ValidAudience = jwtSettings.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),

                // ClockSkew = Zero means token expires EXACTLY at expiry time
                // Default is 5 minutes which means a 15-min token actually lives 20 min
                ClockSkew = TimeSpan.Zero
            };
        });

        return services;
    }

    public static IServiceCollection AddRepositories(
        this IServiceCollection services)
    {
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        return services;
    }

    public static IServiceCollection AddApplicationServices(
        this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IAttendanceService, AttendanceService>();
        services.AddScoped<IMarksService, MarksService>();

        services.AddScoped<IAdminService, AdminService>();

        services.AddScoped<IAssignmentService, AssignmentService>();
        services.AddScoped<INoticeService, NoticeService>();
        services.AddScoped<ILeaveService, LeaveService>();

        services.AddScoped<IDashboardService, DashboardService>();
        services.AddScoped<ITimetableService, TimetableService>();
        services.AddScoped<IExamScheduleService, ExamScheduleService>();

        services.AddScoped<IFeeService, FeeService>();
        services.AddScoped<ILibraryService, LibraryService>();

        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IMessageService, MessageService>();

        services.AddScoped<IBackgroundJobService, BackgroundJobService>();
        services.AddScoped<ICacheService, CacheService>();

        return services;
    }

    public static IServiceCollection AddCorsConfiguration(
        this IServiceCollection services)
    {
        services.AddCors(options =>
        {
            options.AddPolicy("AngularApp", policy =>
            {
                policy.WithOrigins("http://localhost:4200")
                      .AllowAnyHeader()
                      .AllowAnyMethod()
                      .AllowCredentials();
            });
        });
        return services;
    }

    public static IServiceCollection AddSwaggerWithJwt(
        this IServiceCollection services)
    {
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "School Management API",
                Version = "v1"
            });

            // Adds the Authorize button to Swagger UI
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Enter: Bearer {your JWT token here}"
            });

            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id   = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });
        });

        return services;
    }

    // ── Week 2 onwards ────────────────────────────────────────────────────────
    public static IServiceCollection AddMappingAndValidation(
        this IServiceCollection services)
    {
        services.AddAutoMapper(typeof(Program).Assembly);
        services.AddValidatorsFromAssemblyContaining<Program>();
        return services;
    }

    // ADD these methods to ServiceExtensions.cs:

    public static IServiceCollection AddRedisCache(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<RedisSettings>(
            configuration.GetSection("RedisSettings"));

        var useInMemory = configuration
        .GetValue<bool>("UseInMemoryCache");

        if (useInMemory)
        {
            // Use in-memory cache for development — no Redis needed
            services.AddDistributedMemoryCache();
        }
        else
        {
            var redisConnString = configuration
                .GetSection("RedisSettings:ConnectionString").Value
                ?? "localhost:6379";

            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = redisConnString;
                options.InstanceName = "SchoolMgmt:";
            });
        }

        services.AddScoped<ICacheService, CacheService>();

        return services;
    }

    public static IServiceCollection AddHangfireServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connString = configuration.GetConnectionString("DefaultConnection");

        services.AddHangfire(config =>
            config
                .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UseSqlServerStorage(connString));

        services.AddHangfireServer();

        return services;
    }

    public static IServiceCollection AddEmailService(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<SendGridSettings>(
            configuration.GetSection("SendGridSettings"));

        services.AddScoped<IEmailService, EmailService>();

        return services;
    }

    public static IServiceCollection AddAuditFilter(
        this IServiceCollection services)
    {
        services.AddScoped<AuditActionFilter>();
        return services;
    }
}