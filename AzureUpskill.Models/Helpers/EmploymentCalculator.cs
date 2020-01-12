using AzureUpskill.Models.Data;
using System;
using System.Collections.Generic;

namespace AzureUpskill.Models.Helpers
{
    public static class EmploymentCalculator
    {
        public static int CalculateEmploymentPeriodFullMonths(IEnumerable<EmploymentHistory> employmentHistory)
        {
            var fullMonths = 0;
            var lastDateStarted = DateTime.MinValue;
            foreach (var historyItem in employmentHistory)
            {
                var startDate = historyItem.StartDate > lastDateStarted ? historyItem.StartDate : lastDateStarted;
                var endDate = historyItem.EndDate ?? DateTime.Now;

                fullMonths += (endDate.Year - startDate.Year) * 12 + (endDate.Month - startDate.Month);
            }

            return fullMonths;
        }
    }
}
