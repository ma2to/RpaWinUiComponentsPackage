using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RpaWinUiComponentsPackage.LoggerComponent
{
    internal class LoggerConfiguration
    {
        public string FolderPath { get; set; } = "";
        public string BaseFileName { get; set; } = "";
        public int MaxFileSizeMB { get; set; }
        public bool EnableRotation { get; set; }
    }
}
