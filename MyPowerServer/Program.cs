using MyPowerServer.Services.Impl;
using SnapObjects.Data;
using DWNet.Data.AspNetCore;
using SnapObjects.Data.AspNetCore;
using SnapObjects.Data.SqlServer;
using System.IO.Compression;
using MyPowerServer;
using MyPowerServer.Services;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Diagnostics;
using Serilog;

// ─────────────────────────────────────────────────────────────────────────────
// Program.cs — arranque del "Power Server" casero. La idea de la charla: replicar
// a mano lo que hace PowerServer de Appeon. El cliente PowerBuilder nos manda la
// SINTAXIS de una DataWindow + los datos en JSON; aquí montamos un DataStore con
// los SDK de Appeon (DWNet / SnapObjects) y hacemos Retrieve/Update contra SQL Server.
// Mismo modelo "minimal hosting" de ASP.NET Core que en SecurityApi: builder →
// registro de servicios → pipeline HTTP → app.Run().
// ─────────────────────────────────────────────────────────────────────────────

var builder = WebApplication.CreateBuilder(args);

// Serilog: logging estructurado a consola. Lo dejamos a nivel Information.
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .Enrich.FromLogContext()
    .CreateLogger();

// Enganchamos Serilog como logger del host (sustituye al logging por defecto).
builder.Host.UseSerilog();

// Add services to the container.

// Controladores + integración de los SDK de Appeon. Estas dos llamadas son LA CLAVE
// para que MVC sepa serializar/deserializar DataStores y entender el "puente"
// PowerScript ↔ .NET:
//   - UseCoreIntegrated(): infraestructura base de SnapObjects.
//   - UsePowerBuilderIntegrated(): comportamiento compatible con PowerBuilder.
builder.Services.AddControllers(m =>
{
    m.UseCoreIntegrated();
    m.UsePowerBuilderIntegrated();
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Compresión GZip de las respuestas (los DataStores en JSON pueden ser grandes).
builder.Services.AddGzipCompression(CompressionLevel.Fastest);

// Necesario para que la fábrica de contextos pueda leer la cabecera HTTP 'profile'.
builder.Services.AddHttpContextAccessor();

// Selección de Base de datos a Conectar.
// Estas dos líneas (comentadas) serían la forma "normal" de fijar UNA sola BD.
// En su lugar usamos una fábrica que elige la conexión por petición (ver más abajo).
//builder.Services.AddDataContext<DefaultDataContext>(m => m.UseSqlServer(builder.Configuration, "PersonDemo02"));
//builder.Services.AddDataContext<DefaultDataContext>(m => m.UseSqlServer(builder.Configuration, "PBDemoDB2022"));

// Registro de servicios (todos Scoped: una instancia por petición HTTP).
builder.Services.AddScoped<DataContextFactory>();
builder.Services.AddScoped<ISqlExecutorService, SqlExecutorService>();
builder.Services.AddScoped<IDatawindowService, DatawindowService>();

// Aquí está el truco multi-base de datos: en vez de registrar un DefaultDataContext
// fijo, registramos una FÁBRICA. Cada vez que un servicio pide un DefaultDataContext,
// la fábrica mira la cabecera 'profile' de la petición y devuelve la conexión adecuada.
builder.Services.AddScoped(provider =>
{
    var factory = provider.GetRequiredService<DataContextFactory>();
    return factory.GetDataContext();
});

var app = builder.Build();

// ── Pipeline HTTP (el orden importa) ─────────────────────────────────────────
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    // Log de cada petición HTTP con Serilog.
    app.UseSerilogRequestLogging();

    // Volcamos las trazas de Trace.* a la consola de error (útil para depurar los SDK).
    Trace.Listeners.Add(new TextWriterTraceListener(Console.Error));
    Trace.AutoFlush = true;
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

// Activa la compresión registrada arriba.
app.UseResponseCompression();

// Middleware de Appeon: habilita el soporte DataWindow en el pipeline (serialización
// de DataStores, etc.). Sin esto, los endcaps DataWindow no funcionan.
app.UseDataWindow();

// Si ninguna ruta casa (404), devolvemos un JSON propio en vez de la página por defecto.
// Fijaos en las cadenas """..."": son "raw string literals", permiten meter comillas
// dobles dentro sin escaparlas. Muy cómodo para JSON embebido.
app.UseStatusCodePages(async context =>
{
    if (context.HttpContext.Response.StatusCode == 404)
    {
        context.HttpContext.Response.ContentType = "application/json";
        await context.HttpContext.Response.WriteAsync("""{"error":"Endpoint no encontrado"}""");
    }
});

// Endpoint mínimo de cortesía en la raíz de la API (sin controlador, "minimal API").
app.MapGet("/api", () => """Mi "Power Server"... Porque las malas prácticas a veces molan!""");

app.Run();
