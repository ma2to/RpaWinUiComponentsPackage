// Models/ImportExportConfiguration.cs - ✅ PUBLIC konfigurácia pre Import/Export
using System;
using System.Collections.Generic;

namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Models
{
    /// <summary>
    /// Konfigurácia pre Import/Export operácie - ✅ PUBLIC API
    /// </summary>
    public class ImportExportConfiguration
    {
        /// <summary>
        /// Formát pre export/import
        /// </summary>
        public ExportFormat Format { get; set; } = ExportFormat.CSV;

        /// <summary>
        /// Včítať headers do exportu (default: true)
        /// </summary>
        public bool IncludeHeaders { get; set; } = true;

        /// <summary>
        /// Encoding pre súbory (default: UTF-8)
        /// </summary>
        public string Encoding { get; set; } = "UTF-8";

        /// <summary>
        /// Separator pre CSV súbory (default: ,)
        /// </summary>
        public string CsvSeparator { get; set; } = ",";

        /// <summary>
        /// Quote character pre CSV (default: ")
        /// </summary>
        public string CsvQuoteChar { get; set; } = "\"";

        /// <summary>
        /// Line ending type pre CSV
        /// </summary>
        public LineEndingType LineEnding { get; set; } = LineEndingType.Auto;

        /// <summary>
        /// Exportovať iba validné riadky (default: false)
        /// </summary>
        public bool ExportValidRowsOnly { get; set; } = false;

        /// <summary>
        /// Exportovať iba špecifické stĺpce
        /// </summary>
        public List<string> SpecificColumns { get; set; } = new();

        /// <summary>
        /// Použiť custom template pre export
        /// </summary>
        public string? TemplatePath { get; set; }

        /// <summary>
        /// Excel worksheet name (pre Excel export)
        /// </summary>
        public string ExcelWorksheetName { get; set; } = "DataGrid_Export";

        /// <summary>
        /// JSON formatting (pre JSON export)
        /// </summary>
        public JsonFormatting JsonFormatting { get; set; } = JsonFormatting.Indented;

        /// <summary>
        /// Maximálna veľkosť súboru v MB (0 = unlimited)
        /// </summary>
        public int MaxFileSizeMB { get; set; } = 0;

        /// <summary>
        /// Automaticky otvoriť súbor po exporte
        /// </summary>
        public bool AutoOpenFile { get; set; } = false;

        /// <summary>
        /// Backup existujúceho súboru pred prepísaním
        /// </summary>
        public bool BackupExistingFile { get; set; } = true;

        /// <summary>
        /// Pre import - preskočiť prázdne riadky
        /// </summary>
        public bool SkipEmptyRows { get; set; } = true;

        /// <summary>
        /// Pre import - validovať dáta počas importu
        /// </summary>
        public bool ValidateOnImport { get; set; } = true;

        /// <summary>
        /// Pre import - pokračovať aj pri chybách
        /// </summary>
        public bool ContinueOnErrors { get; set; } = false;

        /// <summary>
        /// Predvolené konfigurácie
        /// </summary>
        public static ImportExportConfiguration DefaultCsv => new()
        {
            Format = ExportFormat.CSV,
            IncludeHeaders = true,
            CsvSeparator = ",",
            Encoding = "UTF-8"
        };

        public static ImportExportConfiguration DefaultExcel => new()
        {
            Format = ExportFormat.Excel,
            IncludeHeaders = true,
            ExcelWorksheetName = "DataGrid_Export",
            AutoOpenFile = false
        };

        public static ImportExportConfiguration DefaultJson => new()
        {
            Format = ExportFormat.JSON,
            JsonFormatting = JsonFormatting.Indented,
            Encoding = "UTF-8"
        };

        public static ImportExportConfiguration ValidDataOnly => new()
        {
            Format = ExportFormat.CSV,
            ExportValidRowsOnly = true,
            IncludeHeaders = true,
            SkipEmptyRows = true
        };

        /// <summary>
        /// Validuje konfiguráciu
        /// </summary>
        public bool IsValid()
        {
            return !string.IsNullOrWhiteSpace(CsvSeparator) &&
                   !string.IsNullOrWhiteSpace(CsvQuoteChar) &&
                   !string.IsNullOrWhiteSpace(Encoding) &&
                   MaxFileSizeMB >= 0 &&
                   !string.IsNullOrWhiteSpace(ExcelWorksheetName);
        }

        /// <summary>
        /// Získa popis konfigurácie
        /// </summary>
        public string GetDescription()
        {
            var features = new List<string>();
            
            features.Add($"Format: {Format}");
            features.Add($"Headers: {(IncludeHeaders ? "Yes" : "No")}");
            features.Add($"Encoding: {Encoding}");
            
            if (Format == ExportFormat.CSV)
            {
                features.Add($"Separator: '{CsvSeparator}'");
            }
            else if (Format == ExportFormat.Excel)
            {
                features.Add($"Worksheet: {ExcelWorksheetName}");
            }
            else if (Format == ExportFormat.JSON)
            {
                features.Add($"Formatting: {JsonFormatting}");
            }

            if (ExportValidRowsOnly) features.Add("ValidOnly");
            if (SpecificColumns.Any()) features.Add($"Columns: {SpecificColumns.Count}");
            if (!string.IsNullOrEmpty(TemplatePath)) features.Add("Template");
            if (AutoOpenFile) features.Add("AutoOpen");

            return string.Join(", ", features);
        }

        public override string ToString()
        {
            return GetDescription();
        }
    }

    /// <summary>
    /// Export formáty
    /// </summary>
    public enum ExportFormat
    {
        CSV,
        Excel,
        JSON,
        XML,
        TSV
    }

    /// <summary>
    /// Line ending types
    /// </summary>
    public enum LineEndingType
    {
        Auto,
        Windows,  // CRLF
        Unix,     // LF
        Mac       // CR
    }

    /// <summary>
    /// JSON formatting options
    /// </summary>
    public enum JsonFormatting
    {
        Compact,
        Indented
    }
}