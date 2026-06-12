// Archivo: Controllers/ChequeController.cs
using Microsoft.AspNetCore.Mvc;
using ApertaChequeAPI.Services;

namespace ApertaChequeAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ChequeController : ControllerBase
{
    private readonly ChequeProcessor _processor;

    public ChequeController()
    {
        _processor = new ChequeProcessor();
    }

    [HttpPost("procesar")]
    public IActionResult ProcesarTodos()
    {
        var resultado = _processor.ProcesarTodosLosArchivos();
        return Ok(resultado);
    }

    [HttpGet("estado")]
    public IActionResult GetEstado()
    {
        var estado = _processor.ObtenerEstado();
        return Ok(estado);
    }

    [HttpGet("ping")]
    public IActionResult Ping()
    {
        return Ok(new { status = "ok", timestamp = DateTime.Now });
    }
}