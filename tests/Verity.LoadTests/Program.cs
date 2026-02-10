using System.Diagnostics;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

Console.WriteLine("Initializing Load Test...");

var httpClient = new HttpClient();
// Dummy JWT
var token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJsb2FkLXRlc3QtdXNlciIsIm5hbWUiOiJMb2FkIFRlc3QgVXNlciIsImV4cCI6OTk5OTk5OTk5OX0.dummy"; 
httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

var durationSeconds = 30;
var targetRps = 50;
var totalRequests = targetRps * durationSeconds;
var delayMs = 1000 / targetRps;

Console.WriteLine($"Starting Load Test: {targetRps} RPS for {durationSeconds} seconds. Total: {totalRequests} requests.");

var successCount = 0;
var errorCount = 0;
var stopwatch = Stopwatch.StartNew();

var tasks = new List<Task>();

// We launch requests in a loop with delay
for (int i = 0; i < totalRequests; i++)
{
    tasks.Add(Task.Run(async () =>
    {
        try
        {
            var response = await httpClient.PostAsJsonAsync("http://localhost:5002/api/transactions", new
            {
                Amount = 100,
                Type = 1,
                Description = $"Load Test Transaction {Guid.NewGuid()}"
            });

            if (response.IsSuccessStatusCode)
            {
                Interlocked.Increment(ref successCount);
            }
            else
            {
                // Console.WriteLine($"Error: {response.StatusCode}");
                Interlocked.Increment(ref errorCount);
            }
        }
        catch (Exception ex)
        {
            // Console.WriteLine($"Exception: {ex.Message}");
            Interlocked.Increment(ref errorCount);
        }
    }));

    // Adjust delay to maintain RPS (simple sleep)
    // For more precision we would use a timer, but this suffices for "implementation"
    await Task.Delay(delayMs);
}

await Task.WhenAll(tasks);
stopwatch.Stop();

Console.WriteLine($"Load Test Completed in {stopwatch.Elapsed.TotalSeconds:F2}s");
Console.WriteLine($"Total Requests: {totalRequests}");
Console.WriteLine($"Success: {successCount}");
Console.WriteLine($"Error: {errorCount}");

var lossRate = totalRequests > 0 ? (double)errorCount / totalRequests * 100 : 0;
Console.WriteLine($"Loss Rate: {lossRate:F2}%");

if (lossRate > 5)
{
    Console.WriteLine("Validation Failed: Loss rate > 5%");
    Environment.Exit(1);
}
else
{
    Console.WriteLine("Validation Passed: Loss rate <= 5%");
    Environment.Exit(0);
}
