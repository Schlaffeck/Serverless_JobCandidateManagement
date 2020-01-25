using AzureUpskill.Models.Data;
using AzureUpskill.Models.UpdateCandidate;

namespace AzureUpskill.Models.Commands.UpdateCandidate
{
    public class MoveCandidateOrchestratedCommand
    {
        public CandidateDocument ExistingCandidate { get; set; }

        public UpdateCandidateInput UpdateCandidateInput { get; set; }

        public string NewCategoryId { get; set; }
    }
}
