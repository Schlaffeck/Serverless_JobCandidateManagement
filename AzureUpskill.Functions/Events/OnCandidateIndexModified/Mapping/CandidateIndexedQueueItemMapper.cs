using AutoMapper;
using AzureUpskill.Functions.Events.OnCandidateIndexModified.Models;
using AzureUpskill.Models.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace AzureUpskill.Functions.Events.OnCandidateIndexModified.Mapping
{
    public class CandidateIndexedQueueItemMapper : Profile
    {
        public CandidateIndexedQueueItemMapper()
        {
            CreateMap<Candidate, CandidateIndexedQueueItem>()
                .ForMember(dest => dest.CandidateId, opt => opt.MapFrom(src => src.Id));
            CreateMap<CandidateDocument, CandidateIndexedQueueItem>()
                .IncludeBase<Candidate, CandidateIndexedQueueItem>();
        }
    }
}
