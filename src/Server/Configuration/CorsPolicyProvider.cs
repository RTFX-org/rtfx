using MaSch.Core.Extensions;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.Extensions.Options;

namespace Rtfx.Server.Configuration;

internal sealed class CorsPolicyProvider : ICorsPolicyProvider
{
    private readonly IOptionsMonitor<CorsOptions> _corsOptions;
    private readonly ICorsPolicyProvider _inner;

    public CorsPolicyProvider(IOptions<Microsoft.AspNetCore.Cors.Infrastructure.CorsOptions> options, IOptionsMonitor<CorsOptions> corsOptions)
    {
        _corsOptions = corsOptions;
        _inner = new DefaultCorsPolicyProvider(options);
    }

    public async Task<CorsPolicy?> GetPolicyAsync(HttpContext context, string? policyName)
    {
        CorsPolicy? policy = await _inner.GetPolicyAsync(context, policyName).ConfigureAwait(false);
        if (policy != null && policyName == null)
            ApplyConfiguration(policy, _corsOptions.CurrentValue);

        return policy;
    }

    private static void ApplyConfiguration(CorsPolicy policy, CorsOptions options)
    {
        ICollection<string>? allowedOrigins = options.AllowedOrigins;
        if (allowedOrigins != null)
            policy.Origins.Set(allowedOrigins);
    }
}
