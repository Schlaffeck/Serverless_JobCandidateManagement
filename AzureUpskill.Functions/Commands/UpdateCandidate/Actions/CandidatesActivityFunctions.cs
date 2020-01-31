using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using AzureUpskill.Core;
using AzureUpskill.Functions.Commands.UpdateCandidate.Models;
using AzureUpskill.Functions.Helpers.CosmosDb;
using AzureUpskill.Helpers;
using AzureUpskill.Models.Data;
using AzureUpskill.Models.Data.Base;
using FluentValidation.Results;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;

namespace AzureUpskill.Functions.Commands.UpdateCandidate.Actions
{
    public class CandidatesActivityFunctions
    {
        private readonly IMapper _mapper;

        public CandidatesActivityFunctions(IMapper mapper)
        {
            _mapper = mapper;
        }

        [FunctionName(Names.CandidateDeleteActivityFunctionName)]
        public async Task<Result<bool>> CandidateDeleteActivity(
            [ActivityTrigger] CandidateDocument candidateDocument,
            [CosmosDB(
                Consts.CosmosDb.DbName,
                Consts.CosmosDb.CandidatesContainerName,
                ConnectionStringSetting = Consts.CosmosDb.ConnectionStringName)] DocumentClient candidatesDocumentClient,
            ILogger log)
        {
            if (candidateDocument is null)
            {
                var msg = $"Candidate not found";
                log.LogWarningEx(msg);
                return new Result<bool>(msg);
            }

            var result = await candidateDocument.MarkCandidateForDeletionAsync(candidatesDocumentClient);

            if (result.StatusCode.IsSuccess())
            {
                log.LogInformationEx($"Candidate {candidateDocument.Id} deleted from category: {candidateDocument.CategoryId}");
                return new Result<bool>(true);
            }

            var resMsg = $"Candidate {candidateDocument.Id} could not be deleted from category: {candidateDocument.CategoryId}," +
                           $" message: {result.ToErrorString()}";
            log.LogErrorEx(resMsg);
            return new Result<bool>(resMsg);
        }

        [FunctionName(Names.CandidateCreateInNewCategoryActivityFunctionName)]
        public async Task<Result<Candidate>> CandidateCreateInNewCategoryActivity(
            [ActivityTrigger] CreateCandidateInNewCategoryCommand createCandidateInNewCategoryCommand,
            [CosmosDB(
                Consts.CosmosDb.DbName,
                Consts.CosmosDb.CandidatesContainerName,
                ConnectionStringSetting = Consts.CosmosDb.ConnectionStringName)]
            DocumentClient candidatesDocumentClient,
            ILogger log)
        {
            if (createCandidateInNewCategoryCommand.NewCategory == null)
            {
                var msg = $"New category for candidate not found";
                log.LogErrorEx(msg);
                return new Result<Candidate>(msg);
            }

            if (createCandidateInNewCategoryCommand.ExistingCandidate is null)
            {
                var msg = $"Candidate not found";
                log.LogWarningEx(msg);
                return new Result<Candidate>(msg);
            }

            var candidate = _mapper.Map<Candidate>(createCandidateInNewCategoryCommand.ExistingCandidate);
            _mapper.Map(createCandidateInNewCategoryCommand.UpdateCandidateInput, candidate);
            _mapper.Map(createCandidateInNewCategoryCommand.NewCategory, candidate);
            candidate.Status = DocumentStatus.Moved;
            var docCollectionUri = UriFactory.CreateDocumentCollectionUri(
                Consts.CosmosDb.DbName,
                Consts.CosmosDb.CandidatesContainerName);
            var result = await candidatesDocumentClient.CreateDocumentAsync(
                docCollectionUri,
                candidate,
                new RequestOptions { PartitionKey = new PartitionKey(createCandidateInNewCategoryCommand.NewCategory.PartitionKey) });

            if (result.StatusCode.IsSuccess())
            {
                return new Result<Candidate>(candidate);
            }

            var resultMsg = $"Candidate {candidate.Id} could not be created in category: {candidate.CategoryId}, " +
                           $"message: {result.ToErrorString()}";
            log.LogErrorEx(resultMsg);
            return new Result<Candidate>(resultMsg);
        }


