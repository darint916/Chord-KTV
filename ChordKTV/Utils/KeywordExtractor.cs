using System.Text.RegularExpressions;
using System.Globalization;

namespace ChordKTV.Utils;

public partial class KeywordExtractor
{
    private static readonly HashSet<string> _stopWords = new(StringComparer.OrdinalIgnoreCase)
    {
        "official", "video", "music", "remastered", "live", "ft.", "feat.", "remix",
        "version", "extended", "edit", "audio", "lyric", "cover", "performance",
        "mix", "explicit", "radio", "instrumental"
    };

    [GeneratedRegex(@"[^\w\s]")]
    private static partial Regex SpecialCharsRegex();

    [GeneratedRegex(@"\s+")]
    private static partial Regex ExtraWhiteSpaceRegex();

    public static string? ExtractSongKeywords(params string?[] input)
    {
        string combinedInput = string.Join(" ", input.Where(x => !string.IsNullOrWhiteSpace(x)));
        if (string.IsNullOrWhiteSpace(combinedInput))
        {
            return null;
        }

        string cleanedText = combinedInput.ToLower(CultureInfo.InvariantCulture);

        // Remove special characters like () [] {} & punctuation then eat up the double whitespaces so we get nice array
        cleanedText = SpecialCharsRegex().Replace(cleanedText, " ");
        cleanedText = ExtraWhiteSpaceRegex().Replace(cleanedText, " ").Trim();

        // filter out stop words, TODO: maybe later consider the words after these stop words could be removed?
        var words = cleanedText.Split(' ')
                               .Where(word => !_stopWords.Contains(word))
                               .ToList();

        return string.Join(" ", words);
    }

}

