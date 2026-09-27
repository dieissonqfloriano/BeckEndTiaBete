using Microsoft.AspNetCore.Mvc.Testing;
using Xunit.Abstractions;
using System.Net;
using System.Net.Http.Json;

namespace Tests
{
    public class RateLimitTests
    : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;
        private readonly ITestOutputHelper _output;

        public RateLimitTests(
            WebApplicationFactory<Program> factory,
            ITestOutputHelper output)
        {
            _client = factory.CreateClient();
            _output = output;
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

        [Fact]
        public async Task Api_DeveSuportar500RequisicoesSimultaneas_SemErro500()
        {
            using var factory = new WebApplicationFactory<Program>();
            using var client = factory.CreateClient();

            var requisicoes = new List<Task<HttpResponseMessage>>();

            for (int i = 0; i < 500; i++)
            {
                requisicoes.Add(
                    client.PostAsJsonAsync(
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

            var statusEncontrados = respostas
            .GroupBy(r => r.StatusCode)
            .Select(grupo => new
            {
             Status = grupo.Key,
             Quantidade = grupo.Count()
            });

            foreach (var status in statusEncontrados)
            {
                _output.WriteLine(
                    $"{(int)status.Status} {status.Status}: {status.Quantidade}"
                );
            }

            var quantidade401 = respostas.Count(
                r => r.StatusCode == HttpStatusCode.Unauthorized
            );

            var quantidade429 = respostas.Count(
                r => r.StatusCode == HttpStatusCode.TooManyRequests
            );

            var quantidade500 = respostas.Count(
                r => r.StatusCode == HttpStatusCode.InternalServerError
            );

            _output.WriteLine($"500 Internal Server Error: {quantidade500}");

            Assert.Equal(0, quantidade500);
            Assert.True(quantidade429 > 0);
        }
    }
}