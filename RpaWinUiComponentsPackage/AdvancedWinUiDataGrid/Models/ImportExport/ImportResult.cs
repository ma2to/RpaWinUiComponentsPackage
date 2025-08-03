// Models/ImportResult.cs - ✅ PUBLIC model pre Import results
using System;
using System.Collections.Generic;
using System.Linq;

namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Models.ImportExport
{
    /// <summary>
    /// Výsledok import operácie - ✅ PUBLIC API
    /// </summary>
    public class ImportResult
    {
        /// <summary>
        /// Unique ID import operácie
        /// </summary>
        public string ImportId { get; set; } = Guid.NewGuid().ToString("N")[..8];

        /// <summary>
        /// Cesta k importovanému súboru
        /// </summary>
        public string FilePath { get; set; } = string.Empty;

        /// <summary>
        /// Formát importovaného súboru
        /// </summary>
        public ExportFormat Format { get; set; }

        /// <summary>
        /// Či bol import úspešný
        /// </summary>
        public bool IsSuccessful { get; set; } = true;

        /// <summary>
        /// Celkový počet riadkov v súbore
        /// </summary>
        public int TotalRowsInFile { get; set; } = 0;

        /// <summary>
        /// Počet úspešne importovaných riadkov
        /// </summary>
        public int SuccessfullyImportedRows { get; set; } = 0;

        /// <summary>
        /// Počet preskočených riadkov (prázdne, headers)
        /// </summary>
        public int SkippedRows { get; set; } = 0;

        /// <summary>
        /// Počet riadkov s chybami
        /// </summary>
        public int ErrorRows { get; set; } = 0;

        /// <summary>
        /// Počet stĺpcov v súbore
        /// </summary>
        public int ColumnsCount { get; set; } = 0;

        /// <summary>
        /// Veľkosť súboru v bytoch
        /// </summary>
        public long FileSizeBytes { get; set; } = 0;

        /// <summary>
        /// Doba trvania importu v ms
        /// </summary>
        public double DurationMs { get; set; } = 0;

        /// <summary>
        /// Čas začiatku importu
        /// </summary>
        public DateTime StartTime { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Čas ukončenia importu
        /// </summary>
        public DateTime EndTime { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Import chyby
        /// </summary>
        public List<ImportError> Errors { get; set; } = new();

        /// <summary>
        /// Import warnings
        /// </summary>
        public List<ImportWarning> Warnings { get; set; } = new();

        /// <summary>
        /// Detailné štatistiky importu
        /// </summary>
        public ImportStatistics Statistics { get; set; } = new();

        /// <summary>
        /// Importované dáta
        /// </summary>
        public List<Dictionary<string, object?>> ImportedData { get; set; } = new();

        /// <summary>
        /// Úspešnosť importu v percentách
        /// </summary>
        public double SuccessRate => TotalRowsInFile > 0 ? (double)SuccessfullyImportedRows / TotalRowsInFile * 100 : 0;

        /// <summary>
        /// Má vážne chyby
        /// </summary>
        public bool HasCriticalErrors => Errors.Any(e => e.Severity == ErrorSeverity.Critical);

        /// <summary>
        /// Má warnings
        /// </summary>
        public bool HasWarnings => Warnings.Any();

        /// <summary>
        /// Processing rate v riadkoch/sec
        /// </summary>
        public double ProcessingRate => DurationMs > 0 ? TotalRowsInFile / (DurationMs / 1000.0) : 0;

        /// <summary>
        /// Pridá chybu
        /// </summary>
        public void AddError(string message, int rowIndex = -1, string? columnName = null, ErrorSeverity severity = ErrorSeverity.Error)
        {
            var error = new ImportError
            {
                Message = message,
                RowIndex = rowIndex,
                ColumnName = columnName,
                Severity = severity,
                Timestamp = DateTime.UtcNow
            };

            Errors.Add(error);

            if (!IsSuccessful && severity == ErrorSeverity.Critical)
            {
                IsSuccessful = false;
            }
        }

        /// <summary>
        /// Pridá warning
        /// </summary>
        public void AddWarning(string message, int rowIndex = -1, string? columnName = null)
        {
            var warning = new ImportWarning
            {
                Message = message,
                RowIndex = rowIndex,
                ColumnName = columnName,
                Timestamp = DateTime.UtcNow
            };

            Warnings.Add(warning);
        }

        /// <summary>
        /// Ukončí import a vypočíta finálne štatistiky
        /// </summary>
        public void FinalizeImport()
        {
            EndTime = DateTime.UtcNow;
            DurationMs = (EndTime - StartTime).TotalMilliseconds;
            
            // Update statistics
            Statistics.TotalDurationMs = DurationMs;
            Statistics.ProcessingRateRowsPerSecond = ProcessingRate;
            Statistics.FileSizeMB = FileSizeBytes / 1024.0 / 1024.0;
            Statistics.ErrorRate = TotalRowsInFile > 0 ? (double)ErrorRows / TotalRowsInFile * 100 : 0;
            Statistics.DataCompressionRatio = FileSizeBytes > 0 ? (double)ImportedData.Count / FileSizeBytes * 1000 : 0;

            // Determine final success status
            if (HasCriticalErrors)
            {
                IsSuccessful = false;
            }
            else if (ErrorRows > SuccessfullyImportedRows)
            {
                IsSuccessful = false;
            }
        }

        /// <summary>
        /// Získa summary report
        /// </summary>
        public string GetSummaryReport()
        {
            var status = IsSuccessful ? "SUCCESS" : "FAILED";
            var report = $"Import {status} - File: {FilePath}, Format: {Format}\n";
            report += $"Processed: {TotalRowsInFile} rows, Successful: {SuccessfullyImportedRows} ({SuccessRate:F1}%)\n";
            report += $"Skipped: {SkippedRows}, Errors: {ErrorRows}, Warnings: {Warnings.Count}\n";
            report += $"Duration: {DurationMs:F0}ms, Rate: {ProcessingRate:F0} rows/sec\n";

            if (Errors.Any())
            {
                report += $"Top Errors: {string.Join("; ", Errors.Take(3).Select(e => e.Message))}\n";
            }

            if (Warnings.Any())
            {
                report += $"Top Warnings: {string.Join("; ", Warnings.Take(3).Select(w => w.Message))}\n";
            }

            return report;
        }

        public override string ToString()
        {
            return $"Import {ImportId}: {SuccessfullyImportedRows}/{TotalRowsInFile} rows, " +
                   $"{(IsSuccessful ? "SUCCESS" : "FAILED")}, {DurationMs:F0}ms";
        }
    }

    /// <summary>
    /// Import chyba
    /// </summary>
    public class ImportError
    {
        public string Message { get; set; } = string.Empty;
        public int RowIndex { get; set; } = -1;
        public string? ColumnName { get; set; }
        public ErrorSeverity Severity { get; set; } = ErrorSeverity.Error;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        public override string ToString()
        {
            var location = RowIndex >= 0 
                ? !string.IsNullOrEmpty(ColumnName) 
                    ? $"Row {RowIndex}, Column {ColumnName}" 
                    : $"Row {RowIndex}"
                : "Unknown location";

            return $"{Severity}: {Message} ({location})";
        }
    }

    /// <summary>
    /// Import warning
    /// </summary>
    public class ImportWarning
    {
        public string Message { get; set; } = string.Empty;
        public int RowIndex { get; set; } = -1;
        public string? ColumnName { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        public override string ToString()
        {
            var location = RowIndex >= 0 
                ? !string.IsNullOrEmpty(ColumnName) 
                    ? $"Row {RowIndex}, Column {ColumnName}" 
                    : $"Row {RowIndex}"
                : "Unknown location";

            return $"Warning: {Message} ({location})";
        }
    }

    /// <summary>
    /// Import štatistiky
    /// </summary>
    public class ImportStatistics
    {
        public double TotalDurationMs { get; set; } = 0;
        public double ProcessingRateRowsPerSecond { get; set; } = 0;
        public double FileSizeMB { get; set; } = 0;
        public double ErrorRate { get; set; } = 0;
        public double DataCompressionRatio { get; set; } = 0;
        public Dictionary<string, int> ColumnTypeDistribution { get; set; } = new();
        public Dictionary<string, int> ErrorTypeDistribution { get; set; } = new();
        
        public override string ToString()
        {
            return $"Duration: {TotalDurationMs:F0}ms, Rate: {ProcessingRateRowsPerSecond:F0} rows/sec, " +
                   $"Size: {FileSizeMB:F2}MB, ErrorRate: {ErrorRate:F1}%";
        }
    }

    /// <summary>
    /// Závažnosť chyby
    /// </summary>
    public enum ErrorSeverity
    {
        Info,
        Warning,
        Error,
        Critical
    }
}