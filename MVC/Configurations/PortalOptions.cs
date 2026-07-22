namespace MVC.Configurations;

public sealed class PortalOptions
{
    public const string SectionName = "PortalUrls";

    public string RazorPagesBaseUrl { get; init; } = string.Empty;
    public string BlazorBaseUrl { get; init; } = string.Empty;

    public string GetRazorPagesUrl(string? path = null)
    {
        return Combine(RazorPagesBaseUrl, path);
    }

    public string GetBlazorUrl(string? path = null)
    {
        return Combine(BlazorBaseUrl, path);
    }

    private static string Combine(string baseUrl, string? path)
    {
        var normalizedBaseUrl = baseUrl.TrimEnd('/');
        return string.IsNullOrWhiteSpace(path)
            ? $"{normalizedBaseUrl}/"
            : $"{normalizedBaseUrl}/{path.TrimStart('/')}";
    }

    public static bool HasValidUrls(PortalOptions options)
    {
        return IsHttpUrl(options.RazorPagesBaseUrl) && IsHttpUrl(options.BlazorBaseUrl);
    }

    private static bool IsHttpUrl(string value)
    {
        return Uri.TryCreate(value, UriKind.Absolute, out var uri) &&
               (string.Equals(uri.Scheme, Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(uri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase));
    }
}
