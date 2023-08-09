namespace DolarApi.Services;

public interface IMetalesService
{
    Task<IResult> GetMetales(string cotizacionesUrl, string nombreCotizacion);
}
