using System.Text.RegularExpressions;
using FuzzySharp;
namespace ChordKTV.Utils;

public static partial class CompareUtils
{
    [GeneratedRegex(@"\b\w+\b")]
    private static partial Regex FullWordRegex();

    public static bool IsCloseToF(float? a, float? b, float tolerance = 0.0001f)
    {
        if (a == null || b == null) //treat null as true for query params
        {
            return true;
        }
        return MathF.Abs(a.Value - b.Value) < tolerance;
    }

    //gets fuzzy matched score for song title and duration, order dont matter
    public static int CompareWeightedFuzzyScore(string queryTitle, string candidateTitle, string? artist, string? candidateArtist, float? queryDuration, float candidateDuration, float durationDifferenceWeight = 0.8f, float artistDifferenceWeight = 0.7f)
    {
        int fuzzyScore = Math.Max(
            Fuzz.TokenSortRatio(queryTitle.ToLowerInvariant(), candidateTitle?.ToLowerInvariant()),
            Fuzz.TokenSetRatio(queryTitle.ToLowerInvariant(), candidateTitle?.ToLowerInvariant())
        );

        float durationDifference = MathF.Abs((queryDuration ?? candidateDuration) - candidateDuration);

        int artistDifference = 0;
        if (!string.IsNullOrWhiteSpace(artist) && !string.IsNullOrWhiteSpace(candidateArtist))
        { //Note that the artist diff pretty high when partial match since it is simple ratio so artist comparison required later.
            artistDifference = 100 - Fuzz.Ratio(artist.ToLowerInvariant(), candidateArtist.ToLowerInvariant());
        }
        return (int)(fuzzyScore - (durationDifference * durationDifferenceWeight) - (artistDifference * artistDifferenceWeight));
    }

    //3 entries, queryArtist being user inputed, candidate artist is the one to test, exactArtist is the initial match in db
    public static int CompareArtistFuzzyScore(string? queryArtist, string? candidateArtist, string? exactArtist = null, int nullScore = 95, double bonusWeight = 1.6) //dont want null to beat out complete match
    {
        if (string.IsNullOrWhiteSpace(candidateArtist) || (string.IsNullOrWhiteSpace(queryArtist) && string.IsNullOrWhiteSpace(exactArtist))) //need at least two to compare including candidate
        {
            return nullScore;
        }
        queryArtist = KeywordExtractorUtils.ExtractSongKeywords(queryArtist); //gets us a solid baseline to compare to, we assume user input is always messy
        queryArtist ??= string.Empty;
        exactArtist ??= string.Empty;
        //we can have > 1 artist, sort due to artist order
        candidateArtist = candidateArtist.ToLowerInvariant();
        exactArtist = exactArtist.ToLowerInvariant();
        //query already lowercased
        int baseScore = Math.Max(Fuzz.TokenSortRatio(candidateArtist, queryArtist), Fuzz.TokenSortRatio(candidateArtist, exactArtist));

        //since we dont want partial because it causes problems like
        // "eve" and "neverland" will match 100% with partial
        // we give boosts to full word matches like "Mako & Sayuri" and "Mako"
        double bonusCount = 0;
        //split both into token set
        HashSet<string> tokens = [.. queryArtist.Split(' ', StringSplitOptions.RemoveEmptyEntries)];
        tokens.UnionWith(FullWordRegex().Matches(exactArtist).Select(m => m.Value)); //dont want bonus for symbols

        //normalize points based off words in candidate
        int candidateWordCount = candidateArtist.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length; //not gonna remove symbols since we have artists like (G)I-DLE / G.E.M.

        foreach (string token in tokens) //use the cleaner input for bonuses
        {
            if (Regex.IsMatch(candidateArtist, $@"\b{Regex.Escape(token)}\b"))
            {
                bonusCount++;
            }
        }
        return (int)(baseScore + ((101 - baseScore) * (bonusWeight * (bonusCount / candidateWordCount)))); //assume that ratio < 1 lets say, we get 1/2 full word matches around a 50% boost should work?
    }
}
