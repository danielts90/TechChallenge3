using ContatoApi.Context;
using ContatoApi.Models;
using ContatoApi.RabbitMq;
using ContatoApi.Services;
using Microsoft.EntityFrameworkCore;
using Prometheus;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddDbContext<ContatoDb>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));


#if DEBUG
builder.Services.AddHttpClient("ddd", httpclient => {
    httpclient.BaseAddress = new Uri("https://localhost:7143/");
});
#else
builder.Services.AddHttpClient("ddd", httpclient => {
    httpclient.BaseAddress = new Uri("http://host.docker.internal:8082/");
});
#endif

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApiDocument(config =>
{
    config.DocumentName = "Contato API";
    config.Title = "ContaotApi v1";
    config.Version = "v1";
});

builder.Services.AddHostedService<DddHostedService>();
builder.Services.AddSingleton<IDddService, DddService>();

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

app.UseOpenApi();
app.UseSwaggerUi(config =>
{
    config.DocumentTitle = "ContatoAPI";
    config.Path = "/swagger";
    config.DocumentPath = "/swagger/{documentName}/swagger.json";
    config.DocExpansion = "list";
});

var consumer = new RabbitMqConsumer<Ddd>("localhost", "ddd.updated", app.Services.GetRequiredService<IDddService>());
Task.Run(() => consumer.StartConsumer());


app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();
app.MapGet("/ddds", (IDddService dddService) =>
{
    return TypedResults.Ok(dddService.GetCachedDdds());
});

app.MapMetrics();

app.Run();
public partial class Program { }