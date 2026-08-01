namespace BuildQuickPkg.Templates;

/// <summary>
/// Produces the <c>Caddyfile</c> written by <c>BuildQuickPkg add caddy</c>, pointing at the
/// project's actual configured HTTP port rather than a hardcoded placeholder.
/// </summary>
internal static class CaddyTemplate
{
    public static string Generate(int httpPort) => $$"""
        # Replace `localhost` with your real domain before deploying.
        # Caddy automatically provisions and renews a TLS certificate for any public domain.
        localhost {
            reverse_proxy localhost:{{httpPort}}
        }
        """;
}
