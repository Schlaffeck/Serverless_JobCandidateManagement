using AzureUpskill.Models.Data;
using AzureUpskill.Models.UpdateCandidate;

namespace AzureUpskill.Models.Commands.UpdateCandidate
{
    public class UpdateCandidateOrchestratedCommand
    {
        public UpdateCandidateInput UpdateCandidateInput { get; set; }

        public CandidateDocument CurrentCandidate { get; set; }
    }
}
