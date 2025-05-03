using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Batch;

namespace MBatch.Azure.Extensions.TestApi.Controllers;

[ApiController]
[Route("job")]
public class JobController : ControllerBase
{
    private readonly BatchClient _batchClient;
    private readonly ILogger<JobController> _logger;

    public JobController(
        BatchClient batchClient,
        ILogger<JobController> logger)
    {
        _batchClient = batchClient;
        _logger = logger;
    }

    [HttpGet("{jobId}")]
    public async Task<IActionResult> Get(string jobId)
    {
        var job = await _batchClient.GetJobAsync(jobId);

        return Ok(job);
    }

    [HttpPost("{poolId}/{jobId}")]
    public async Task<IActionResult> Create(string poolId, string jobId)
    {
        var job = await _batchClient.CreateJobAsync(poolId, jobId);

        return Ok(job);
    }

    [HttpDelete("{jobId}")]
    public async Task<IActionResult> DeleteIfExists(string jobId)
    {
        await _batchClient.DeleteJobIfExistsAsync(jobId);

        return Ok(jobId);
    }

    [HttpPut("{jobId}")]
    public async Task<IActionResult> Update(string jobId, [FromBody] bool terminateJobAfterTasksCompleted)
    {
        await _batchClient.UpdateJobAsync(jobId, terminateJobAfterTasksCompleted);

        return Ok(jobId);
    }

    [HttpPost("{jobId}/terminate")]
    public async Task<IActionResult> TerminateJob(string jobId)
    {
        await _batchClient.TerminateJobAsync(jobId);

        return Ok(jobId);
    }

    [HttpGet("{poolId}/activeJobs")]
    public IActionResult GetActiveJobs(string poolId)
    {
        var list = _batchClient.GetRunningJobs(poolId);

        return Ok(list);
    }

    [HttpGet("{jobId}/failed")]
    public async Task<IActionResult> IsAnyTaskFailed(string jobId)
    {
        var failed = await _batchClient.IsAnyTaskFailedAsync(jobId);

        return Ok(failed);
    }

    [HttpGet("tasksStats")]
    public async Task<IActionResult> GetJobsTasksCountAsync()
    {
        var jobIds = _batchClient.JobOperations.ListJobs().Select(x => x.Id);

        var jobsTasksCount = await _batchClient.GetJobsTasksCountsAsync(jobIds);

        return Ok(jobsTasksCount);
    }
}
