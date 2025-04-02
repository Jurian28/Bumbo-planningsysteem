using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

public class RoleBasedRedirectHandler : AuthorizationHandler<RoleBasedRedirectRequirement>
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public RoleBasedRedirectHandler(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, RoleBasedRedirectRequirement requirement)
    {
        var httpContext = _httpContextAccessor.HttpContext;

        if (context.User.IsInRole("Manager"))
        {
            httpContext.Response.Redirect("/Home/Index");
        }
        else if (context.User.IsInRole("Employee"))
        {
            httpContext.Response.Redirect("/Home/EmptyView");
        }

        context.Succeed(requirement);
        return Task.CompletedTask;
    }
}