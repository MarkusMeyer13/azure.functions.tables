using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Azure.Data.Tables;
using Newtonsoft.Json.Linq;

namespace SampleTableService;

public static class SampleTableServiceFunction
{
    [FunctionName("Delete")]
    public static async Task<IActionResult> DeleteAsync(
        [HttpTrigger(AuthorizationLevel.Function, "delete", Route = "sample/{partitionKey}/{rowKey}")] HttpRequest req,
        [Table("Sample", Connection = "SampleTableConnection")] TableClient client,
        string partitionKey,
        string rowKey,
        ILogger log)
    {
        var result = await client.DeleteEntityAsync(partitionKey, rowKey).ConfigureAwait(false);
        log.LogInformation($"Delete - StatusCode: {result.Status}");
        log.LogInformation($"Delete - Content: {result.Content}");

        if (result.Status.Equals(404))
        {
            log.LogInformation($"Delete - not found");
            return new NotFoundResult();
        }

        log.LogInformation($"Delete - deleted");

        return new NoContentResult();
    }

    [FunctionName("Add")]
    public static async Task<IActionResult> AddAsync(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "sample")] HttpRequest req,
        [Table("Sample", Connection = "SampleTableConnection")] TableClient client,
        ILogger log)
    {
        var body = await req.ReadAsStringAsync().ConfigureAwait(false);

        var data = JObject.Parse(body);

        var entity = new TableEntity(data["Country"].Value<string>(), data["Company"].Value<string>())
        {
            ["Description"] = data["Description"].Value<string>()
        };

        var result = await client.AddEntityAsync(entity).ConfigureAwait(false);
        log.LogInformation($"Add - StatusCode: {result.Status}");
        log.LogInformation($"Add - Content: {result.Content}");

        log.LogInformation($"Add - added");

        return new NoContentResult();
    }
}