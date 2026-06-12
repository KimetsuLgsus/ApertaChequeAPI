// Archivo: Program.cs
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.IO;

# Crear un nuevo Program.cs con puertos fijos
@'
var builder = WebApplication.CreateBuilder(args);

// Configurar puertos específicos (evitar conflictos)
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(8080);  // Puerto HTTP 8080 (casi siempre libre)
});

builder.Services.AddCors();
builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

app.UseCors(x => x.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());

app.MapGet("/ping", () => Results.Ok(new { 
    status = "ok", 
    message = "Servidor funcionando en puerto 8080",
    timestamp = DateTime.Now 
}));

app.MapGet("/estado", () => Results.Ok(new { 
    running = true,
    port = 8080,
    timestamp = DateTime.Now 
}));

app.MapPost("/procesar", () => Results.Ok(new { 
    success = true, 
    message = "Procesamiento simulado",
    timestamp = DateTime.Now 
}));

Console.WriteLine("✅ Servidor iniciado en: http://localhost:8080");
Console.WriteLine("📡 Endpoints:");
Console.WriteLine("   GET  http://localhost:8080/ping");
Console.WriteLine("   GET  http://localhost:8080/estado");
Console.WriteLine("   POST http://localhost:8080/procesar");

app.Run();
'@ | Out-File -FilePath Program.cs -Encoding UTF8 -Force

Write-Host "✅ Program.cs actualizado para usar puerto 8080" -ForegroundColor Green