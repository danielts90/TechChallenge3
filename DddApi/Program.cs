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

#if DEBUG


builder.Services.AddHttpClient("regiao", httpclient => {
    httpclient.BaseAddress = new Uri("https://localhost:7236/");
});
#else
builder.Services.AddHttpClient("ddd", httpclient => {
    httpclient.BaseAddress = new Uri("http://host.docker.internal:8081/");
});
#endif

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApiDocument(config =>
{
    config.DocumentName = "DddApi";
    config.Title = "DddApi v1";
    config.Version = "v1";
});

builder.Services.AddHostedService<RegiaoHostedService>();
builder.Services.AddSingleton<IRegiaoService, RegiaoService>();
builder.Services.AddSingleton<IMessageProducer>(provider => new Producer("ddd.updated"));

var app = builder.Build();

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

var consumer = new RabbitMqConsumer<Regiao>("localhost", "regiao.updated", app.Services.GetRequiredService<IRegiaoService>());
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
