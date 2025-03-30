namespace ChordKTV.Dtos;

public class MatchScores
{
    public bool LrcExactMatch { get; set; }
    public int? LrcRomanizedScore { get; set; } //if we find romanized version first, compare to og match
    public int? LrcOriginalScore { get; set; } //og match vs if we find romanized version first
    public int? LrcInputParamScore { get; set; } //score compared against function call supplied args
}
