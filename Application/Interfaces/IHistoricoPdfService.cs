using Application.DTOs;

public interface IHistoricoPdfService
{
    byte[] GerarHistoricoPdf(UsuarioOutputDto usuario, List<RegistroGlicemiaOutputDto> registros);
}