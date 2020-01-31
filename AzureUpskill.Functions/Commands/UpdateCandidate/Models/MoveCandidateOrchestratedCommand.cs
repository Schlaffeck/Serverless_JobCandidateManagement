using AzureUpskill.Models.Data;

namespace AzureUpskill.Functions.Commands.UpdateCandidate.Models
{
    public class MoveCandidateOrchestratedCommand
    {
        public CandidateDocument ExistingCandidate { get; set; }

        public UpdateCandidateInput UpdateCandidateInput { get; set; }

        public string NewCategoryId { get; set; }
    }
}
