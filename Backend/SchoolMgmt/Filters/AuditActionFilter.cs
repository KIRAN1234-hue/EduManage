using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SchoolMgmt.Data;
using SchoolMgmt.Entities;

namespace SchoolMgmt.Filters;

public class AuditActionFilter : IAsyncActionFilter
{
    private readonly AppDbContext _context;

    public AuditActionFilter(AppDbContext context)
    {
        _context = context;
    }

    public async Task OnActionExecutionAsync(
        ActionExecutingContext context,
        ActionExecutionDelegate next)
    {
        var method = context.HttpContext.Request.Method;

        // Only audit write operations
        if (method != "POST" && method != "PUT" && method != "DELETE")
        {
            await next();
            return;
        }

        var executedContext = await next();

        // Only log successful responses
        var statusCode =
            (executedContext.Result as ObjectResult)?.StatusCode
         ?? (executedContext.Result as StatusCodeResult)?.StatusCode
         ?? 200;

        if (statusCode < 200 || statusCode >= 300) return;

        // UserId is required (not nullable) — skip if not authenticated
        var userIdString = context.HttpContext.User
            .FindFirstValue(ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(userIdString, out var userId)) return;

        // Entity name from controller route
        var routeData = context.HttpContext.Request.RouteValues;
        var controller = routeData["controller"]?.ToString() ?? "Unknown";

        // Try to extract entity ID from route
        var entityId =
            routeData["id"]?.ToString()
         ?? routeData.FirstOrDefault(r =>
                r.Key.EndsWith("Id", StringComparison.OrdinalIgnoreCase) &&
                r.Key != "controller" &&
                r.Value?.ToString()?.Length == 36
            ).Value?.ToString()
         ?? string.Empty;

        var action = method switch
        {
            "POST" => "Create",
            "PUT" => "Update",
            "DELETE" => "Delete",
            _ => method
        };

        // Capture request body as NewValues for POST/PUT
        var newValues = string.Empty;
        if ((method == "POST" || method == "PUT") &&
            context.ActionArguments.Any())
        {
            try
            {
                var firstArg = context.ActionArguments.Values.FirstOrDefault();
                if (firstArg != null)
                    newValues = JsonSerializer.Serialize(firstArg,
                        new JsonSerializerOptions
                        {
                            WriteIndented = false
                        });
            }
            catch { newValues = string.Empty; }
        }

        var audit = new AuditLog
        {
            Id = Guid.NewGuid(),
            UserId = userId,                    // Guid — not nullable ✅
            Action = action,
            EntityName = controller,
            EntityId = entityId,
            OldValues = null,                      // OldValues is nullable ✅
            NewValues = newValues,                 // NewValues is required ✅
            IPAddress = context.HttpContext.Connection
                             .RemoteIpAddress?.ToString(),
            Timestamp = DateTime.UtcNow
        };

        _context.AuditLogs.Add(audit);

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            // Audit failure must never break the main request
            Console.WriteLine($"Audit log failed: {ex.Message}");
        }
    }
}