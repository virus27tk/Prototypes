var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddHttpClient();

var app = builder.Build();

if (app.Environment.IsDevelopment())
    app.MapOpenApi();

app.MapPost("/trigger", async (TriggerRequest request, IHttpClientFactory httpClientFactory, IConfiguration config) =>
{
    // Simulate doing some work
    Console.WriteLine($"[Producer] Received trigger for job '{request.JobId}'. Starting work...");
    await Task.Delay(500); // pretend work
    Console.WriteLine($"[Producer] Job '{request.JobId}' completed. Firing webhook...");

    var webhookUrl = config["WebhookUrl"]!;
    var payload = new WebhookPayload(request.JobId, "completed", DateTime.UtcNow);

    var client = httpClientFactory.CreateClient();
    var response = await client.PostAsJsonAsync(webhookUrl, payload);

    Console.WriteLine($"[Producer] Webhook sent to {webhookUrl} → {(int)response.StatusCode} {response.ReasonPhrase} at {DateTime.UtcNow}");

    return Results.Ok(new { message = $"Job '{request.JobId}' done, webhook fired." });
});

app.MapPost("/webhook", async (WebhookPayload payload) =>
{
    Console.WriteLine($"[Receiver] Webhook received!");
    Console.WriteLine($"           JobId  : {payload.JobId}");
    Console.WriteLine($"           Status : {payload.Status}");
    Console.WriteLine($"           At     : {payload.CompletedAt:O}");
    //throw new NotImplementedException("Throwing at webhook");
    
    await Task.Delay(5000);
    return Results.Ok(new { received = true });
});

app.Run();

record TriggerRequest(string JobId);
record WebhookPayload(string JobId, string Status, DateTime CompletedAt);