using AzureUpskill.Models.Data;

namespace AzureUpskill.Functions.Commands.UpdateCandidate.Models
{
    public class UpdateCandidateOrchestratedCommand
    {
        public UpdateCandidateInput UpdateCandidateInput { get; set; }

        public CandidateDocument CurrentCandidate { get; set; }
    }
}