        [FunctionName(Names.CategoryGetActivityFunctionName)]
        public async Task<Result<CategoryDocument>> CategoryGetNewActivity(
            [ActivityTrigger] string categoryId,
            [CosmosDB(
                Consts.CosmosDb.DbName,
                Consts.CosmosDb.CategoriesContainerName,
                ConnectionStringSetting = Consts.CosmosDb.ConnectionStringName)] DocumentClient categoriesContainerName,
            ILogger log)
        {
            if (string.IsNullOrEmpty(categoryId))
            {
                var exMsg = $"Category id not provided";
                log.LogWarningEx(exMsg);
                throw new ArgumentNullException(nameof(categoryId), exMsg);
            }

            var docUri = UriFactory.CreateDocumentUri(Consts.CosmosDb.DbName, Consts.CosmosDb.CategoriesContainerName,
                categoryId);
            var result = await categoriesContainerName.ReadDocumentAsync<CategoryDocument>(docUri, new RequestOptions { PartitionKey = new PartitionKey(categoryId) });

            if (result.StatusCode.IsSuccess())
            {
                log.LogInformationEx($"Category with id '{categoryId}' found");
                return new Result<CategoryDocument>(result.Document);
            }

            var msg = $"Could not find category with id '{categoryId}'," +
                           $" message: {result.ToErrorString()}";
            log.LogErrorEx(msg);
            return new Result<CategoryDocument>(msg);
        }

        [FunctionName(Names.CandidateUpdateActivityFunctionName)]
        public async Task<Result<Candidate>> UpdateCandidateActivity(
            [ActivityTrigger] UpdateCandidateOrchestratedCommand updateCandidateCommand,
            [CosmosDB(
                Consts.CosmosDb.DbName,
                Consts.CosmosDb.CandidatesContainerName,
                ConnectionStringSetting = Consts.CosmosDb.ConnectionStringName)]
            DocumentClient candidatesDocumentClient,
            ILogger log)
        {
            if (updateCandidateCommand.CurrentCandidate is null)
            {
                return new Result<Candidate>("No candidate provided");
            }

            try
            {
                log.LogInformationEx($"Updating candidate with id: {updateCandidateCommand.CurrentCandidate.PartitionKey}/{updateCandidateCommand.CurrentCandidate.Id}");
                var candidate = _mapper.Map(updateCandidateCommand.UpdateCandidateInput, (Candidate)updateCandidateCommand.CurrentCandidate);
                var result = await candidatesDocumentClient.ReplaceDocumentAsync(
                    updateCandidateCommand.CurrentCandidate.SelfLink,
                    candidate,
                    new RequestOptions { PartitionKey = new PartitionKey(candidate.CategoryId) });

                if (result.StatusCode.IsSuccess())
                {
                    return new Result<Candidate>(candidate);
                }

                return new Result<Candidate>(result.ToErrorString());
            }
            catch (DocumentClientException dce)
            {
                var message = $"Candidate update failed: {dce.Message}";
                log.LogErrorEx(dce, message);
                return new Result<Candidate>(message);
            }
        }

