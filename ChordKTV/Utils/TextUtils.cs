namespace ChordKTV.Utils;

using System.Text.RegularExpressions;

public static partial class LanguageUtils
{
    [GeneratedRegex(@"[^\p{L}\p{M}\p{N}]")]
    private static partial Regex PureText();

    public static bool IsRomanizedText(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return false; // Assume empty input is in the original language
        }
        // clean out punctuation
        text = PureText().Replace(text, "");
        return text.All(IsLatinCharacter);
    }

    public static bool IsLatinCharacter(char c) => c switch //Todo maybe can replace with regex in future
    {
        >= '\u0000' and <= '\u024F' => true, // Latin scripts: ASCII, Latin-1, Extended-A & B
        >= '\u1E00' and <= '\u1EFF' => true, //Latin Extended Additional
        >= '\u02B0' and <= '\u02FF' => true, //assume these are special spacing chars
        >= '\u0300' and <= '\u036F' => true, //combining marks
        //assume further is greek
        _ => false
    };
}
