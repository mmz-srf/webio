namespace WebIO.Api.Controllers.Auth;

using DataAccess.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Web.Resource;

[Route("api/[controller]")]
[Authorize]
[RequiredScope(Claims.CanRead)]
[ApiController]
public class AuthorizationController : Controller
{
  private readonly AppDbContext _context;

  public AuthorizationController(AppDbContext context)
  {
    _context = context;
  }

  [HttpGet("write-access")]
  public async Task<ActionResult<bool>> WriteAccess(CancellationToken ct)
  {
    var username = TokenHelper.GetUsernameFromClaims(User.Claims);
    return Ok(!string.IsNullOrWhiteSpace(username) && await _context.AdminUsers.AnyAsync(u => u.Email == username, ct));
  }
}