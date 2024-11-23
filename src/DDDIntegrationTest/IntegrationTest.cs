using DddApi.Context;
using DddApi.Entities;
using DddApi.RabbitMq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace DDDIntegrationTest
{
    public class CustomWebApplicationFactory<TProgram> : WebApplicationFactory<TProgram> where TProgram : class
    {
        protected override IHost CreateHost(IHostBuilder builder)
        {
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
            builder.ConfigureServices(services =>
            {
                builder.ConfigureServices(services =>
                {
                    var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<DddDb>));
                    if (descriptor != null)
                    {
                        services.Remove(descriptor);
                    }

                    services.AddDbContext<DddDb>(options =>
                    {
                        var connectionString = "Host=postgres;Database=DddTestDb;Username=techuser;Password=techpassword";
                        options.UseNpgsql(connectionString);
                    });
                });

                //services.AddSingleton<IMessageProducer>(provider => new Producer("test.ddd.updated"));
            });
        }


    }
}
