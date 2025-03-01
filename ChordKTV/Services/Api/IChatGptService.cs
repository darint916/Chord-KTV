using ChordKTV.Dtos;
using ChordKTV.Dtos.TranslationGptApi;
using ChordKTV.Dtos.Quiz;

namespace ChordKTV.Services.Api;

public interface IChatGptService
{
    public Task<TranslationResponseDto> TranslateLyricsAsync(string originalLyrics, LanguageCode languageCode, bool romanize, bool translate);
    public Task<List<TranslationResponseDto>> BatchTranslateLyricsAsync(List<TranslationRequestDto> lrcLyrics);
    public Task<QuizResponseDto> GenerateRomanizationQuizAsync(string lyrics, int difficulty, int numQuestions, Guid songId);
}
