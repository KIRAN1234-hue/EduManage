using Hangfire.Dashboard;

namespace SchoolMgmt.Filters;

public class HangfireAuthFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        var httpContext = context.GetHttpContext();
        // In production: check if user is Principal
        // For dev: allow all
        return httpContext.User.Identity?.IsAuthenticated == true
            && httpContext.User.IsInRole("Principal");
    }
}