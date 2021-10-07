using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace demo
{
    public static class UpdatePage
    {
        [FunctionName("UpdatePage")]
        public static async Task Run([TimerTrigger("0 0 0 * * *")]TimerInfo myTimer,
            [Table("numbers")] CloudTable numbers,
            [Blob("site/template.html", FileAccess.Write)] Stream file,
            ILogger log)
        {
            log.LogInformation($"Page update started at {DateTime.Now}");

            string text = System.IO.File.ReadAllText(@".\page\template.html");

            /*
             * 1) get unused numbers from storage
             * 2) add them to the page
             * 3) write page to storage -DONE
             * 4) update table with used numbers
             * 
             */

            var filter = TableQuery.GenerateFilterCondition("State", QueryComparisons.Equal, "new");

            var query = (await numbers.ExecuteQuerySegmentedAsync(new TableQuery<Number>().Where(filter).Take(1), null)).FirstOrDefault();

            if (query != null)
            {
                var number = query.Numbers
                    .Replace("[", "")
                    .Replace("]", "")
                    .Replace(",", "");
                int value = Convert.ToInt32(number, 2);

                text = text.Replace("{numbers}", value.ToString());

                query.State = "completed";

                TableOperation replaceEntity = TableOperation.Replace(query);

                await numbers.ExecuteAsync(replaceEntity);
            }
            else
            {
                text = text.Replace("{numbers}", $"New values coming soon {DateTime.Now.ToShortTimeString()}");
            }

            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(text);
            writer.Flush();
            stream.Position = 0;
            stream.CopyTo(file);
        }
    }
}