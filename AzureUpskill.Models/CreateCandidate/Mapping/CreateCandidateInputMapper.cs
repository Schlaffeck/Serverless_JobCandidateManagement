using AutoMapper;
using AzureUpskill.Models.Helpers;
using System;

namespace AzureUpskill.Models.CreateCandidate.Mapping
{
    public class CreateCandidateInputMapper : Profile
    {
        public CreateCandidateInputMapper()
        {
            CreateMap<CreateCandidateInput, Candidate>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => Guid.NewGuid()))
                .ForMember(dest => dest.EmploymentFullMonths, opt => opt.MapFrom(src => EmploymentCalculator.CalculateEmploymentPeriodFullMonths(src.EmploymentHistory)));

            CreateMap<Category, Candidate>()
                .ForMember(dest => dest.CategoryId, opt => opt.MapFrom(dest => dest.CategoryId))
                .ForAllOtherMembers(opt => opt.Ignore());
        }
    }
}
