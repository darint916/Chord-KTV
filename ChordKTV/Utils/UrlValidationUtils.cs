using System.Text.RegularExpressions;

namespace ChordKTV.Utils;

public partial class UrlValidationUtils
{
    [GeneratedRegex(@"^(https?:\/\/)?([\w\-]+\.)+([a-z]{2,})(\/[\w\-?=&%\.]*)?$", RegexOptions.IgnoreCase)]
    public static partial Regex PlaylistUrlRegex();
    [GeneratedRegex(@"[?&]list=([^&]+)", RegexOptions.IgnoreCase)]
    public static partial Regex PlaylistIdRegex();

    public static string? ExtractPlaylistId(string url)
    {
        Match m = PlaylistIdRegex().Match(url);
        return m.Success ? m.Groups[1].Value : null;
    }
}
