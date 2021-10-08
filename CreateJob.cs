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

namespace demo
{
    public static class CreateJob
    {
        [FunctionName("CreateJob")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation($"CreateJob function started {DateTime.Now}");

            var client =
                new QuantumJobClient(
                    Environment.GetEnvironmentVariable("subId"),
                    Environment.GetEnvironmentVariable("workspace"),
                    Environment.GetEnvironmentVariable("workspace"),
                    "eastUS",
                    new DefaultAzureCredential());


            var jobId = Guid.NewGuid().ToString().Replace("-", "");



            var job = new JobDetails($"{Environment.GetEnvironmentVariable("quantumStorage")}/createnumbers", 
                "microsoft.ionq-ir.v2", 
                "IonQ", 
                Environment.GetEnvironmentVariable("target"));
            //job.InputParams = "{ 'shots': 50 }"; //https://github.com/Azure/azure-sdk-for-net/issues/24580
            job.InputDataUri = $"{Environment.GetEnvironmentVariable("quantumStorage")}/createnumbers/inputData";

            job.OutputDataFormat = "microsoft.quantum-results.v1";
            job.OutputDataUri = $"{Environment.GetEnvironmentVariable("quantumStorage")}/quantum-job-{jobId}/mappingData";

            var response = client.CreateJob(jobId, job);

            return new OkObjectResult(JsonConvert.SerializeObject(response));
        }    
    }
}
