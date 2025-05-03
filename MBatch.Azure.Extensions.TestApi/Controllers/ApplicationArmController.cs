using Azure.ResourceManager;
using Azure.Storage.Blobs;
using MBatch.TestApi;
using Microsoft.AspNetCore.Mvc;

namespace MBatch.Azure.Extensions.TestApi.Controllers;

[ApiController]
[Route("applicationPackageArm")]
public class ApplicationArmController : ControllerBase
{
    private readonly BatchConfiguration _batchConfiguration;
    private readonly ArmClient _armClient;
    private readonly ILogger<ApplicationArmController> _logger;

    public ApplicationArmController(
        BatchConfiguration batchConfiguration,
        ArmClient armClient,
        ILogger<ApplicationArmController> logger)
    {
        _batchConfiguration = batchConfiguration;
        _armClient = armClient;
        _logger = logger;
    }

    [HttpPost("upload")]
    public async Task<IActionResult> UploadArm([FromForm] string appName, [FromForm] string appVersion, IFormFile file)
    {
        var uri = await _armClient.UpdateBatchApplicationPackageAsync(_batchConfiguration.SubscriptionId, _batchConfiguration.ResourceGroup, _batchConfiguration.BatchAccountName, appName, appVersion, true);

        // use blob storage sdk to upload zip file
        await UploadFile(_batchConfiguration.BlobStorageConn, uri, file);

        return Ok();
    }

    [HttpPost("activate")]
    public async Task<IActionResult> Activate([FromBody] ActivateBody body)
    {
        await _armClient.ActivateBatchApplicationPackageAsync(_batchConfiguration.SubscriptionId, _batchConfiguration.ResourceGroup, _batchConfiguration.BatchAccountName, body.AppName, body.AppVersion);

        return Ok();
    }

    [HttpDelete("package/{appName}/{appVersion}")]
    public async Task<IActionResult> DeletePackage(string appName, string appVersion)
    {
        await _armClient.DeleteBatchApplicationPackageAsync(_batchConfiguration.SubscriptionId, _batchConfiguration.ResourceGroup, _batchConfiguration.BatchAccountName, appName, appVersion, true);

        return Ok();
    }

    [HttpDelete("{appName}")]
    public async Task<IActionResult> DeleteApp(string appName)
    {
        await _armClient.DeleteBatchApplicationAsync(_batchConfiguration.SubscriptionId, _batchConfiguration.ResourceGroup, _batchConfiguration.BatchAccountName, appName, true);

        return Ok();
    }

    public class ActivateBody
    {
        public string AppName { get; set; }
        public string AppVersion { get; set; }
    }

    internal static async Task UploadFile(string connString, Uri fileUri, IFormFile file)
    {
        var (containerName, fullPath) = fileUri.GetBlobAbsolutePathParts();

        BlobServiceClient client = new(connString);

        var containerClient = client.GetBlobContainerClient(containerName);
        await containerClient.CreateIfNotExistsAsync();

        var blobClient = containerClient.GetBlobClient(fullPath);

        await blobClient.UploadAsync(file.OpenReadStream());
    }
}
