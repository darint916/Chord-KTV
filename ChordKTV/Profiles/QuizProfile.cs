using AutoMapper;
using ChordKTV.Models.Quiz;
using ChordKTV.Dtos.Quiz;

namespace ChordKTV.Profiles;

public class QuizProfile : Profile
{
    public QuizProfile()
    {
        CreateMap<Quiz, QuizResponseDto>()
            .ForMember(dest => dest.QuizId, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Questions, opt => opt.MapFrom(src => src.Questions ?? new List<QuizQuestion>()));

        CreateMap<QuizQuestion, QuizQuestionDto>()
            .ForMember(dest => dest.Options, opt => opt.MapFrom(src => src.Options.OrderBy(o => o.OrderIndex).Select(o => o.Text).ToList()))
            .ForMember(dest => dest.CorrectOptionIndex, opt => opt.MapFrom(src => src.Options.OrderBy(o => o.OrderIndex).ToList().FindIndex(o => o.IsCorrect)));
    }
}
