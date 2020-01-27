using AutoMapper;
using AzureUpskill.Models.Data;
using AzureUpskill.Search.Models.Candidates;

namespace AzureUpskill.Search.Mapping
{
    public class CandidateIndexMapper : Profile
    {
        public CandidateIndexMapper()
        {
            CreateMap<Address, AddressIndex>();

            CreateMap<EmploymentHistory, EmploymentHistoryIndex>();

            CreateMap<EducationHistory, EducationHistoryIndex>();

            CreateMap<ContactDetails, ContactDetailsIndex>();

            CreateMap<Skill, SkillIndex>();

            CreateMap<Category, CandidateIndex>()
                .ForMember(dest => dest.CategoryId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Name));

            CreateMap<Candidate, CandidateIndex>()
                .ForMember(dest => dest.EmploymentFullYears, opt => opt.MapFrom(src => src.EmploymentFullMonths / 12))
                .ForMember(dest => dest.CategoryName, opt => opt.Ignore());

            CreateMap<CandidateDocument, CandidateIndex>()
                .IncludeBase<Candidate, CandidateIndex>();
        }
    }
}
