using FuzzySharp;
namespace ChordKTV.Utils;

public static class CompareUtils
{
    public static bool IsCloseToF(float? a, float? b, float tolerance = 0.0001f)
    {
        if (a == null || b == null) //treat null as true for query params
        {
            return true;
        }

        return MathF.Abs(a.Value - b.Value) < tolerance;
    }

    //gets fuzzy matched score for song title and duration, order dont matter
    public static int CompareWeightedFuzzyScore(string queryTitle, string candidateTitle, string? artist, string? candidateArtist, float? queryDuration, float candidateDuration, float durationDifferenceWeight = .16f)
    {
        int fuzzyScore = Math.Max(
            Fuzz.TokenSortRatio(queryTitle.ToLowerInvariant(), candidateTitle?.ToLowerInvariant()),
            Fuzz.TokenSetRatio(queryTitle.ToLowerInvariant(), candidateTitle?.ToLowerInvariant())
        );

        float durationDifference = MathF.Abs((queryDuration ?? candidateDuration) - candidateDuration);

        int artistDifference = 0;
        if (!string.IsNullOrWhiteSpace(artist) && !string.IsNullOrWhiteSpace(candidateArtist))
        {
            artistDifference = 100 - Fuzz.Ratio(artist.ToLowerInvariant(), candidateArtist.ToLowerInvariant());
        }
        return (int)(fuzzyScore - (durationDifference * durationDifferenceWeight) - (artistDifference * 0.7));
    }
}
