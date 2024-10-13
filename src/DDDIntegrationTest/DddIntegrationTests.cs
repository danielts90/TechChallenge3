using DddApi.Entities;
using DddApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace DDDIntegrationTest
{
    public class DddIntegrationTests : IClassFixture<CustomWebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;

        public DddIntegrationTests(CustomWebApplicationFactory<Program> factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task Get_Ddds_ReturnsSuccessStatusCode()
        {
            // Act
            var response = await _client.GetAsync("/ddd");

            // Assert
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            Console.WriteLine(content);
            Assert.NotNull(content);
        }

        [Fact]
        public async Task Get_DddsById_ReturnsSuccessStatusCode()
        {
            // Act
            var response = await _client.GetAsync("/ddd/1");

            // Assert
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            Assert.NotNull(content);
        }

        [Fact]
        public async Task CreateDdd_IntegrationTest_ReturnsSuccessStatusCode()
        {
            // Arrange
            var ddd = new Ddd { Code = "99", RegiaoId = 1 };
            var json = JsonSerializer.Serialize(ddd);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PostAsync("/ddd", content);

            // Assert
            response.EnsureSuccessStatusCode();
        }

        [Fact]
        public async Task Update_DddById_ReturnsSuccessStatusCode()
        {
            // Arrange
            var ddd = new Ddd { Code = "88", RegiaoId = 2 };
            var json = JsonSerializer.Serialize(ddd);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            //Act
            var response = await _client.PutAsync("/ddd/1", content);

            // Assert
            response.EnsureSuccessStatusCode();
        }

        [Fact]
        public async Task Delete_RegiaoById_ReturnsSuccessStatusCode()
        {
            // Act
            var response = await _client.DeleteAsync("/ddd/2");

            // Assert
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            Assert.NotNull(content);
        }
    }
}
