using ChordKTV.Dtos;
using ChordKTV.Dtos.OpenAI;
using ChordKTV.Dtos.TranslationGptApi;
using ChordKTV.Models.Quiz;

namespace ChordKTV.Services.Api;

public interface IChatGptService
{
    public Task<TranslationResponseDto> TranslateLyricsAsync(string originalLyrics, LanguageCode languageCode, bool romanize, bool translate);
    public Task<List<TranslationResponseDto>> BatchTranslateLyricsAsync(List<TranslationRequestDto> lrcLyrics);
    public Task<Quiz> GenerateRomanizationQuizAsync(string lyrics, int difficulty, int numQuestions, Guid songId);
    public Task<CandidateSongInfoListResponse> GetCandidateSongInfosAsync(string videoTitle, string channelName);
    public Task<List<string>> GenerateAudioQuizDistractorsAsync(string correctLyric, int difficulty);
    public Task<TranslatePhrasesResponseDto> TranslateRomanizeAsync(string[] phrases, LanguageCode languageCode, int difficulty = 3);
}
