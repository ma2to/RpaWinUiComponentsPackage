// Services/Interfaces/IExportService.cs
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace RpaWinUiComponents.AdvancedWinUiDataGrid
{
    /// <summary>
    /// Rozhranie pre export funkcionalitu DataGrid komponentu.
    /// Podporuje export do DataTable, CSV, Excel a iných formátov.
    /// </summary>
    internal interface IExportService : IDisposable
    {
        #region Events

        /// <summary>
        /// Event vyvolaný keď sa zmení progres exportu.
        /// </summary>
        event EventHandler<ExportProgressEventArgs>? ExportProgressChanged;

        /// <summary>
        /// Event vyvolaný keď sa dokončí export.
        /// </summary>
        event EventHandler<ExportCompletedEventArgs>? ExportCompleted;

        #endregion

        #region Inicializácia

        /// <summary>
        /// Inicializuje službu s konfiguráciou gridu.
        /// </summary>
        /// <param name="configuration">Konfigurácia gridu</param>
        /// <returns>Task pre asynchrónnu inicializáciu</returns>
        Task InitializeAsync(GridConfiguration configuration);

        /// <summary>
        /// Určuje či je služba inicializovaná.
        /// </summary>
        bool IsInitialized { get; }

        #endregion

        #region Export do DataTable

        /// <summary>
        /// Exportuje všetky dáta do DataTable.
        /// Zahŕňa ValidAlerts stĺpec, ale NEZAHŔŇA DeleteRows stĺpec.
        /// </summary>
        /// <param name="includeEmptyRows">Zahrnúť prázdne riadky</param>
        /// <param name="cancellationToken">Token pre zrušenie</param>
        /// <returns>Task s DataTable obsahujúcou všetky dáta</returns>
        Task<DataTable> ExportToDataTableAsync(bool includeEmptyRows = false, CancellationToken cancellationToken = default);

        /// <summary>
        /// Exportuje iba validné riadky do DataTable.
        /// </summary>
        /// <param name="cancellationToken">Token pre zrušenie</param>
        /// <returns>Task s DataTable obsahujúcou iba validné dáta</returns>
        Task<DataTable> ExportValidRowsToDataTableAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Exportuje konkrétne riadky do DataTable.
        /// </summary>
        /// <param name="rowIndices">Indexy riadkov na export</param>
        /// <param name="cancellationToken">Token pre zrušenie</param>
        /// <returns>Task s DataTable obsahujúcou vybrané riadky</returns>
        Task<DataTable> ExportRowsToDataTableAsync(IEnumerable<int> rowIndices, CancellationToken cancellationToken = default);

        #endregion

        #region Export do CSV

        /// <summary>
        /// Exportuje dáta do CSV formátu.
        /// </summary>
        /// <param name="includeHeaders">Zahrnúť hlavičky stĺpcov</param>
        /// <param name="includeEmptyRows">Zahrnúť prázdne riadky</param>
        /// <param name="separator">Oddeľovač stĺpcov (default: čiarka)</param>
        /// <param name="cancellationToken">Token pre zrušenie</param>
        /// <returns>Task s CSV stringom</returns>
        Task<string> ExportToCsvAsync(bool includeHeaders = true, bool includeEmptyRows = false, string separator = ",", CancellationToken cancellationToken = default);

        #endregion

        #region Export do TSV (Excel kompatibilný)

        /// <summary>
        /// Exportuje dáta do TSV formátu (Excel kompatibilný).
        /// </summary>
        /// <param name="includeHeaders">Zahrnúť hlavičky stĺpcov</param>
        /// <param name="includeEmptyRows">Zahrnúť prázdne riadky</param>
        /// <param name="cancellationToken">Token pre zrušenie</param>
        /// <returns>Task s TSV stringom</returns>
        Task<string> ExportToTsvAsync(bool includeHeaders = true, bool includeEmptyRows = false, CancellationToken cancellationToken = default);

        #endregion

        #region Export metadát

        /// <summary>
        /// Exportuje metadáta gridu (konfigurácia, validačné pravidlá, atď.).
        /// </summary>
        /// <param name="cancellationToken">Token pre zrušenie</param>
        /// <returns>Task s JSON stringom obsahujúcim metadáta</returns>
        Task<string> ExportMetadataAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Exportuje štatistiky gridu.
        /// </summary>
        /// <param name="cancellationToken">Token pre zrušenie</param>
        /// <returns>Task s objektom obsahujúcim štatistiky</returns>
        Task<GridStatistics> ExportStatisticsAsync(CancellationToken cancellationToken = default);

        #endregion

        #region Parciálny export

        /// <summary>
        /// Exportuje konkrétne stĺpce do DataTable.
        /// </summary>
        /// <param name="columnNames">Názvy stĺpcov na export</param>
        /// <param name="includeEmptyRows">Zahrnúť prázdne riadky</param>
        /// <param name="cancellationToken">Token pre zrušenie</param>
        /// <returns>Task s DataTable obsahujúcou vybrané stĺpce</returns>
        Task<DataTable> ExportColumnsToDataTableAsync(IEnumerable<string> columnNames, bool includeEmptyRows = false, CancellationToken cancellationToken = default);

        /// <summary>
        /// Exportuje obdĺžnikovú oblasť buniek.
        /// </summary>
        /// <param name="startRow">Začiatočný riadok</param>
        /// <param name="startColumn">Začiatočný stĺpec</param>
        /// <param name="endRow">Koncový riadok</param>
        /// <param name="endColumn">Koncový stĺpec</param>
        /// <param name="cancellationToken">Token pre zrušenie</param>
        /// <returns>Task s DataTable obsahujúcou vybranú oblasť</returns>
        Task<DataTable> ExportRangeToDataTableAsync(int startRow, int startColumn, int endRow, int endColumn, CancellationToken cancellationToken = default);

        #endregion

        #region Konfigurácia exportu

        /// <summary>
        /// Konfigurácia pre export operácie.
        /// </summary>
        ExportConfiguration Configuration { get; set; }

        /// <summary>
        /// Nastaví custom formátovanie pre konkrétny stĺpec.
        /// </summary>
        /// <param name="columnName">Názov stĺpca</param>
        /// <param name="formatter">Funkcia pre formátovanie hodnôt</param>
        void SetColumnFormatter(string columnName, Func<object?, string> formatter);

        /// <summary>
        /// Odstraní custom formátovanie pre stĺpec.
        /// </summary>
        /// <param name="columnName">Názov stĺpca</param>
        void RemoveColumnFormatter(string columnName);

        #endregion

        #region Validácia

        /// <summary>
        /// Skontroluje či je možné exportovať do určeného formátu.
        /// </summary>
        /// <param name="format">Cieľový formát</param>
        /// <returns>Výsledok validácie</returns>
        ExportValidationResult ValidateExport(ExportFormat format);

        #endregion
    }

    #region Enums

    /// <summary>
    /// Podporované formáty exportu.
    /// </summary>
    internal enum ExportFormat
    {
        DataTable,
        Csv,
        Tsv,
        Json,
        Xml
    }

    #endregion

    #region Helper classes

    /// <summary>
    /// Konfigurácia pre export operácie.
    /// </summary>
    internal class ExportConfiguration
    {
        public bool IncludeHeaders { get; set; } = true;
        public bool IncludeEmptyRows { get; set; } = false;
        public bool IncludeValidationErrors { get; set; } = true;
        public bool IncludeSpecialColumns { get; set; } = false; // DeleteRows stĺpec
        public string CsvSeparator { get; set; } = ",";
        public string DateTimeFormat { get; set; } = "yyyy-MM-dd HH:mm:ss";
        public string DecimalFormat { get; set; } = "N2";
        public bool UseQuotesForStrings { get; set; } = true;
        public string NullValuePlaceholder { get; set; } = "";

        public static ExportConfiguration Default => new();
    }

    /// <summary>
    /// Štatistiky gridu.
    /// </summary>
    internal class GridStatistics
    {
        public int TotalRows { get; set; }
        public int NonEmptyRows { get; set; }
        public int ValidRows { get; set; }
        public int InvalidRows { get; set; }
        public int TotalColumns { get; set; }
        public int DataColumns { get; set; }
        public Dictionary<string, int> ValidationErrorsByColumn { get; set; } = new();
        public Dictionary<string, object> ColumnStatistics { get; set; } = new();
        public DateTime ExportTimestamp { get; set; } = DateTime.Now;

        public double ValidRowsPercentage => TotalRows > 0 ? (double)ValidRows / TotalRows * 100 : 0;
        public double DataDensity => (TotalRows * DataColumns) > 0 ? (double)GetNonEmptyCellsCount() / (TotalRows * DataColumns) * 100 : 0;

        private int GetNonEmptyCellsCount()
        {
            // Tento výpočet by sa mal implementovať v službe
            return NonEmptyRows * DataColumns; // Zjednodušený odhad
        }
    }

    /// <summary>
    /// Výsledok validácie exportu.
    /// </summary>
    internal class ExportValidationResult
    {
        public ExportValidationResult(bool isValid, string? errorMessage = null, List<string>? warnings = null)
        {
            IsValid = isValid;
            ErrorMessage = errorMessage;
            Warnings = warnings ?? new List<string>();
        }

        public bool IsValid { get; }
        public string? ErrorMessage { get; }
        public List<string> Warnings { get; }

        public static ExportValidationResult Valid(List<string>? warnings = null)
            => new(true, null, warnings);

        public static ExportValidationResult Invalid(string errorMessage)
            => new(false, errorMessage);
    }

    #endregion

    #region Event Args

    /// <summary>
    /// Event args pre progres exportu.
    /// </summary>
    internal class ExportProgressEventArgs : EventArgs
    {
        public ExportProgressEventArgs(int processedRows, int totalRows, string currentOperation)
        {
            ProcessedRows = processedRows;
            TotalRows = totalRows;
            CurrentOperation = currentOperation;
        }

        public int ProcessedRows { get; }
        public int TotalRows { get; }
        public string CurrentOperation { get; }
        public double ProgressPercentage => TotalRows > 0 ? (double)ProcessedRows / TotalRows * 100 : 0;
    }

    /// <summary>
    /// Event args pre dokončenie exportu.
    /// </summary>
    internal class ExportCompletedEventArgs : EventArgs
    {
        public ExportCompletedEventArgs(bool success, ExportFormat format, int exportedRows, string? errorMessage = null)
        {
            Success = success;
            Format = format;
            ExportedRows = exportedRows;
            ErrorMessage = errorMessage;
        }

        public bool Success { get; }
        public ExportFormat Format { get; }
        public int ExportedRows { get; }
        public string? ErrorMessage { get; }
    }

    #endregion
}