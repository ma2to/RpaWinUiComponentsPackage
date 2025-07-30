// LoggerComponent/Configuration/LoggerConfiguration.cs - ✅ OPRAVENÝ namespace
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RpaWinUiComponentsPackage.Logger  // ✅ CHANGED: LoggerComponent -> Logger
{
    /// <summary>
    /// Konfigurácia pre LoggerComponent - INTERNAL
    /// </summary>
    internal class LoggerConfiguration
    {
        public string FolderPath { get; set; } = "";
        public string BaseFileName { get; set; } = "";
        public int MaxFileSizeMB { get; set; }
        public bool EnableRotation { get; set; }
    }
}