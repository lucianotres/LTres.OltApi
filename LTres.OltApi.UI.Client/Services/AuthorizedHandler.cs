using Microsoft.AspNetCore.Components.Authorization;
using System.Net;

namespace LTres.OltApi.UI.Client.Services;

public class AuthorizedHandler : DelegatingHandler
{
    private readonly HostAuthenticationStateProvider _authenticationStateProvider;

    public AuthorizedHandler(AuthenticationStateProvider authenticationStateProvider)
        => _authenticationStateProvider = (HostAuthenticationStateProvider)authenticationStateProvider;

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var authState = await _authenticationStateProvider.GetAuthenticationStateAsync();
        HttpResponseMessage responseMessage;
        if (authState.User.Identity != null && !authState.User.Identity.IsAuthenticated)
        {
            // if user is not authenticated, immediately set response status to 401 Unauthorized
            responseMessage = new HttpResponseMessage(HttpStatusCode.Unauthorized);
        }
        else
        {
            responseMessage = await base.SendAsync(request, cancellationToken);
        }

        if (responseMessage.StatusCode == HttpStatusCode.Unauthorized)
        {
            // if server returned 401 Unauthorized, redirect to login page
            _authenticationStateProvider.SignIn();
        }

        return responseMessage;
    }
}