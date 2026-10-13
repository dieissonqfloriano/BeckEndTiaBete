using Application.DTOs;
using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces;

namespace Application.Services
{
    public class RegistroGlicemiaService : IRegistroGlicemiaService
    {
        private readonly IRegistroGlicemiaRepository _repository;

        public RegistroGlicemiaService(IRegistroGlicemiaRepository repository)
        {
            _repository = repository;
        }

        public async Task<RegistroGlicemiaOutputDto> CreateAsync(RegistroGlicemiaCreateDto dto, int usuarioId)
        {
            var registro = new RegistroGlicemia { UsuarioId = usuarioId };

            Aplicar(registro, dto.Glicemia, dto.GlicemiaAcimaDoLimite, dto.Dose,
                dto.Hora, dto.Refeicao, dto.Data, dto.Observacao);

            await _repository.AddAsync(registro);
            await _repository.SaveChangesAsync();

            return MapearParaOutput(registro);
        }

        public Task<bool> DeleteAsync(int id, int usuarioId) =>
            _repository.DeleteAsync(id, usuarioId);

        public async Task<List<RegistroGlicemiaOutputDto>> GetAllAsync(
            int usuarioId,
            int pagina = 1,
            int tamanho = RegistroGlicemiaPaginacao.TamanhoPadrao)
        {
            pagina = Math.Max(pagina, 1);
            tamanho = Math.Clamp(tamanho, 1, RegistroGlicemiaPaginacao.TamanhoMaximo);

            var registros = await _repository.GetPaginaAsync(usuarioId, pagina, tamanho);

            return registros.Select(MapearParaOutput).ToList();
        }

        public async Task<RegistroGlicemiaOutputDto?> GetByIdAsync(int id, int usuarioId)
        {
            var registro = await _repository.GetByIdSomenteLeituraAsync(id, usuarioId);

            return registro == null ? null : MapearParaOutput(registro);
        }

        public async Task<bool> UpdateAsync(int id, RegistroGlicemiaUpdateDto dto, int usuarioId)
        {
            var registro = await _repository.GetByIdAsync(id, usuarioId);

            if (registro == null)
            {
                return false;
            }

            Aplicar(registro, dto.Glicemia, dto.GlicemiaAcimaDoLimite, dto.Dose,
                dto.Hora, dto.Refeicao, dto.Data, dto.Observacao);

            // A entidade já está sendo rastreada: o EF grava só as colunas alteradas.
            await _repository.SaveChangesAsync();

            return true;
        }

        public async Task<List<RegistroGlicemiaOutputDto>> GetByPeriodoAsync(
            int usuarioId, DateOnly dataInicial, DateOnly dataFinal)
        {
            var registros = await _repository.GetByPeriodoAsync(usuarioId, dataInicial, dataFinal);

            return registros.Select(MapearParaOutput).ToList();
        }

        private static void Aplicar(
            RegistroGlicemia registro,
            int? glicemia,
            bool acimaDoLimite,
            decimal dose,
            TimeSpan hora,
            string refeicao,
            DateOnly data,
            string? observacao)
        {
            TipoRefeicaoConversor.TentarConverter(refeicao, out var tipoRefeicao);

            registro.Glicemia = acimaDoLimite ? null : glicemia;
            registro.GlicemiaAcimaDoLimite = acimaDoLimite;
            registro.Dose = dose;
            registro.Hora = new TimeSpan(hora.Hours, hora.Minutes, hora.Seconds); // sem frações de segundo
            registro.Refeicao = tipoRefeicao;
            registro.Data = data;
            registro.Observacao = string.IsNullOrWhiteSpace(observacao) ? null : observacao.Trim();
        }

        private static RegistroGlicemiaOutputDto MapearParaOutput(RegistroGlicemia registro) => new()
        {
            Id = registro.Id,
            Glicemia = registro.Glicemia,
            GlicemiaAcimaDoLimite = registro.GlicemiaAcimaDoLimite,
            Dose = registro.Dose,
            Hora = registro.Hora,
            Refeicao = TipoRefeicaoConversor.ParaTexto(registro.Refeicao),
            Data = registro.Data,
            Observacao = registro.Observacao,
            UsuarioId = registro.UsuarioId
        };
    }
}
