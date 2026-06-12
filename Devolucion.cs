// Archivo: Models/Devolucion.cs
using System.Collections.Generic;
using System.Xml.Serialization;

namespace ApertaChequeAPI.Models
{
    [XmlRoot("DevolCods")]
    public class DevolCods
    {
        [XmlArray("_DetDev")]
        [XmlArrayItem("DetDev")]
        public List<DetDev> _DetDev { get; set; } = new List<DetDev>();

        [XmlArray("_BusinnesDate")]
        [XmlArrayItem("BusDate")]
        public List<BusDate> _BusinnesDate { get; set; } = new List<BusDate>();
    }

    public class DetDev
    {
        public string CodOri { get; set; } = "";
        public string CodNew { get; set; } = "";
        public string Mensaje { get; set; } = "";
    }

    public class BusDate
    {
        public string Busdate { get; set; } = "";
        public int Run { get; set; }
        public int Din { get; set; }
    }

    public class ProcesamientoResultado
    {
        public bool Success { get; set; }
        public string Message { get; set; } = "";
        public int ArchivosProcesados { get; set; }
        public int RegistrosProcesados { get; set; }
        public int DevolucionesGeneradas { get; set; }
        public string ArchivoSalida { get; set; } = "";
        public DateTime FechaProceso { get; set; }
        public List<string> Logs { get; set; } = new List<string>();
    }
}