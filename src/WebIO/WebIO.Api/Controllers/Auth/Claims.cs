namespace WebIO.Api.Controllers.Auth;

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

public static class Claims
{
  public const string CanEdit = "ReadWrite";
  public const string CanRead = "Read";
}

public static class TokenHelper
{
  public static string GetUserNameFromBearerToken(string? bearerToken)
  {
    if (bearerToken == null)
    {
      return string.Empty;
    }

    var token = bearerToken[7..];
    var jwtToken = new JwtSecurityToken(token);
    var claims = jwtToken.Claims.ToList();
    var username = GetUsernameFromClaims(claims);
    return username;
  }

  public static string GetUsernameFromClaims(IEnumerable<Claim> claims)
    => claims.FirstOrDefault(claim
        => claim.Type.Equals("upn") ||
           claim.Type.Equals("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/upn"))?
      .Value
      .ToLowerInvariant()
    ?? string.Empty;
}