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

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .Enrich.FromLogContext()
    .CreateLogger();

// Agregar Serilog al builder
builder.Host.UseSerilog();

// Add services to the container.

builder.Services.AddControllers(m =>
{
    m.UseCoreIntegrated();
    m.UsePowerBuilderIntegrated();
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();



builder.Services.AddGzipCompression(CompressionLevel.Fastest);
builder.Services.AddHttpContextAccessor();

//Seleccción de Base de datos a Conectar.
//builder.Services.AddDataContext<DefaultDataContext>(m => m.UseSqlServer(builder.Configuration, "PersonDemo02"));
//builder.Services.AddDataContext<DefaultDataContext>(m => m.UseSqlServer(builder.Configuration, "PBDemoDB2022"));


builder.Services.AddScoped<DataContextFactory>();
builder.Services.AddScoped<ISqlExecutorService, SqlExecutorService>();
builder.Services.AddScoped<IDatawindowService, DatawindowService>();

// Registrar un factory para resolver el contexto adecuado
builder.Services.AddScoped(provider =>
{
    var factory = provider.GetRequiredService<DataContextFactory>();
    return factory.GetDataContext();
});



var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    // Usar Serilog para solicitudes HTTP
    app.UseSerilogRequestLogging();

    Trace.Listeners.Add(new TextWriterTraceListener(Console.Error));
    Trace.AutoFlush = true;
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.UseResponseCompression();

app.UseDataWindow();

app.UseStatusCodePages(async context =>
{
    if (context.HttpContext.Response.StatusCode == 404)
    {
        context.HttpContext.Response.ContentType = "application/json";
        await context.HttpContext.Response.WriteAsync("""{"error":"Endpoint no encontrado"}""");
    }
});

app.MapGet("/api", () => """Mi "Power Server"... Porque las malas prácticas a veces molan!""");


app.Run();