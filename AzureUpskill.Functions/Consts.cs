namespace AzureUpskill.Functions
{
    public static class Consts
    {
        public static class CosmosDb
        {
            public const string DbName = "CvDatabase";
            public const string CategoriesContainerName = "Categories";
            public const string CandidatesContainerName = "Candidates";
            public const string ConnectionStringName = "CosmosDbConnection";
            public const string LeasesContainerName = "leases";
        }

        public static class Storage
        {
            public const string ConnectionStringName = "FilesStorageConnection";
            public const string CandidatesDocumentsBlobContainerName = "candidates-documents";
            public const string CandidatesPicturesBlobContainerName = "candidates-pictures";
        }

        public static class Queues
        {
            public const string CandidatesIndexedQueueName = "CandidatesIndexedQueue";
            public const string ConnectionStringName = "QueuesStorageConnection";
        }

        public static class Notifications 
        {
            public const string SignalRConnectionStringName = "SignalRConnection";
            public const string CandidateCreatedNotificationHubName = "CandidateCreatedHub";
        }
    }
}
