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

            CreateMap<Category, Candidate>()
                .ForMemberMapWithUpdatedChangedProperty(dest => dest.CategoryId, src => src.CategoryId)
                .ForAllOtherMembers(opt => opt.Ignore());
        }
    }
}
