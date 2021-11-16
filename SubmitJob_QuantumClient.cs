using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Azure.Quantum;
using Microsoft.Quantum.Providers.IonQ.Targets;
using Microsoft.Quantum.Simulation.Core;
using Quantum.QSharpApplication1;
using Microsoft.Quantum.Arrays;

namespace demo
{
    public static class SubmitJob_QuantumClient
    {
        [FunctionName("SubmitJob_QunatumClient")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {

            // First create an instance of a Quantum Workspace, where the jobs are executed in Azure:
            // Fill out with your own Azure Workspace information
            var workspace = new Workspace
            (
                subscriptionId: Environment.GetEnvironmentVariable("subId"),
                resourceGroupName: Environment.GetEnvironmentVariable("resourceGroup"),
                workspaceName: Environment.GetEnvironmentVariable("workspace"),
                location: Environment.GetEnvironmentVariable("location")
            );

            // Then select the machine that you will be executing the Q# operation.
            // This target needs to be enabled in the workspace above.
            var targetId = "ionq.simulator";
                // Create the machine for the Azure Quantum Provider
                // you'll be submitting jobs to:
                var quantumMachine = new IonQQuantumMachine(
                    target: targetId,
                    workspace: workspace);

                // Submit a job for each sample Q# application.
                // The Q# operation needs to be defined in a separate Q# library
                // that is imported as a ProjectReference.                
                var randomBitJob = await quantumMachine.SubmitAsync(SampleRandomNumber.Info, 4);

                // Print the job ids:
                Console.WriteLine($"RandomBit job id: {randomBitJob.Id}");
            

            return new OkObjectResult("");
        }
    }
}
