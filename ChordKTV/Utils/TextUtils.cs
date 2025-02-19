namespace ChordKTV.Utils;

public static class LanguageUtils
{
    public static bool IsRomanizedText(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return false; // Assume empty input is in the original language
        }

        return text.All(IsLatinCharacter);
    }

    public static bool IsLatinCharacter(char c) => c switch
    {
        >= '\u0000' and <= '\u024F' => true, // Latin scripts: ASCII, Latin-1, Extended-A & B
        _ => false
    };
}
