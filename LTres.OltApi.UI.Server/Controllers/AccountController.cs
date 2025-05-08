using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using RouteAttribute = Microsoft.AspNetCore.Mvc.RouteAttribute;

namespace LTres.OltApi.UI.Server;


[Route("api/[controller]")]
public class AccountController : ControllerBase
{
    [HttpGet("Schemes")]
    public async Task<IActionResult> GetSchemes([FromServices] IAuthenticationSchemeProvider schemeProvider)
    {
        var schemes = await schemeProvider.GetAllSchemesAsync();
        return Ok(schemes
            .Select(s => s.Name)
            .Where(w => !w.Equals("cookies", StringComparison.InvariantCultureIgnoreCase))
            .ToList());
    }

    [HttpGet("Login/{scheme}")]
    public IActionResult Login(string scheme, string returnUrl)
    {
        var redirectUrl = !string.IsNullOrEmpty(returnUrl) ? returnUrl : "/";
        return Challenge(new AuthenticationProperties { RedirectUri = redirectUrl }, scheme.ToLower());
    }

    [Authorize]
    [HttpGet("Logout")]
    public IActionResult Logout()
    {
        var authProperties = new AuthenticationProperties
        {
            RedirectUri = "/"
        };

        var authScheme = User.FindFirstValue("authentication_scheme");
        if (authScheme != null && authScheme != "google")
            return SignOut(authProperties, CookieAuthenticationDefaults.AuthenticationScheme, authScheme);

        return SignOut(authProperties, CookieAuthenticationDefaults.AuthenticationScheme);
    }

}