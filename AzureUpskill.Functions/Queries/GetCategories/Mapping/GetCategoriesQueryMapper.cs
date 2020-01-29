using AutoMapper;
using AzureUpskill.Functions.Queries.GetCategories.Models;
using Microsoft.Azure.Search.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace AzureUpskill.Functions.Queries.GetCategories.Mapping
{
    public class GetCategoriesQueryMapper : Profile
    {
        public GetCategoriesQueryMapper()
        {
            CreateMap<GetCategoriesQuery, SearchParameters>();
        }
    }
}
