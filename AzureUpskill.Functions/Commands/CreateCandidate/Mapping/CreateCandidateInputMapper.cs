using AutoMapper;
using AzureUpskill.Functions.Commands.CreateCandidate.Models;
using AzureUpskill.Models.Data;
using AzureUpskill.Models.Helpers;
using System;

namespace AzureUpskill.Functions.Commands.CreateCandidate.Mapping
{
    public class CreateCandidateInputMapper : Profile
    {
        public CreateCandidateInputMapper()
        {
            CreateMap<CreateCandidateInput, Candidate>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => Guid.NewGuid()))
                .ForMember(dest => dest.EmploymentFullMonths, opt => opt.MapFrom(src => EmploymentCalculator.CalculateEmploymentPeriodFullMonths(src.EmploymentHistory)))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTimeOffset.Now));

            CreateMap<Category, Candidate>()
                .ForMember(dest => dest.CategoryId, opt => opt.MapFrom(dest => dest.CategoryId))
                .ForAllOtherMembers(opt => opt.Ignore());
        }
    }
}
