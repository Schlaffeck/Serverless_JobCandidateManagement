namespace AzureUpskill.Functions.Commands.SubscribeToNewCandidateInCategory.Models
{
    public class SubscribeToNewCandidateInCategoryInput
    {
        public string UserId { get; set; }

        public string[]  CategoryIds { get; set; }
    }
}
