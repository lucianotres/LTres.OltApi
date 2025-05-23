﻿using LTres.OltApi.UI.Shared.Authorization;
using LTres.OltApi.UI.Shared.Defaults;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using System.Net.Http.Json;
using System.Security.Claims;

namespace LTres.OltApi.UI.Client.Services;

public class HostAuthenticationStateProvider : AuthenticationStateProvider
{
    private static readonly TimeSpan _userCacheRefreshInterval = TimeSpan.FromSeconds(60);

    private readonly NavigationManager _navigation;
    private readonly HttpClient _client;
    private readonly ILogger<HostAuthenticationStateProvider> _logger;

    private DateTimeOffset _userLastCheck = DateTimeOffset.FromUnixTimeSeconds(0);
    private ClaimsPrincipal _cachedUser = new(new ClaimsIdentity());

    public HostAuthenticationStateProvider(NavigationManager navigation, HttpClient client, ILogger<HostAuthenticationStateProvider> logger)
    {
        _navigation = navigation;
        _client = client;
        _logger = logger;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        => new AuthenticationState(await GetUser(useCache: true));

    public void SignIn(string? customReturnUrl = null)
    {
        var returnUrl = customReturnUrl != null ? _navigation.ToAbsoluteUri(customReturnUrl).ToString() : null;
        var encodedReturnUrl = Uri.EscapeDataString(returnUrl ?? _navigation.Uri);
        var logInUrl = _navigation.ToAbsoluteUri($"{AuthDefaults.LogInPath}?returnUrl={encodedReturnUrl}");
        _navigation.NavigateTo(logInUrl.ToString(), true);
    }

    private async ValueTask<ClaimsPrincipal> GetUser(bool useCache = false)
    {
        var now = DateTimeOffset.Now;
        if (useCache && now < _userLastCheck + _userCacheRefreshInterval)
        {
            _logger.LogDebug("Taking user from cache");
            return _cachedUser;
        }

        _logger.LogDebug("Fetching user");
        _cachedUser = await FetchUser();
        _userLastCheck = now;

        return _cachedUser;
    }

    private async Task<ClaimsPrincipal> FetchUser()
    {
        UserInfo? user = null;

        try
        {
            _logger.LogInformation("{clientBaseAddress}", _client.BaseAddress?.ToString());
            user = await _client.GetFromJsonAsync<UserInfo>("api/User");
        }
        catch (Exception exc)
        {
            _logger.LogWarning(exc, "Fetching user failed.");
        }

        if (user == null || !user.IsAuthenticated)
        {
            return new ClaimsPrincipal(new ClaimsIdentity());
        }

        var identity = new ClaimsIdentity(
            nameof(HostAuthenticationStateProvider),
            user.NameClaimType,
            user.RoleClaimType);

        if (user.Claims != null)
        {
            identity.AddClaims(user.Claims.Select(c => new Claim(c.Type, c.Value)));
        }

        return new ClaimsPrincipal(identity);
    }

    public async Task<IList<string>> GetSchemesList()
    {
        try
        {
            return await _client.GetFromJsonAsync<IList<string>>("api/Account/Schemes") ?? [];
        }
        catch (Exception exc)
        {
            _logger.LogWarning(exc, "Getting schemes list failed.");
            return [];
        }
    }
}