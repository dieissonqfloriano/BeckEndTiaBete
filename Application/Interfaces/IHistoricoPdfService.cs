using Application.DTOs;
using Domain.Entities;

public interface IHistoricoPdfService
{
    byte[] GerarHistoricoPdf(UsuarioOutputDto usuario, List<RegistroGlicemiaOutputDto> registros);

}