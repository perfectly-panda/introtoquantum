using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Azure.Quantum;
using Azure.Identity;
using Azure.Quantum.Jobs;
using Azure.Quantum.Jobs.Models;
using Azure.Storage.Blobs;
using Microsoft.Azure.Cosmos.Table;
using System.Linq;

namespace demo
{
    public static class CreateJob
    {
        [FunctionName("CreateJob")]
        public static async Task Run([TimerTrigger("0 0 1 * * *")] TimerInfo myTimer,
            [Table("jobs")] CloudTable jobs,
            [Table("numbers")] CloudTable numbers,
            [Blob("rawfiles/inputData", FileAccess.Read)] Stream inputData,
            [Blob("rawfiles/mappingData", FileAccess.Read)] Stream mappingData,
            [Blob("rawfiles/translatedInputData", FileAccess.Read)] Stream translatedInputData,
            ILogger log)
        {
            log.LogInformation($"CreateJob function started {DateTime.Now}");

            //Is there a job already running?
            var filter = TableQuery.GenerateFilterCondition("status", QueryComparisons.Equal, "waiting");

            var activeJob = (await jobs.ExecuteQuerySegmentedAsync(new TableQuery<Jobs>().Where(filter).Take(1), null)).FirstOrDefault();

            var filter2 = TableQuery.GenerateFilterCondition("State", QueryComparisons.Equal, "new");

            var availableNumbers = (await numbers.ExecuteQuerySegmentedAsync(new TableQuery<Number>().Where(filter), null)).Count();

            if (activeJob == null && availableNumbers <= 3)
            {

                var client =
                    new QuantumJobClient(
                        Environment.GetEnvironmentVariable("subId"),
                        Environment.GetEnvironmentVariable("resourceGroup"),
                        Environment.GetEnvironmentVariable("workspace"),
                         Environment.GetEnvironmentVariable("location"),
                        new DefaultAzureCredential());


                var jobId = Guid.NewGuid().ToString().Replace("-", "");

                var container = new BlobContainerClient(Environment.GetEnvironmentVariable("quantumStorageConn"), jobId);
                container.Create();

                BlobClient blob = container.GetBlobClient("inputData");
                await blob.UploadAsync(inputData);
                blob = container.GetBlobClient("mappingData");
                await blob.UploadAsync(mappingData);
                blob = container.GetBlobClient("translatedInputData");
                await blob.UploadAsync(translatedInputData);

                var job = new JobDetails($"{Environment.GetEnvironmentVariable("quantumStorage")}/{jobId}",
                    "microsoft.ionq-ir.v2",
                    "IonQ",
                    Environment.GetEnvironmentVariable("target"));
                //job.InputParams = "{ 'shots': 50 }"; //https://github.com/Azure/azure-sdk-for-net/issues/24580

                job.Metadata.Add("entryPointInput", "8");
                job.Metadata.Add("outputMappingBlobUri", $"{Environment.GetEnvironmentVariable("quantumStorage")}/{jobId}/mappingData");
                job.InputDataUri = $"{Environment.GetEnvironmentVariable("quantumStorage")}/{jobId}/inputData";

                job.OutputDataFormat = "microsoft.quantum-results.v1";
                job.OutputDataUri = $"{Environment.GetEnvironmentVariable("quantumStorage")}/{jobId}";

                var response = await client.CreateJobAsync(jobId, job);

                var jobTable = new Jobs()
                {
                    PartitionKey = "key",
                    RowKey = jobId,
                    status = "waiting"
                };

                TableOperation insertJob = TableOperation.InsertOrReplace(jobTable);
                await jobs.ExecuteAsync(insertJob);

            }
        }    
    }
}
