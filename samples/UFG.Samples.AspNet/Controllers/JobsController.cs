using Microsoft.AspNetCore.Mvc;
using UFG.BackgroundTaskQueue;

namespace UFG.Samples.AspNet.Controllers;

[ApiController]
[Route("api/[controller]")]
public class JobsController : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> EnqueueJob([FromServices] ITaskQueue queue)
    {
        await queue.EnqueueAsync(async (scope, ct) =>
        {
            //Do time consuming work....
            await Task.Delay(10_000, ct);
            
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<JobsController>>();
            logger.Log(LogLevel.Information, "Logging from enqueued task");
        });

        return NoContent();
    }
}