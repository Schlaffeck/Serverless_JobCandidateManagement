using AutoMapper;
using AzureUpskill.Functions.Commands.UpdateCategory.Models;
using AzureUpskill.Models.Data;
using AzureUpskill.Models.Data.Base;
using AzureUpskill.Models.Helpers;
using System;

namespace AzureUpskill.Functions.Commands.UpdateCategory.Mapping
{
    public class UpdateCategoryInputMapper : Profile
    {
        public UpdateCategoryInputMapper()
        {
            CreateMap<UpdateCategoryInput, Category>()
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTimeOffset.Now))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => DocumentStatus.Updated));
        }
    }
}
