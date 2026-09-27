using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using System.Net;
using System.Net.Http.Json;

namespace Tests
{
    public class RateLimitTests
        : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;

        public RateLimitTests(
            WebApplicationFactory<Program> factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task Login_DeveRetornar429_AposUltrapassarLimite()
        {
            var login = new
            {
                Email = "teste@teste.com",
                Senha = "senhaerrada"
            };

            HttpResponseMessage? resposta = null;

            for (int i = 0; i < 6; i++)
            {
                resposta = await _client.PostAsJsonAsync(
                    "/api/usuario/login",
                    login
                );
            }

            Assert.Equal(
                HttpStatusCode.TooManyRequests,
                resposta!.StatusCode
            );
        }

        [Fact]
        public async Task Api_DeveSuportarMultiplasRequisicoesSimultaneas_SemErro500()
        {
            var requisicoes = new List<Task<HttpResponseMessage>>();

            for (int i = 0; i < 20; i++)
            {
                requisicoes.Add(
                    _client.PostAsJsonAsync(
                        "/api/usuario/login",
                        new
                        {
                            Email = "teste@teste.com",
                            Senha = "senhaerrada"
                        }
                    )
                );
            }

            var respostas = await Task.WhenAll(requisicoes);

            Assert.All(
                respostas,
                resposta =>
                {
                    Assert.NotEqual(
                        HttpStatusCode.InternalServerError,
                        resposta.StatusCode
                    );
                }
            );
        }
    }
}