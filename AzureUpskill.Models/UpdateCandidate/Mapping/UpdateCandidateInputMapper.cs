using AutoMapper;
using AzureUpskill.Models.Helpers;
using System;

namespace AzureUpskill.Models.UpdateCandidate.Mapping
{
    public class UpdateCandidateInputMapper : Profile
    {
        public UpdateCandidateInputMapper()
        {
            CreateMap<UpdateCandidateInput, Candidate>()
                .ForMember(dest => dest.EmploymentFullMonths, opt => opt.MapFrom(src => EmploymentCalculator.CalculateEmploymentPeriodFullMonths(src.EmploymentHistory)));

            CreateMap<Category, Candidate>()
                .ForMember(dest => dest.CategoryId, opt => opt.MapFrom(dest => dest.CategoryId))
                .ForAllOtherMembers(opt => opt.Ignore());
        }
    }
}
