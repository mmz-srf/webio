namespace WebIO.Api.Controllers.Auth;

using DataAccess.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

public class AdminAuthorizationHandler : AuthorizationHandler<IsAdminRequirement>
{
  private readonly AppDbContext _context;

  public AdminAuthorizationHandler(AppDbContext context)
  {
    _context = context;
  }

  protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, IsAdminRequirement requirement)
  {
    var username = TokenHelper.GetUsernameFromClaims(context.User.Claims);

    if (!string.IsNullOrWhiteSpace(username) && _context.AdminUsers.Any(u => u.Email == username))
    {
      context.Succeed(requirement);
    }

    return Task.CompletedTask;
  }
}