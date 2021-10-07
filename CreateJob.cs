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
using Microsoft.Quantum.Providers.IonQ;
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

            var ionq = new Microsoft.Quantum.Providers.IonQ.Targets.IonQQuantumMachine.SubmissionContext();

            //ionq.

            var client =
                new QuantumJobClient(
                    Environment.GetEnvironmentVariable("subId"),
                    "penguinquantum",
                    "penguinquantum",
                    "eastUS",
                    new DefaultAzureCredential());

            var job = new JobDetails("", "", "", ""); 

            client.CreateJob("", job);

            return null;
        }

            
    }
}