        [FunctionName(Names.CandidateMoveOrchestratedFunctionName)]
        public async Task<Result<Candidate>> MoveCandidateOrchestrated(
             [OrchestrationTrigger] IDurableOrchestrationContext orchestrationContext,
             ILogger log)
        {
            try
            {
                log.LogInformationEx($"Move candidate - Orchestration started");
                var moveCandidateCommand = orchestrationContext.GetInput<MoveCandidateOrchestratedCommand>();
                var newCategoryResult = await orchestrationContext.CallActivityAsync<Result<CategoryDocument>>(
                    Names.CategoryGetActivityFunctionName,
                    moveCandidateCommand.NewCategoryId);
                if (!newCategoryResult.IsValid)
                {
                    return new Result<Candidate>(newCategoryResult.Errors);
                }

                var createCandidateInNewCategoryCommand =
                    _mapper.Map<CreateCandidateInNewCategoryCommand>(moveCandidateCommand);
                _mapper.Map(newCategoryResult.Value, createCandidateInNewCategoryCommand);
                var newCandidateResult = await orchestrationContext.CallActivityAsync<Result<Candidate>>(
                    Names.CandidateCreateInNewCategoryActivityFunctionName,
                    createCandidateInNewCategoryCommand);
                if (!newCandidateResult.IsValid)
                {
                    return new Result<Candidate>(newCandidateResult.Errors);
                }

                var deleteCandidateResult = await orchestrationContext.CallActivityAsync<Result<bool>>(Names.CandidateDeleteActivityFunctionName, moveCandidateCommand.ExistingCandidate);
                if (!deleteCandidateResult.IsValid
                    || !deleteCandidateResult.Value)
                {
                    return new Result<Candidate>(
                        deleteCandidateResult.Errors
                            .Concat(new[] { new ValidationFailure(string.Empty, "Could not delete previous candidate") }));
                }

                return newCandidateResult;
            }
            catch (Exception e)
            {
                log.LogErrorEx(e, "Move candidate - Orchestration failed");
                throw;
            }
            finally
            {
                log.LogInformationEx($"Move candidate - Orchestration Finished");
            }
        }

        [FunctionName(Names.CandidateUpdateOrchestratedFunctionName)]
        public async Task<Result<Candidate>> UpdateCandidateOrchestrated(
             [OrchestrationTrigger] IDurableOrchestrationContext orchestrationContext,
             ILogger log)
        {
            try
            {
                log.LogInformationEx($"Update candidate - Orchestration started");
                var updateCandidateCommand = orchestrationContext.GetInput<UpdateCandidateOrchestratedCommand>();
                var updateCandidateResult = new Result<Candidate>();

                if (updateCandidateCommand.CurrentCandidate.CategoryId != updateCandidateCommand.UpdateCandidateInput.CategoryId)
                {
                    var moveCandidateCommand = new MoveCandidateOrchestratedCommand
                    {
                        ExistingCandidate = updateCandidateCommand.CurrentCandidate,
                        UpdateCandidateInput = updateCandidateCommand.UpdateCandidateInput,
                        NewCategoryId = updateCandidateCommand.UpdateCandidateInput.CategoryId
                    };
                    updateCandidateResult = await orchestrationContext.CallSubOrchestratorAsync<Result<Candidate>>(
                        Names.CandidateMoveOrchestratedFunctionName,
                        moveCandidateCommand);
                }
                else
                {
                    updateCandidateResult = await orchestrationContext.CallActivityAsync<Result<Candidate>>(
                        Names.CandidateUpdateActivityFunctionName,
                        updateCandidateCommand);
                }

                return updateCandidateResult;
            }
            catch (Exception e)
            {
                log.LogErrorEx(e, "Move candidate - Orchestration failed");
                throw;
            }
            finally
            {
                log.LogInformationEx($"Move candidate - Orchestration Finished");
            }
        }

        public static class Names
        {
            public const string CandidateMoveOrchestratedFunctionName = "Candidate_Move_Orchestrated";
            public const string CandidateUpdateOrchestratedFunctionName = "Candidate_Update_Orchestrated";

            public const string CategoryGetActivityFunctionName = "Category_Get_Activity";
            public const string CandidateDeleteActivityFunctionName = "Candidate_Delete_Activity";
            public const string CandidateUpdateActivityFunctionName = "Candidate_Update_Activity";
            public const string CandidateCreateInNewCategoryActivityFunctionName = "Candidate_MoveToNewCategory_Activity";
        }
    }
}