using AzureUpskill.Models.Data.Base;

namespace AzureUpskill.Functions.Events.OnCandidateIndexModified.Models
{
    public class CandidateIndexedQueueItem
    {
        public string CategoryId { get; set; }

        public string CandidateId { get; set; }

        public DocumentStatus Status { get; set; }
    }
}
