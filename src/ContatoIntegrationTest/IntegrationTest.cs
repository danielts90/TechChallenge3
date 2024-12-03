using ContatoApi.Context;
using ContatoApi.Entities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ContatoIntegrationTest
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
                var dbContext = services.GetRequiredService<ContatoDb>();

                dbContext.Database.EnsureDeleted();
                dbContext.Database.EnsureCreated();

                if (!dbContext.Contatos.Any())
                {
                    dbContext.Contatos.AddRange(
                        new Contato { Nome = "Teste A", Email = "teste@teste.com.br", DddId = 1, Telefone = "123456789" },
                        new Contato { Nome = "Teste B", Email = "teste@teste.com.br", DddId = 1, Telefone = "123456789" },
                        new Contato { Nome = "Teste C", Email = "teste@teste.com.br", DddId = 1, Telefone = "123456789" }
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

                var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<ContatoDb>));
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                services.AddDbContext<ContatoDb>(options =>
                {
                    options.UseNpgsql(connectionString);
                });
            });
        }
    }
}
