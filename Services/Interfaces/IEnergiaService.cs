namespace DolarApi.Services;

public interface IEnergiaService
{
    Task<IResult> GetEnergia(string cotizacionesUrl, string nombreCotizacion);
}
