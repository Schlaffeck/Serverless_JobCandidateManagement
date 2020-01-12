using AutoMapper;
using AzureUpskill.Models.Data;
using AzureUpskill.Models.Data.Base;
using AzureUpskill.Models.Helpers;
using System;

namespace AzureUpskill.Models.UpdateCandidate.Mapping
{
    public class UpdateCandidateInputMapper : Profile
    {
        public UpdateCandidateInputMapper()
        {
            CreateMap<UpdateCandidateInput, Candidate>()
                .ForMember(dest => dest.EmploymentFullMonths, opt => opt.MapFrom(src => EmploymentCalculator.CalculateEmploymentPeriodFullMonths(src.EmploymentHistory)))
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTimeOffset.Now))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => DocumentStatus.Updated));
        }
    }
}
