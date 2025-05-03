using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Batch;

namespace MBatch.Azure.Extensions.TestApi.Controllers;

[ApiController]
[Route("task")]
public class JobTaskController : ControllerBase
{
    private readonly BatchClient _batchClient;
    private readonly ILogger<JobTaskController> _logger;

    public JobTaskController(
        BatchClient batchClient,
        ILogger<JobTaskController> logger)
    {
        _batchClient = batchClient;
        _logger = logger;
    }

    [HttpGet("{jobId}/{taskId}")]
    public async Task<IActionResult> Get(string jobId, string taskId)
    {
        var task = await _batchClient.GetTaskAsync(jobId, taskId);

        return Ok(new
        {
            task.Id,
            State = task.State.ToString()
        });
    }

    [HttpPost("{jobId}/{taskId}")]
    public async Task<IActionResult> Create(string jobId, string taskId, [FromBody] string commandLine)
    {
        var task = CloudTaskUtilities.CreateTask(taskId, commandLine);

        var success = await _batchClient.CommitTaskAsync(jobId, task, false);

        return Ok(success);
    }

    [HttpDelete("{jobId}/{taskId}")]
    public async Task<IActionResult> Delete(string jobId, string taskId)
    {
        var success = await _batchClient.DeleteTaskAsync(jobId, taskId);

        return Ok(success);
    }

    [HttpGet("taskCommandLine")]
    public IActionResult GetTaskCommandLine(string applicationName, string applicationVersion, string relativePath, bool isWindows, string arg)
    {
        var appPath = CommandLineUtilities.GetInstalledApplicationPath(isWindows, applicationName, applicationVersion, relativePath);

        var commandLine = $"cmd /c {appPath}{(!string.IsNullOrWhiteSpace(arg) ? $" -{arg}" : string.Empty)}";

        return Ok(commandLine);
    }
}
