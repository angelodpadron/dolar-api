namespace DolarApi.Services;

public interface IReservasYCirculantesService
{
    Task<IResult> GetReservas(string cotizacionesUrl);
    Task<IResult> GetCirculantes(string cotizacionesUrl);
}
