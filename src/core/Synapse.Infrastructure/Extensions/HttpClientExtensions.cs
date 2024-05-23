using System.Net.Http.Headers;

namespace Synapse;

/// <summary>
/// Defines extensions for <see cref="HttpClient"/>s
/// </summary>
public static class HttpClientExtensions
{

    /// <summary>
    /// Configures the <see cref="HttpClient"/> to use the specified authentication mechanism 
    /// </summary>
    /// <param name="httpClient">The <see cref="HttpClient"/> to configure</param>
    /// <param name="authentication">An object that describes the authentication mechanism to use</param>
    /// <param name="serviceProvider">The current <see cref="IServiceProvider"/></param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
    /// <returns>A new awaitable <see cref="Task"/></returns>
    public static async Task ConfigureAuthenticationAsync(this HttpClient httpClient, AuthenticationPolicyDefinition? authentication, IServiceProvider serviceProvider, CancellationToken cancellationToken = default)
    {
        if (authentication == null) return;
        var authorization = await AuthorizationInfo.CreateAsync(authentication, serviceProvider, cancellationToken).ConfigureAwait(false);
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(authorization.Scheme, authorization.Parameter);
    }

}
