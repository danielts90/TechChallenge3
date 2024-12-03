using Microsoft.EntityFrameworkCore;
using Prometheus;
using RegiaoApi.Context;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle

builder.Services.AddDbContext<RegiaoDb>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApiDocument(config =>
{
    config.DocumentName = "RegiaoApi";
    config.Title = "RegiaoApi v1";
    config.Version = "v1";
});

var rabbitMqHost = builder.Configuration["RabbitMq:Host"];
var rabbitMqPort = builder.Configuration["RabbitMq:Port"];
var rabbitMqUser = builder.Configuration["RabbitMq:User"];
var rabbitMqPassword = builder.Configuration["RabbitMq:Password"];
var rabbitMqRegiaoQueue = builder.Configuration["RabbitMq:RegiaoQueue"];
var rabbitMqDddQueue = builder.Configuration["RabbitMq:DddQueue"];


builder.Services.AddSingleton<IMessageProducer>(provider => new Producer(rabbitMqHost, rabbitMqRegiaoQueue, Convert.ToInt32(rabbitMqPort), rabbitMqUser, rabbitMqPassword));
var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<RegiaoDb>();
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
    config.DocumentTitle = "RegiaoAPI";
    config.Path = "/swagger";
    config.DocumentPath = "/swagger/{documentName}/swagger.json";
    config.DocExpansion = "list";
});


app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.MapMetrics();

app.Run();

public partial class Program { }
