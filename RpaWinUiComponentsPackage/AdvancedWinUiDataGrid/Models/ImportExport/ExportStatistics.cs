// Models/ExportStatistics.cs - ✅ NOVÝ: Model pre export štatistiky
using System;
using System.Collections.Generic;

namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Models.ImportExport
{
    /// <summary>
    /// Model pre export štatistiky - INTERNAL
    /// Obsahuje informácie o export operácii
    /// </summary>
    internal class ExportStatistics
    {
        #region Properties

        /// <summary>
        /// Celkový počet riadkov v DataGrid
        /// </summary>
        public int TotalRows { get; set; }

        /// <summary>
        /// Počet validných riadkov
        /// </summary>
        public int ValidRows { get; set; }

        /// <summary>
        /// Počet nevalidných riadkov
        /// </summary>
        public int InvalidRows { get; set; }

        /// <summary>
        /// Počet prázdnych riadkov
        /// </summary>
        public int EmptyRows { get; set; }

        /// <summary>
        /// Počet exportovaných riadkov
        /// </summary>
        public int ExportedRows { get; set; }

        /// <summary>
        /// Počet stĺpcov
        /// </summary>
        public int TotalColumns { get; set; }

        /// <summary>
        /// Počet exportovaných stĺpcov
        /// </summary>
        public int ExportedColumns { get; set; }

        /// <summary>
        /// Celkový počet buniek
        /// </summary>
        public int TotalCells { get; set; }

        /// <summary>
        /// Počet exportovaných buniek
        /// </summary>
        public int ExportedCells { get; set; }

        /// <summary>
        /// Veľkosť exportovaných dát (v bajtoch)
        /// </summary>
        public long ExportedDataSize { get; set; }

        /// <summary>
        /// Trvanie export operácie
        /// </summary>
        public TimeSpan ExportDuration { get; set; }

        /// <summary>
        /// Typ exportu (CSV, JSON, Excel, etc.)
        /// </summary>
        public string ExportFormat { get; set; } = string.Empty;

        /// <summary>
        /// Časová pečiatka exportu
        /// </summary>
        public DateTime ExportTimestamp { get; set; } = DateTime.Now;

        /// <summary>
        /// Export bol úspešný
        /// </summary>
        public bool IsSuccessful { get; set; }

        /// <summary>
        /// Chybová správa (ak export nebol úspešný)
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// Štatistiky stĺpcov
        /// </summary>
        public Dictionary<string, ColumnStatistics> ColumnStatistics { get; set; } = new();

        #endregion

        #region Calculated Properties

        /// <summary>
        /// Percentuálny podiel validných riadkov
        /// </summary>
        public double ValidRowsPercentage => TotalRows > 0 ? (ValidRows * 100.0) / TotalRows : 0;

        /// <summary>
        /// Percentuálny podiel nevalidných riadkov
        /// </summary>
        public double InvalidRowsPercentage => TotalRows > 0 ? (InvalidRows * 100.0) / TotalRows : 0;

        /// <summary>
        /// Percentuálny podiel prázdnych riadkov
        /// </summary>
        public double EmptyRowsPercentage => TotalRows > 0 ? (EmptyRows * 100.0) / TotalRows : 0;

        /// <summary>
        /// Export rýchlosť (riadky za sekundu)
        /// </summary>
        public double ExportSpeed => ExportDuration.TotalSeconds > 0 ? ExportedRows / ExportDuration.TotalSeconds : 0;

        /// <summary>
        /// Dátová efektívnosť (exportované bunky / celkové bunky)
        /// </summary>
        public double DataEfficiency => TotalCells > 0 ? (ExportedCells * 100.0) / TotalCells : 0;

        #endregion

        #region Factory Methods

        /// <summary>
        /// Vytvorí prázdne štatistiky
        /// </summary>
        public static ExportStatistics CreateEmpty()
        {
            return new ExportStatistics
            {
                IsSuccessful = false,
                ExportFormat = "Unknown"
            };
        }

        /// <summary>
        /// Vytvorí úspešné štatistiky
        /// </summary>
        public static ExportStatistics CreateSuccessful(
            int totalRows, 
            int validRows, 
            int invalidRows, 
            int emptyRows,
            int exportedRows,
            int totalColumns,
            int exportedColumns,
            string format,
            TimeSpan duration,
            long dataSize = 0)
        {
            return new ExportStatistics
            {
                TotalRows = totalRows,
                ValidRows = validRows,
                InvalidRows = invalidRows,
                EmptyRows = emptyRows,
                ExportedRows = exportedRows,
                TotalColumns = totalColumns,
                ExportedColumns = exportedColumns,
                TotalCells = totalRows * totalColumns,
                ExportedCells = exportedRows * exportedColumns,
                ExportedDataSize = dataSize,
                ExportDuration = duration,
                ExportFormat = format,
                IsSuccessful = true
            };
        }

        /// <summary>
        /// Vytvorí neúspešné štatistiky s chybou
        /// </summary>
        public static ExportStatistics CreateFailed(string errorMessage, string format = "Unknown")
        {
            return new ExportStatistics
            {
                IsSuccessful = false,
                ErrorMessage = errorMessage,
                ExportFormat = format
            };
        }

        #endregion

        #region Methods

        /// <summary>
        /// Diagnostické informácie
        /// </summary>
        public string GetDiagnosticInfo()
        {
            if (!IsSuccessful)
            {
                return $"FAILED Export ({ExportFormat}) - Error: {ErrorMessage}";
            }

            return $"SUCCESS Export ({ExportFormat}) - " +
                   $"Rows: {ExportedRows}/{TotalRows} ({ValidRowsPercentage:F1}% valid), " +
                   $"Duration: {ExportDuration.TotalMilliseconds:F0}ms, " +
                   $"Speed: {ExportSpeed:F1} rows/sec, " +
                   $"Size: {ExportedDataSize:N0} bytes";
        }

        /// <summary>
        /// Súhrn exportu
        /// </summary>
        public string GetSummary()
        {
            if (!IsSuccessful)
            {
                return $"Export failed: {ErrorMessage}";
            }

            return $"Exported {ExportedRows} rows ({ValidRows} valid, {InvalidRows} invalid, {EmptyRows} empty) " +
                   $"to {ExportFormat} in {ExportDuration.TotalSeconds:F1}s";
        }

        #endregion

        #region ToString

        public override string ToString()
        {
            return GetDiagnosticInfo();
        }

        #endregion
    }

    /// <summary>
    /// Štatistiky pre jednotlivý stĺpec
    /// </summary>
    internal class ColumnStatistics
    {
        /// <summary>
        /// Názov stĺpca
        /// </summary>
        public string ColumnName { get; set; } = string.Empty;

        /// <summary>
        /// Počet hodnôt v stĺpci
        /// </summary>
        public int ValueCount { get; set; }

        /// <summary>
        /// Počet prázdnych hodnôt
        /// </summary>
        public int EmptyCount { get; set; }

        /// <summary>
        /// Počet null hodnôt
        /// </summary>
        public int NullCount { get; set; }

        /// <summary>
        /// Počet unikátnych hodnôt
        /// </summary>
        public int UniqueCount { get; set; }

        /// <summary>
        /// Typ dát v stĺpci
        /// </summary>
        public string DataType { get; set; } = "Unknown";

        /// <summary>
        /// Minimálna dĺžka hodnoty
        /// </summary>
        public int MinLength { get; set; }

        /// <summary>
        /// Maximálna dĺžka hodnoty
        /// </summary>
        public int MaxLength { get; set; }

        /// <summary>
        /// Priemerná dĺžka hodnoty
        /// </summary>
        public double AverageLength { get; set; }

        /// <summary>
        /// Či bol stĺpec exportovaný
        /// </summary>
        public bool IsExported { get; set; } = true;

        /// <summary>
        /// Diagnostický popis stĺpca
        /// </summary>
        public override string ToString()
        {
            return $"{ColumnName}: {ValueCount} values, {EmptyCount} empty, {UniqueCount} unique ({DataType})";
        }
    }
}