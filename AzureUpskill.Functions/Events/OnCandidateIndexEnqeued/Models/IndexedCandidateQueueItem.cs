using AzureUpskill.Models.Data.Base;

namespace AzureUpskill.Functions.Events.OnCandidateIndexEnqeued.Models
{
    public class IndexedCandidateQueueItem
    {
        public string CategoryId { get; set; }

        public string  CandidateId { get; set; }

        public DocumentStatus Status { get; set; }
    }
}
