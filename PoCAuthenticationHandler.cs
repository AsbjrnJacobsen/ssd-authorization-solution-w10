using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace ssd_authorization_solution;

public class PoCAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public PoCAuthenticationHandler(IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        ISystemClock clock)
        : base(options, logger, encoder, clock)
    {
        Logger.LogInformation("PoCAuthenticationHandler");
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        Logger.LogInformation($"{Request.Headers.Authorization.ToString()}");
        //check for custom header
        if(!Request.Headers.ContainsKey("X-User"))
            return Task.FromResult(AuthenticateResult.Fail("Missing X-User"));
        var username = Request.Headers["X-User"].ToString();
        var rolesHeader = Request.Headers.ContainsKey("X-Roles")? Request.Headers["X-Roles"].ToString() : string.Empty;
        var roles = rolesHeader.Split(",");
        
        Logger.LogInformation($"User: {username}, rolesHeader: {rolesHeader}, Roles: {roles}");
        
        //Create Claims
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, username),
            new Claim(ClaimTypes.NameIdentifier, username)
        };

        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role.Trim()));
        }
        
        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);
        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}