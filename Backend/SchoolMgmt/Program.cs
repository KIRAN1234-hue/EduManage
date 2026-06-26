using Hangfire;
using Hangfire.Dashboard;
using Microsoft.EntityFrameworkCore;
using SchoolMgmt.Data;
using SchoolMgmt.Data.Seed;
using SchoolMgmt.Extensions;
using SchoolMgmt.Filters;
using SchoolMgmt.Services;

var builder = WebApplication.CreateBuilder(args);

// ── Register services ────────────────────────────────────────
builder.Services.AddDatabase(builder.Configuration);
builder.Services.AddIdentityConfiguration();
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddRepositories();
builder.Services.AddApplicationServices();
builder.Services.AddCorsConfiguration();
builder.Services.AddRedisCache(builder.Configuration);
builder.Services.AddHangfireServices(builder.Configuration);
builder.Services.AddEmailService(builder.Configuration);
builder.Services.AddAuditFilter();

builder.Services.AddControllers(options =>
{
    options.Filters.AddService<AuditActionFilter>();
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerWithJwt();

var app = builder.Build();

// ── Migration + Seed ─────────────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<AppDbContext>();
        await context.Database.MigrateAsync();
        await DatabaseSeeder.SeedAsync(services);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Error during migration or seeding.");
        throw;
    }
}

// ── Hangfire Dashboard FIRST ─────────────────────────────────
app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = new[] { new HangfireAuthFilter() }
});

// ── THEN Register Recurring Jobs ─────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var jobManager = scope.ServiceProvider
        .GetRequiredService<IRecurringJobManager>();
    HangfireJobRegistration.RegisterRecurringJobs(jobManager);
}

// ── Middleware Pipeline ───────────────────────────────────────
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AngularApp");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();