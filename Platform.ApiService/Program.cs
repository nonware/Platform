using Platform.Contracts;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

Log.Logger = new LoggerConfiguration()
    .CreateLogger();

builder.AddSeqEndpoint(connectionName: "seq");

// Add Orleans
builder.UseOrleansClient();

// Add services to the container.
builder.Services.AddProblemDetails();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapGet("/counter/{grainId}", async (IClusterClient client, string grainId) =>
{
    var grain = client.GetGrain<ICounterGrain>(grainId);
    return await grain.Get();
});

app.MapPost("/counter/{grainId}", async (IClusterClient client, string grainId) =>
{
    var grain = client.GetGrain<ICounterGrain>(grainId);
    var inc =  await grain.Increment();
    return inc;
});

app.MapDefaultEndpoints();

app.Run();