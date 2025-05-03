using Azure.ResourceManager;
using Azure.ResourceManager.Batch;
using Azure.ResourceManager.Batch.Models;
using MBatch.Azure.Extensions.Models;
using MBatch.TestApi;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Batch;

namespace MBatch.Azure.Extensions.TestApi.Controllers;

[ApiController]
[Route("poolarm")]
public class BatchPoolResourceController : ControllerBase
{
    private readonly BatchConfiguration _batchConfiguration;
    private readonly ArmClient _armClient;
    private readonly BatchAccountResource _batchAccountResource;
    private readonly BatchClient _batchClient;
    private readonly ILogger<BatchPoolResourceController> _logger;

    public BatchPoolResourceController(
        BatchConfiguration batchConfiguration,
        ArmClient armClient,
        BatchAccountResource batchAccountResource,
        BatchClient batchClient,
        ILogger<BatchPoolResourceController> logger)
    {
        _batchConfiguration = batchConfiguration;
        _armClient = armClient;
        _batchAccountResource = batchAccountResource;
        _batchClient = batchClient;
        _logger = logger;
    }

    [HttpPost("create")]
    public async Task<IActionResult> Create([FromBody] CreateBody body)
    {
        var vmConfiguration = VMUtilities.MatchVirtualMachineConfiguration([.. _batchClient.PoolOperations.ListSupportedImages()], _batchConfiguration.Sku, null, null);

        var vmSize = _armClient.GetVirtualMachineSize(_batchConfiguration.SubscriptionId, _batchConfiguration.Location, minMemory: 1, minvCPUs: 1);

        var application = body.ApplicationName is not null && body.ApplicationVersion is not null ? new ApplicationPackageReference
        {
            ApplicationId = body.ApplicationName,
            Version = body.ApplicationVersion
        } : null;

        var identity = body.ManagedIdentityName is not null ? new ManagedIdentityInfo(_batchConfiguration.SubscriptionId, _batchConfiguration.ResourceGroup, body.ManagedIdentityName) : null;

        var startTask = body.StartTaskCommandLine is not null ? new StartTaskSettings(body.StartTaskCommandLine, true, BatchUserAccountElevationLevel.Admin, BatchAutoUserScope.Pool) : null;

        var scaleSettings = new FixedScaleSettings(body.NumberOfNodes ?? 1);

        await _batchAccountResource.CreatePoolAsync(
            body.PoolId,
            vmConfiguration,
            vmSize,
            application is not null ? [application] : null,
            identity is not null ? [identity] : null,
            startTask,
            scaleSettings,
            true
            );

        return Ok();
    }

    [HttpPost("update")]
    public async Task<IActionResult> Update([FromBody] CreateBody body)
    {
        var application = body.ApplicationName is not null && body.ApplicationVersion is not null ? new ApplicationPackageReference
        {
            ApplicationId = body.ApplicationName,
            Version = body.ApplicationVersion
        } : null;

        var identity = body.ManagedIdentityName is not null ? new ManagedIdentityInfo(_batchConfiguration.SubscriptionId, _batchConfiguration.ResourceGroup, body.ManagedIdentityName) : null;

        var startTask = body.StartTaskCommandLine is not null ? new StartTaskSettings(body.StartTaskCommandLine, true, BatchUserAccountElevationLevel.Admin, BatchAutoUserScope.Pool) : null;

        var scaleSettings = new FixedScaleSettings(body.NumberOfNodes ?? 1);

        await _batchAccountResource.UpdatePoolAsync(
            body.PoolId,
            application is not null ? [application] : null,
            identity is not null ? [identity] : null,
            startTask,
            scaleSettings
            );

        return Ok();
    }

    public class CreateBody
    {
        public string PoolId { get; set; }
        public string ApplicationName { get; set; }
        public string ApplicationVersion { get; set; }
        public string ManagedIdentityName { get; set; }
        public string StartTaskCommandLine { get; set; }
        public int? NumberOfNodes { get; set; }
    }

    [HttpDelete("{poolId}")]
    public async Task<IActionResult> Delete(string poolId)
    {
        await _batchAccountResource.DeletePoolAsync(poolId, false);

        return Ok();
    }
}
