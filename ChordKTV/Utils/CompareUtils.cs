using FuzzySharp;
namespace ChordKTV.Utils;

public static class CompareUtils
{
    public static bool IsCloseToF(float? a, float? b, float tolerance = 0.0001f)
    {
        if (a == null || b == null)
        {
            return false;
        }

        return MathF.Abs(a.Value - b.Value) < tolerance;
    }

    //gets fuzzy matched score for song title and duration, order dont matter
    public static int CompareWeightedFuzzyScore(string queryTitle, string candidateTitle, float? queryDuration, float candidateDuration, float durationDifferenceWeight = .16f)
    {
        int fuzzyScore = Math.Max(
            Fuzz.TokenSortRatio(queryTitle.ToLowerInvariant(), candidateTitle?.ToLowerInvariant()),
            Fuzz.TokenSetRatio(queryTitle.ToLowerInvariant(), candidateTitle?.ToLowerInvariant())
        );

        float durationDifference = MathF.Abs((queryDuration ?? candidateDuration) - candidateDuration);

        return (int)(fuzzyScore - (durationDifference * durationDifferenceWeight));
    }
}
