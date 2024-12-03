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

var dddApi = builder.Configuration["HttpClients:dddApi"];

builder.Services.AddHttpClient("ddd", httpclient => {
    httpclient.BaseAddress = new Uri(dddApi);
});

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

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ContatoDb>();
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

app.UseOpenApi();
app.UseSwaggerUi(config =>
{
    config.DocumentTitle = "ContatoAPI";
    config.Path = "/swagger";
    config.DocumentPath = "/swagger/{documentName}/swagger.json";
    config.DocExpansion = "list";
});


var rabbitMqHost = builder.Configuration["RabbitMq:Host"];
var rabbitMqPort = builder.Configuration["RabbitMq:Port"];
var rabbitMqUser = builder.Configuration["RabbitMq:User"];
var rabbitMqPassword = builder.Configuration["RabbitMq:Password"];
var rabbitMqRegiaoQueue = builder.Configuration["RabbitMq:RegiaoQueue"];
var rabbitMqDddQueue = builder.Configuration["RabbitMq:DddQueue"];

var consumer = new RabbitMqConsumer<Ddd>(rabbitMqHost, rabbitMqDddQueue, Convert.ToInt32(rabbitMqPort), rabbitMqUser, rabbitMqPassword, app.Services.GetRequiredService<IDddService>());
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