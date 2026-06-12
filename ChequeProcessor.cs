// Archivo: Services/ChequeProcessor.cs
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using ApertaChequeAPI.Models;

namespace ApertaChequeAPI.Services
{
    public class ChequeProcessor
    {
        private Config _config;
        private DevolCods _devolCods;
        private List<string> _logs;

        public ChequeProcessor()
        {
            _logs = new List<string>();
            _config = new Config();
            _devolCods = new DevolCods();
            CargarConfiguracion();
            CargarParametros();
        }

        private void CargarConfiguracion()
        {
            string configPath = Path.Combine(Directory.GetCurrentDirectory(), "Config", "config.xml");
            
            // Si existe el archivo, cargarlo
            if (File.Exists(configPath))
            {
                try
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(Config));
                    using (FileStream fs = new FileStream(configPath, FileMode.Open))
                    {
                        _config = (Config)serializer.Deserialize(fs) ?? new Config();
                    }
                }
                catch (Exception ex)
                {
                    AgregarLog($"Error cargando configuración: {ex.Message}");
                }
            }
            
            AgregarLog($"Configuración cargada: PathImport={_config.PathImport}");
        }

        private void CargarParametros()
        {
            string paramsPath = Path.Combine(Directory.GetCurrentDirectory(), "Config", "Params.xml");
            
            if (File.Exists(paramsPath))
            {
                try
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(DevolCods));
                    using (FileStream fs = new FileStream(paramsPath, FileMode.Open))
                    {
                        _devolCods = (DevolCods)serializer.Deserialize(fs) ?? new DevolCods();
                    }
                }
                catch (Exception ex)
                {
                    AgregarLog($"Error cargando parámetros: {ex.Message}");
                }
            }
            
            // Si no hay datos, agregar códigos por defecto
            if (_devolCods._DetDev.Count == 0)
            {
                _devolCods._DetDev.Add(new DetDev { CodOri = "1", CodNew = "1", Mensaje = "No Tiene Suficientes Fondos" });
                _devolCods._DetDev.Add(new DetDev { CodOri = "2", CodNew = "2", Mensaje = "No Tiene Cuenta" });
                _devolCods._DetDev.Add(new DetDev { CodOri = "3", CodNew = "3", Mensaje = "Cuenta Cerrada" });
            }
            
            AgregarLog($"Parámetros cargados: {_devolCods._DetDev.Count} códigos");
        }

        public ProcesamientoResultado ProcesarTodosLosArchivos()
        {
            var resultado = new ProcesamientoResultado
            {
                Success = true,
                FechaProceso = DateTime.Now
            };

            try
            {
                if (!Directory.Exists(_config.PathImport))
                {
                    resultado.Success = false;
                    resultado.Message = $"Carpeta no existe: {_config.PathImport}";
                    return resultado;
                }

                var archivos = Directory.GetFiles(_config.PathImport, $"*.{_config.DevFileExt}");
                
                if (archivos.Length == 0)
                {
                    resultado.Message = "No hay archivos para procesar";
                    resultado.Logs.Add("No se encontraron archivos");
                    return resultado;
                }

                resultado.ArchivosProcesados = archivos.Length;

                foreach (var archivo in archivos)
                {
                    var resultadoArchivo = ProcesarArchivo(archivo);
                    resultado.RegistrosProcesados += resultadoArchivo.RegistrosProcesados;
                    resultado.DevolucionesGeneradas += resultadoArchivo.DevolucionesGeneradas;
                    resultado.ArchivoSalida = resultadoArchivo.ArchivoSalida;
                    resultado.Logs.AddRange(resultadoArchivo.Logs);
                }

                resultado.Message = $"Procesado: {resultado.RegistrosProcesados} registros, {resultado.DevolucionesGeneradas} devoluciones";
            }
            catch (Exception ex)
            {
                resultado.Success = false;
                resultado.Message = ex.Message;
                resultado.Logs.Add($"ERROR: {ex.Message}");
            }

            resultado.Logs = _logs;
            return resultado;
        }

        private ProcesamientoResultado ProcesarArchivo(string rutaArchivo)
        {
            var resultado = new ProcesamientoResultado { Success = true, FechaProceso = DateTime.Now };
            
            try
            {
                string nombreArchivo = Path.GetFileName(rutaArchivo);
                AgregarLog($"Procesando: {nombreArchivo}");

                var lineas = File.ReadAllLines(rutaArchivo, Encoding.Default);
                resultado.RegistrosProcesados = lineas.Length;
                resultado.Logs.Add($"Archivo leído: {lineas.Length} líneas");

                var lineasProcesadas = new List<string>();
                int devoluciones = 0;

                foreach (var linea in lineas)
                {
                    if (string.IsNullOrWhiteSpace(linea)) continue;
                    
                    string codigo = ExtraerCodigoDevolucion(linea);
                    var mapeo = _devolCods._DetDev.FirstOrDefault(x => x.CodOri == codigo);
                    
                    if (mapeo != null)
                    {
                        lineasProcesadas.Add($"{linea}|{mapeo.CodNew}|{mapeo.Mensaje}");
                        devoluciones++;
                    }
                    else if (!string.IsNullOrEmpty(codigo))
                    {
                        lineasProcesadas.Add($"{linea}|00|CODIGO NO MAPEADO: {codigo}");
                    }
                    else
                    {
                        lineasProcesadas.Add(linea);
                    }
                }

                resultado.DevolucionesGeneradas = devoluciones;
                resultado.Logs.Add($"Devoluciones encontradas: {devoluciones}");

                // Guardar archivo de salida
                string nombreSalida = _config.FnDevolucion;
                if (_config.AddDateToReturnPath)
                {
                    nombreSalida = nombreSalida.Replace(".txt", $" {DateTime.Now:dd.MM.yyyy}.txt");
                }
                
                string rutaSalida = Path.Combine(_config.PathDev, nombreSalida);
                Directory.CreateDirectory(_config.PathDev);
                File.WriteAllLines(rutaSalida, lineasProcesadas, Encoding.Default);
                resultado.ArchivoSalida = nombreSalida;
                
                AgregarLog($"Archivo generado: {nombreSalida}");

                // Mover archivo procesado
                if (_config.RenameProcessedFile)
                {
                    string carpetaProc = Path.Combine(_config.PathDev, "Procesados");
                    Directory.CreateDirectory(carpetaProc);
                    string nuevoNombre = $"{Path.GetFileNameWithoutExtension(nombreArchivo)}_PROC_{DateTime.Now:yyyyMMddHHmmss}{Path.GetExtension(nombreArchivo)}";
                    File.Move(rutaArchivo, Path.Combine(carpetaProc, nuevoNombre));
                    AgregarLog($"Archivo original movido a Procesados/");
                }
            }
            catch (Exception ex)
            {
                resultado.Success = false;
                resultado.Message = ex.Message;
                resultado.Logs.Add($"ERROR: {ex.Message}");
            }

            return resultado;
        }

        private string ExtraerCodigoDevolucion(string linea)
        {
            // Extraer código de posición fija (ejemplo)
            if (linea.Length >= 82)
            {
                string codigo = linea.Substring(80, 2).Trim();
                if (codigo.All(char.IsDigit) && !string.IsNullOrEmpty(codigo))
                    return codigo;
            }
            return "";
        }

        private void AgregarLog(string mensaje)
        {
            string logLine = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {mensaje}";
            _logs.Add(logLine);
            
            string logPath = Path.Combine(Directory.GetCurrentDirectory(), "Logs", $"log_{DateTime.Now:yyyyMMdd}.txt");
            Directory.CreateDirectory(Path.GetDirectoryName(logPath));
            File.AppendAllText(logPath, logLine + Environment.NewLine, Encoding.UTF8);
        }

        public object ObtenerEstado()
        {
            return new
            {
                config = new { _config.PathImport, _config.PathDev, _config.FnDevolucion },
                carpetas = new
                {
                    ImportExists = Directory.Exists(_config.PathImport),
                    ImportFiles = Directory.Exists(_config.PathImport) ? 
                        Directory.GetFiles(_config.PathImport, $"*.{_config.DevFileExt}").Length : 0
                },
                logs = _logs.TakeLast(10).ToList(),
                codigosMapeados = _devolCods._DetDev.Count
            };
        }
    }
}