using AutoMapper;
using ChordKTV.Models.Quiz;
using ChordKTV.Dtos.Quiz;
using System.Globalization;

namespace ChordKTV.Profiles;

public class QuizProfile : Profile
{
    public QuizProfile()
    {
        CreateMap<Quiz, QuizResponseDto>()
            .ForMember(dest => dest.QuizId, opt => opt.MapFrom(src => src.Id));

        CreateMap<QuizQuestion, QuizQuestionDto>()
            .ForMember(dest => dest.StartTimestamp, opt => opt.MapFrom(src => src.StartTimestamp.HasValue ? src.StartTimestamp.Value.ToString(@"hh\:mm\:ss\.fffffff", CultureInfo.InvariantCulture) : null))
            .ForMember(dest => dest.EndTimestamp, opt => opt.MapFrom(src => src.EndTimestamp.HasValue ? src.EndTimestamp.Value.ToString(@"hh\:mm\:ss\.fffffff", CultureInfo.InvariantCulture) : null))
            .ForMember(dest => dest.Options, opt => opt.MapFrom(src => src.Options.OrderBy(o => o.OrderIndex).Select(o => o.Text)))
            .ForMember(dest => dest.CorrectOptionIndex, opt => opt.MapFrom(src => src.Options.FindIndex(o => o.IsCorrect)));
    }
}
