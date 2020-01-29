using AutoMapper;
using AzureUpskill.Models.Data;
using AzureUpskill.Search.Models.Categories;
using System;
using System.Collections.Generic;
using System.Text;

namespace AzureUpskill.Search.Mapping
{
    public class CategoryIndexMapper : Profile
    {
        public CategoryIndexMapper()
        {
            CreateMap<Category, CategoryIndex>();

            CreateMap<CategoryDocument, CategoryIndex>()
                .IncludeBase<Category, CategoryIndex>();
        }
    }
}
