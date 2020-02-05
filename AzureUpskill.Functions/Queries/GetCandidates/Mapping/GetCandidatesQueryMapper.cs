using AutoMapper;
using AzureUpskill.Functions.Queries.GetCategories.Models;
using Microsoft.Azure.Search.Models;

namespace AzureUpskill.Functions.Queries.GetCandidates.Mapping
{
    public class GetCandidatesQueryMapper : Profile
    {
        public GetCandidatesQueryMapper()
        {
            CreateMap<GetCategoriesQuery, SearchParameters>();
        }
    }
}
