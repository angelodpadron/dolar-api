using Microsoft.OpenApi.Models;

using DolarApi.Services;
using DolarApi.Helpers;
using DolarApi.Exceptions;

var builder = WebApplication.CreateBuilder(args);
var port = Environment.GetEnvironmentVariable("PORT") ?? "8081";

builder.WebHost.UseUrls($"http://*:{port}");

builder.Services.AddHttpClient();

builder.Services.AddSingleton<ICotizacionDolarService, CotizacionDolarService>();
builder.Services.AddSingleton<IRiesgoPaisService, RiesgoPaisService>();
builder.Services.AddSingleton<IReservasYCirculantesService, ReservasYCirculantesService>();
builder.Services.AddSingleton<IAgroService, AgroService>();
builder.Services.AddSingleton<IEnergiaService, EnergiaService>();
builder.Services.AddSingleton<IMetalesService, MetalesService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc(
        "v1",
        new OpenApiInfo
        {
            Version = "v1.0.0",
            Title = "Dolar API",
            Description =
                "Minimal Web API hecha con ASP.NET Core para consultar valores económicos en Argentina (Dolar, Reservas, Riesgo País, etc).  \n Los datos son obtenidos de [DolarSi](https://www.dolarsi.com/)",
            Contact = new OpenApiContact
            {
                Name = "Angelo Padron",
                Url = new Uri("mailto:padron891@gmail.com?subject=Dolar API")
            },
            License = new OpenApiLicense
            {
                Name = "GNU GPLv3",
                Url = new Uri("https://www.gnu.org/licenses/gpl-3.0.en.html")
            }
        }
    );
});

var app = builder.Build();

app.UseSwagger();

var cotizacionesUrl =
    app.Configuration.GetValue<string>("CotizacionesUrl")
    ?? throw new BadConfigException("No se ha especificado la URL de las cotizaciones");
var endpointsConfigPath =
    app.Configuration.GetValue<string>("EndpointsConfigPath")
    ?? throw new BadConfigException(
        "La ruta de los endpoints no ha sido especificada o es inválida"
    );
await EndpointConfigurator.ConfigureEndpointsAsync(
    app,
    endpointsConfigPath,
    cotizacionesUrl
);

app.MapGet(
    "/",
    context =>
    {
        context.Response.Redirect("/swagger/index.html");
        return Task.CompletedTask;
    }
);

app.UseSwaggerUI();

app.Run();
