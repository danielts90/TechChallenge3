using DddApi.Context;
using DddApi.Entities;
using DddApi.RabbitMq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace DDDIntegrationTest
{
    public class CustomWebApplicationFactory<TProgram> : WebApplicationFactory<TProgram> where TProgram : class
    {
        protected override IHost CreateHost(IHostBuilder builder)
        {
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Test");
            var host = base.CreateHost(builder);

            using (var scope = host.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                var dbContext = services.GetRequiredService<DddDb>();

                dbContext.Database.EnsureDeleted();
                dbContext.Database.EnsureCreated();

                if (!dbContext.Ddds.Any())
                {
                    dbContext.Ddds.AddRange(
                        new Ddd { Code = "11", RegiaoId = 1 },
                        new Ddd { Code = "22", RegiaoId = 1 },
                        new Ddd { Code = "33", RegiaoId = 1 }
                    );
                    dbContext.SaveChanges();
                }
            }

            return host;
        }


        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureAppConfiguration((context, config) =>
            {
                config.AddJsonFile("appsettings.Test.json", optional: false, reloadOnChange: true);
            });

            builder.ConfigureServices((context, services) =>
            {
                var configuration = context.Configuration;
                var connectionString = configuration.GetConnectionString("DefaultConnection");

                var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<DddDb>));
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                services.AddDbContext<DddDb>(options =>
                {
                    options.UseNpgsql(connectionString);
                });

                var rabbitMqHost = context.Configuration["RabbitMq:Host"];
                var rabbitMqPort = context.Configuration["RabbitMq:Port"];
                var rabbitMqUser = context.Configuration["RabbitMq:User"];
                var rabbitMqPassword = context.Configuration["RabbitMq:Password"];
                var rabbitMqRegiaoQueue = context.Configuration["RabbitMq:RegiaoQueue"];
                var rabbitMqDddQueue = context.Configuration["RabbitMq:DddQueue"];

                services.AddSingleton<IMessageProducer>(provider => new Producer(rabbitMqHost, rabbitMqDddQueue, Convert.ToInt32(rabbitMqPort), rabbitMqUser, rabbitMqPassword));
            });
        }


    }
}
