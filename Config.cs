// Archivo: Models/Config.cs
using System;

namespace ApertaChequeAPI.Models
{
    public class Config
    {
        public string Lastpath { get; set; } = @"C:\AiDPS\Archivos procesar";
        public string PathImport { get; set; } = @"\\SRVRAPERTA791\ECLIPSe\Data\Common\Import\Waiting";
        public string PathImg { get; set; } = @"C:\AiDPS\Imagenes";
        public string PathDev { get; set; } = @"C:\AiDPS\Archivos procesar";
        public string DevFileExt { get; set; } = "TXT";
        public string AppPath { get; set; } = @"C:\AiDPS\Programa de Importación\BNPPro1.exe";
        public DateTime SelDate { get; set; } = DateTime.Now;
        public int WorkSource { get; set; } = 11;
        public int Transport { get; set; } = 2;
        public int Site { get; set; } = 1;
        public int OriBank { get; set; } = 139;
        public string FnEntrante { get; set; } = "export.txt";
        public string FnDevolucion { get; set; } = "Cks Devueltos.txt";
        public bool BusDatefromfile { get; set; } = false;
        public bool RenameProcessedFile { get; set; } = true;
        public bool AutomaticProcess { get; set; } = false;
        public bool AddDateToIncleringPath { get; set; } = false;
        public bool AddDateToReturnPath { get; set; } = true;
        public bool ShortDin { get; set; } = true;
        public bool ImagenConPath { get; set; } = true;
    }
}