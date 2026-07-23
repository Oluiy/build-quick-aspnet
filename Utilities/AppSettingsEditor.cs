using System.Text.Json;
using System.Text.Json.Nodes;

namespace BuildQuickPkg.Utilities;

/// <summary>
/// Merges new sections into an existing generated <c>appsettings*.json</c> file without disturbing
/// what's already there, used by <c>BuildQuickPkg add</c> to retrofit an optional feature onto an
/// already-generated project.
/// </summary>
internal static class AppSettingsEditor
{
    private static readonly JsonSerializerOptions WriteOptions = new() { WriteIndented = true };

    /// <summary>Adds <c>ConnectionStrings:DefaultConnection</c> if it isn't already present; leaves the file untouched otherwise.</summary>
    public static void AddConnectionString(string appsettingsPath, string connectionString)
    {
        var root = LoadOrCreate(appsettingsPath);

        if (root["ConnectionStrings"] is JsonObject existing && existing.ContainsKey("DefaultConnection"))
        {
            return;
        }

        root["ConnectionStrings"] = new JsonObject { ["DefaultConnection"] = connectionString };
        Save(appsettingsPath, root);
    }

    /// <summary>Adds a <c>Jwt</c> section if it isn't already present; leaves the file untouched otherwise.</summary>
    public static void AddJwtSection(string appsettingsPath, string? issuer, string? audience, string key, int? expiryMinutes)
    {
        var root = LoadOrCreate(appsettingsPath);

        if (root.ContainsKey("Jwt"))
        {
            return;
        }

        var jwt = new JsonObject { ["Key"] = key };
        if (issuer is not null)
        {
            jwt["Issuer"] = issuer;
        }

        if (audience is not null)
        {
            jwt["Audience"] = audience;
        }

        if (expiryMinutes is not null)
        {
            jwt["ExpiryMinutes"] = expiryMinutes.Value;
        }

        root["Jwt"] = jwt;
        Save(appsettingsPath, root);
    }

    private static JsonObject LoadOrCreate(string path)
    {
        if (!File.Exists(path))
        {
            return new JsonObject();
        }

        var content = File.ReadAllText(path);
        return string.IsNullOrWhiteSpace(content)
            ? new JsonObject()
            : JsonNode.Parse(content) as JsonObject ?? new JsonObject();
    }

    private static void Save(string path, JsonObject root) =>
        File.WriteAllText(path, root.ToJsonString(WriteOptions));
}
