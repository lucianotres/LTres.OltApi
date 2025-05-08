using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

var services = builder.Services;
var configuration = builder.Configuration;

services.AddHttpClient();
services.AddOptions();

var authBuilder = services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = "UNKNOWN";
})
.AddCookie();

var OIDCoptions = configuration.GetSection("OIDC").GetChildren();

foreach (var oidcOptions in OIDCoptions)
{
    var name = oidcOptions.GetValue<string>("name")?.ToLower();
    var enabled = oidcOptions.GetValue<bool?>("enabled");

    if (string.IsNullOrEmpty(name) || !enabled.GetValueOrDefault(true))
        continue;
        
    authBuilder.AddOpenIdConnect(name, options =>
    {
        oidcOptions.Bind(options);
        options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.CallbackPath = $"/api/Account/login-callback/{name}"; 
        options.SignedOutCallbackPath = $"/api/Account/logout-callback/{name}";
        options.Scope.Add("profile");
        options.SaveTokens = true;
        options.ResponseType = OpenIdConnectResponseType.IdToken;
        options.GetClaimsFromUserInfoEndpoint = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            NameClaimType = "name",
            RoleClaimType = "role"
        };
        //options.ClaimActions.MapAll();
        options.ClaimActions.MapCustomJson("authentication_scheme", f => name);
    });
}

services.AddRazorPages().AddMvcOptions(options =>
{
    //var policy = new AuthorizationPolicyBuilder()
    //    .RequireAuthenticatedUser()
    //    .Build();
    //options.Filters.Add(new AuthorizeFilter(policy));
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error");
}

//app.UseHttpsRedirection();
app.UseBlazorFrameworkFiles();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();
app.MapControllers();

app.Map("/api/{**segment}", context =>
{
    context.Response.StatusCode = StatusCodes.Status404NotFound;
    return Task.CompletedTask;
});
app.MapFallbackToPage("/_Host");

app.Run();
