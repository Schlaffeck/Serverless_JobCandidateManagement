using AutoMapper;
using AzureUpskill.Models.Data.Base;
using System;
using System.Linq.Expressions;

namespace AzureUpskill.Models.Helpers
{
    public static class ChangesDescribingModelExtensions
    {
        public static bool IsNew(this IDocumentStatusInfo modelBase)
        {
            return modelBase.UpdatedAt is null || modelBase.UpdatedAt == modelBase.CreatedAt;
        }

        public static bool IsNewOrMoved(this IDocumentStatusInfo modelBase)
        {
            return modelBase.IsNew() || modelBase.Status == DocumentStatus.Moved;
        }

        public static bool IsUpdated(this IDocumentStatusInfo modelBase)
        {
            return modelBase.UpdatedAt.HasValue && modelBase.UpdatedAt != modelBase.CreatedAt;
        }

        public static bool HasChangedProperty(this IDocumentWithChangesHistory modelBase, string propertyName)
        {
            if (modelBase.IsUpdated()
                && modelBase.ChangedPropertiesOldValues.ContainsKey(propertyName))
            {
                return true;
            }

            return false;
        }

        public static bool HasChangedProperty(this IDocumentWithChangesHistory modelBase, string propertyName, out object oldPropertyValue)
        {
            oldPropertyValue = null;
            if(modelBase.IsUpdated()
                && modelBase.ChangedPropertiesOldValues.TryGetValue(propertyName, out oldPropertyValue))
            {
                return true;
            }

            return false;
        }

        public static IMappingExpression<TSource, TDestination> ForMemberMapWithUpdatedChangedProperty<TSource, TDestination, TValue>(
            this IMappingExpression<TSource, TDestination> mappingExpression,
            Expression<Func<TDestination, TValue>> getFromDestFunction, 
            Func<TSource, TValue> getFromSourceFunc)
            where TDestination : IDocumentWithChangesHistory
        {
            mappingExpression
                .ForMember(getFromDestFunction, opt => opt.MapFrom((source, dest, member, ctx) => {

                    var newValue = getFromSourceFunc(source);
                    if (!dest.IsNew())
                    {
                        var oldValue = getFromDestFunction.Compile().Invoke(dest);
                        if (!object.Equals(oldValue, newValue))
                        {
                            var memberName = (getFromDestFunction.Body as MemberExpression).Member.Name;
                            if (dest.ChangedPropertiesOldValues.ContainsKey(memberName))
                            {
                                dest.ChangedPropertiesOldValues[memberName] = oldValue;
                            }
                            else
                            {
                                dest.ChangedPropertiesOldValues.Add(memberName, oldValue);
                            }
                        }
                    }
                    return newValue;
                }));

            return mappingExpression;
        }
    }
}
