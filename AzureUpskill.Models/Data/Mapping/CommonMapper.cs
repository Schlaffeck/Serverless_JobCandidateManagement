using AutoMapper;
using AzureUpskill.Models.Data.Base;
using AzureUpskill.Models.Helpers;
using System;

namespace AzureUpskill.Models.Data.Mapping
{
    public class CommonMapper : Profile
    {
        public CommonMapper()
        {
            CreateMap<Candidate, Candidate>();
            CreateMap<CandidateDocument, CandidateDocument>();
            CreateMap<CandidateDocument, Candidate>();

            CreateMap<Category, Candidate>()
                .ForMember(dest => dest.CategoryId, opt => opt.MapFrom(src => src.CategoryId))
                .ForAllOtherMembers(opt => opt.Ignore());
            CreateMap<CategoryDocument, Candidate>()
                .IncludeBase<Category, Candidate>();
            CreateMap<CategoryDocument, CandidateDocument>()
                .IncludeBase<Category, Candidate>();

            CreateMap<Category, Category>()
                .ForMember(dest => dest.ChangedPropertiesOldValues, opt => opt.Ignore())
                .ForMemberMapWithUpdatedChangedProperty(dest => dest.Name, src => src.Name);
        }
    }
}
