using System.Text.RegularExpressions;

namespace ChordKTV.Utils;

public partial class UrlValidationUtils
{
    [GeneratedRegex(@"^(https?:\/\/)?([\w\-]+\.)+([a-z]{2,})(\/[\w\-?=&%\.]*)?$", RegexOptions.IgnoreCase)]
    public static partial Regex PlaylistUrlRegex();
}
