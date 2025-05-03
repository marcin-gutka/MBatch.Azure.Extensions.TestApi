using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Batch;
using Microsoft.Azure.Batch.Common;

namespace MBatch.Azure.Extensions.TestApi.Controllers;

[ApiController]
[Route("pool")]
public class BatchPoolController : ControllerBase
{
    private readonly BatchClient _batchClient;
    private readonly ILogger<BatchPoolController> _logger;

    public BatchPoolController(
        BatchClient batchClient,
        ILogger<BatchPoolController> logger)
    {
        _batchClient = batchClient;
        _logger = logger;
    }

    [HttpGet("{poolId}/exist")]
    public async Task<IActionResult> DoesExistAsync(string poolId)
    {
        var exist = await _batchClient.DoesPoolExistAsync(poolId);

        return Ok(exist);
    }

    [HttpGet("{poolId}/jobs")]
    public IActionResult GetPoolJobs(string poolId)
    {
        var jobs = _batchClient.GetPoolJobs(poolId);

        return Ok(jobs);
    }

    [HttpPost("{poolId}/reboot")]
    public async Task<IActionResult> RebootAsync(string poolId)
    {
        await _batchClient.RebootNodesAsync(poolId, ComputeNodeRebootOption.Terminate);

        return Ok();
    }

    [HttpPut("{poolId}/setTargetNumber/{targetNumber:int}")]
    public async Task<IActionResult> SetTargetNodesCount(string poolId, int targetNumber)
    {
        var pool = await _batchClient.PoolOperations.GetPoolAsync(poolId);
        await pool.SetTargetNodesCountAsync(targetNumber, ComputeNodeDeallocationOption.TaskCompletion, _logger);

        return Ok();
    }

    [HttpPost("{poolId}/recover")]
    public async Task<IActionResult> RecoverAsync(string poolId)
    {
        var pool = await _batchClient.PoolOperations.GetPoolAsync(poolId);

        var numberOfUnhealthyNodes = await pool.RecoverUnhealthyNodesAsync(_logger);

        return Ok(numberOfUnhealthyNodes);
    }
}
