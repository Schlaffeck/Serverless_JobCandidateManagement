using AutoMapper;
using AzureUpskill.Models.Data.Base;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace AzureUpskill.Models.Helpers
{
    public static class ChangesDescribingModelExtensions
    {
        public static bool IsNew(this IDocumentStatusInfo modelBase)
        {
            return modelBase.UpdatedAt is null || modelBase.UpdatedAt == modelBase.CreatedAt;
        }

        public static bool IsUpdated(this IDocumentStatusInfo modelBase)
        {
            return modelBase.UpdatedAt.HasValue && modelBase.UpdatedAt != modelBase.CreatedAt;
        }
    }
}
