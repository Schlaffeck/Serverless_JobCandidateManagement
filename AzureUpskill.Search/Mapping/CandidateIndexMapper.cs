using AutoMapper;
using AzureUpskill.Models.Data;
using AzureUpskill.Search.Models.Candidates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AzureUpskill.Search.Mapping
{
    public class CandidateIndexMapper : Profile
    {
        public CandidateIndexMapper()
        {
            CreateMap<Address, AddressIndex>();

            CreateMap<EmploymentHistory, EmploymentHistoryIndex>();

            CreateMap<EducationHistory, EducationHistoryIndex>();

            CreateMap<Category, CandidateIndex>()
                .ForMember(dest => dest.CategoryId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Name));

            CreateMap<Candidate, CandidateIndex>()
                .ForMember(dest => dest.CategoryName, opt => opt.Ignore());
        }
    }
}
