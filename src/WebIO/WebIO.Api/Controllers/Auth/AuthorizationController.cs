namespace WebIO.Api.Controllers.Auth;

using DataAccess.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
  public ActionResult<bool> WriteAccess()
  {
    var username = TokenHelper.GetUsernameFromClaims(User.Claims);
    return Ok(!string.IsNullOrWhiteSpace(username) && _context.AdminUsers.Any(u => u.Email == username));
  }
}