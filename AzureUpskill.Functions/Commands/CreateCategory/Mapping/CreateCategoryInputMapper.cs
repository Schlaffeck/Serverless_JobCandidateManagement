using AutoMapper;
using AzureUpskill.Functions.Commands.CreateCategory.Models;
using AzureUpskill.Models.Data;
using AzureUpskill.Models.Data.Base;
using AzureUpskill.Models.Helpers;
using System;

namespace AzureUpskill.Functions.Commands.CreateCategory.Mapping
{
    public class CreateCategoryInputMapper : Profile
    {
        public CreateCategoryInputMapper()
        {
            CreateMap<CreateCategoryInput, Category>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => Guid.NewGuid()))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.Now))
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => DocumentStatus.New))
                .AfterMap((src, dest) => dest.CategoryId = dest.Id);
        }
    }
}
