using System;
using AutoMapper;
using AzureUpskill.Functions.Commands.UpdateCandidate.Models;
using AzureUpskill.Models.Data;
using AzureUpskill.Models.Data.Base;

namespace AzureUpskill.Functions.Commands.UpdateCandidate.Mapping
{
    public class CreateCandidateInNewCategoryCommandMapper : Profile
    {
        public CreateCandidateInNewCategoryCommandMapper()
        {
            CreateMap<MoveCandidateOrchestratedCommand, CreateCandidateInNewCategoryCommand>()
                .ForMember(dest => dest.NewCategory, opt => opt.Ignore());

            CreateMap<CategoryDocument, CreateCandidateInNewCategoryCommand>()
                .AfterMap((src, dest) => { dest.NewCategory = src; })
                .ForAllOtherMembers(opt => opt.Ignore());

            CreateMap<Category, CreateCandidateInNewCategoryCommand>()
                .ForMember(dest => dest.NewCategory, opt => opt.MapFrom(src => src))
                .ForAllOtherMembers(opt => opt.Ignore());

            //CreateMap<CreateCandidateInNewCategoryCommand, Candidate>()
            //    .ForMember(dest => dest, opt => opt.MapFrom(src => src.ExistingCandidate))
            //    .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.Now))
            //    .ForMember(dest => dest.Status, opt => opt.MapFrom(src => DocumentStatus.Moved))
            //    .AfterMap((src, dest, ctx) => {
            //        ctx.Mapper.Map(src.UpdateCandidateInput, dest);
            //        ctx.Mapper.Map(src.NewCategory, dest);
            //    });
        }
    }
}
