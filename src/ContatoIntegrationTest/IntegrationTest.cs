using ContatoApi.Context;
using ContatoApi.Entities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Npgsql;

namespace ContatoIntegrationTest
{
    public class CustomWebApplicationFactory<TProgram> : WebApplicationFactory<TProgram> where TProgram : class
    {
        protected override IHost CreateHost(IHostBuilder builder)
        {
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
            builder.ConfigureServices(services =>
            {
                var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<ContatoDb>));
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                services.AddDbContext<ContatoDb>(options =>
                {
                    var connectionString = "Host=localhost;Database=ContatoTestDb;Username=techuser;Password=techpassword";
                    options.UseNpgsql(connectionString);
                });
            });
        }
    }
}
