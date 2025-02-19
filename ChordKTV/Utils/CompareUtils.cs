using FuzzySharp;
namespace ChordKTV.Utils;

public static class CompareUtils
{
    public static double CalculateStringSimilarity(string string1, string string2)
    {
        return Fuzz.Ratio(string1, string2); // Returns similarity score (0-100)
    }
}