namespace DolarApi.Services;

public interface IAgroService
{
    Task<IResult> GetAgro(string cotizacionesUrl, string nombreCotizacion);
}
