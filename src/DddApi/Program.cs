using DddApi.Context;
using DddApi.Models;
using DddApi.RabbitMq;
using DddApi.Services;
using Microsoft.EntityFrameworkCore;
using Prometheus;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddDbContext<DddDb>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));


var regiaoApi = builder.Configuration["HttpClients:regiaoApi"];

builder.Services.AddHttpClient("regiao", httpclient => {
    httpclient.BaseAddress = new Uri(regiaoApi);
});


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApiDocument(config =>
{
    config.DocumentName = "DddApi";
    config.Title = "DddApi v1";
    config.Version = "v1";
});

builder.Services.AddHostedService<RegiaoHostedService>();
builder.Services.AddSingleton<IRegiaoService, RegiaoService>();

var rabbitMqHost = builder.Configuration["RabbitMq:Host"];
var rabbitMqPort = builder.Configuration["RabbitMq:Port"];
var rabbitMqUser = builder.Configuration["RabbitMq:User"];
var rabbitMqPassword = builder.Configuration["RabbitMq:Password"];
var rabbitMqRegiaoQueue = builder.Configuration["RabbitMq:RegiaoQueue"];
var rabbitMqDddQueue = builder.Configuration["RabbitMq:DddQueue"];


builder.Services.AddSingleton<IMessageProducer>(provider => new Producer(rabbitMqHost, rabbitMqDddQueue, Convert.ToInt32(rabbitMqPort), rabbitMqUser, rabbitMqPassword));

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<DddDb>();
    try
    {
        if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") != "Test")
        {
            dbContext.Database.Migrate();
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Erro ao aplicar migrações: {ex.Message}");
        throw;
    }
}


var counter = Metrics.CreateCounter("webapimetric", "Contador de requests",
    new CounterConfiguration
    {
        LabelNames = new[] { "method", "endpoint", "status" }
    });

app.Use((context, next) =>
{
    counter.WithLabels(context.Request.Method, context.Request.Path, context.Response.StatusCode.ToString()).Inc();
    return next();
});



var consumer = new RabbitMqConsumer<Regiao>(rabbitMqHost, rabbitMqRegiaoQueue, Convert.ToInt32(rabbitMqPort), rabbitMqUser, rabbitMqPassword,  app.Services.GetRequiredService<IRegiaoService>());
Task.Run(() => consumer.StartConsumer());

app.UseOpenApi();
app.UseSwaggerUi(config =>
{
    config.DocumentTitle = "DddAPI";
    config.Path = "/swagger";
    config.DocumentPath = "/swagger/{documentName}/swagger.json";
    config.DocExpansion = "list";
});

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();
app.MapGet("/regioes", (IRegiaoService regiaoService) =>
{
    return TypedResults.Ok(regiaoService.GetCachedRegioes());
});

app.MapMetrics();
app.Run();

public partial class Program { }
