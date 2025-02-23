using System.Text.RegularExpressions;
using System.Globalization;

namespace ChordKTV.Utils;

//partial cuz of stinky regex https://stackoverflow.com/a/78951626
public partial class KeywordExtractorUtils
{
    private static readonly HashSet<string> _stopWords = new(StringComparer.OrdinalIgnoreCase)
    {
        "official", "video", "music", "remastered", "live", "ft.", "feat.", "remix",
        "version", "extended", "edit", "audio", "lyric", "cover", "performance",
        "mix", "explicit", "radio", "instrumental", "tv", "lyrics", "on vocal", "vocal", "off vocal", "karaoke", "ktv",
        "op", "opening", "ending", "english", "japanese", "full", "hd", "hq", "4k", "8k", "1080p", "720p", "480p", "360p",
        "jpn", "romaji", "eng", "engsub", "eng sub", "eng dub", "engdub", "sub", "dub", "subbed", "dubbed", "subtitles",
        "amv", "indonesia", "subtitle", "by"
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

        string cleanedText = combinedInput.ToLowerInvariant();

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

