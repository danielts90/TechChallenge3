using ContatoApi.Entities;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ContatoIntegrationTest
{
    
    public class ContatoIntegrationTest : IClassFixture<CustomWebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;

        public ContatoIntegrationTest(CustomWebApplicationFactory<Program> factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task Get_Contatos_ReturnsSuccessStatusCode()
        {
            // Act
            var response = await _client.GetAsync("/contato");

            // Assert
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            Console.WriteLine(content);
            Assert.NotNull(content);
        }

        [Fact]
        public async Task Get_ContatosById_ReturnsSuccessStatusCode()
        {
            // Act
            var response = await _client.GetAsync("/contato/1");

            // Assert
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            Assert.NotNull(content);
        }

        [Fact]
        public async Task CreateContato_IntegrationTest_ReturnsSuccessStatusCode()
        {
            // Arrange
            var contato = new Contato { Nome = "Teste A", Email = "teste@teste.com.br", DddId = 1, Telefone = "123456789" };
            var json = JsonSerializer.Serialize(contato);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PostAsync("/contato", content);

            // Assert
            response.EnsureSuccessStatusCode();
        }

        [Fact]
        public async Task Update_RegiaoById_ReturnsSuccessStatusCode()
        {
            // Arrange
            var contato = new Contato { Id = 1, Nome = "Teste A", Email = "teste@teste.com.br", DddId = 1, Telefone = "123456789" };
            var json = JsonSerializer.Serialize(contato);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            //Act
            var response = await _client.PutAsync("/contato/1", content);

            // Assert
            response.EnsureSuccessStatusCode();
        }

        [Fact]
        public async Task Delete_RegiaoById_ReturnsSuccessStatusCode()
        {
            // Act
            var response = await _client.DeleteAsync("/contato/2");

            // Assert
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            Assert.NotNull(content);
        }
    }
}
