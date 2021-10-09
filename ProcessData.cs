using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Azure.Cosmos.Table;
using System.Linq;
using Azure.Quantum.Jobs;
using Azure.Identity;
using Azure.Quantum.Jobs.Models;
using Azure.Storage.Blobs;
using System.Text;
using demo.Models;

namespace demo
{
    public static class ProcessData
    {
        [FunctionName("ProcessData")]
        public static async Task Run([TimerTrigger("0 0 */4 * * *")] TimerInfo myTimer,
            [Table("jobs")] CloudTable jobs,
            [Table("numbers")] CloudTable numbers,
            ILogger log)
        {
            var client = new QuantumJobClient(
                Environment.GetEnvironmentVariable("subId"),
                Environment.GetEnvironmentVariable("workspace"),
                Environment.GetEnvironmentVariable("workspace"),
                "eastUS",
                new DefaultAzureCredential());

            var filter = TableQuery.GenerateFilterCondition("status", QueryComparisons.Equal, "waiting");

            var query = (await jobs.ExecuteQuerySegmentedAsync(new TableQuery<Jobs>().Where(filter).Take(5), null));

            foreach(var item in query)
            {
                var status = (await client.GetJobAsync(item.RowKey)).Value;

                if(status.Status == JobStatus.Failed || status.Status == JobStatus.Cancelled)
                {
                    item.status = "failed";

                    TableOperation replaceEntity = TableOperation.Replace(item);
                    await jobs.ExecuteAsync(replaceEntity);
                }
                else if(status.Status == JobStatus.Succeeded)
                {
                    var blob = new BlobClient(new Uri(status.OutputDataUri));
                    var data = await blob.DownloadContentAsync();

                    var results = JsonConvert.DeserializeObject<Results>(Encoding.UTF8.GetString(data.Value.Content));

                    foreach(var value in results.Values)
                    {
                        var numbersRow = new Number()
                        {
                            PartitionKey = "1",
                            RowKey = Guid.NewGuid().ToString(),
                            Numbers = value,
                            State = "new"
                        };

                        TableOperation insertNumbers = TableOperation.InsertOrReplace(numbersRow);
                        await numbers.ExecuteAsync(insertNumbers);
                    }
                }
            }
        }
    }
}
