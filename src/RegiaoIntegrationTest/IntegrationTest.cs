using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RegiaoApi.Context;
using RegiaoApi.Models;

namespace RegiaoIntegrationTest
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
                var dbContext = services.GetRequiredService<RegiaoDb>();

                if (IsTestEnvironment())
                {
                    dbContext.Database.EnsureDeleted();
                    dbContext.Database.Migrate();

                    if (!dbContext.Regioes.Any())
                    {
                        dbContext.Regioes.AddRange(
                            new Regiao { Name = "Regiao 1" },
                            new Regiao { Name = "Regiao 2" },
                            new Regiao { Name = "Regiao 3" }
                        );
                        dbContext.SaveChanges();
                    }
                }
            }

            return host;
        }

        private bool IsTestEnvironment()
        {
            return Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Test";
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

                var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<RegiaoDb>));
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                services.AddDbContext<RegiaoDb>(options =>
                {
                    options.UseNpgsql(connectionString);
                });

                var rabbitMqHost = context.Configuration["RabbitMq:Host"];
                var rabbitMqPort = context.Configuration["RabbitMq:Port"];
                var rabbitMqUser = context.Configuration["RabbitMq:User"];
                var rabbitMqPassword = context.Configuration["RabbitMq:Password"];
                var rabbitMqRegiaoQueue = context.Configuration["RabbitMq:RegiaoQueue"];
                var rabbitMqDddQueue = context.Configuration["RabbitMq:DddQueue"];

                services.AddSingleton<IMessageProducer>(provider => new Producer(rabbitMqHost, rabbitMqRegiaoQueue, Convert.ToInt32(rabbitMqPort), rabbitMqUser, rabbitMqPassword));
            });
        }
    }
}
