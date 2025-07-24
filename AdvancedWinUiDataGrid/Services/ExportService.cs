// Services/ExportService.cs
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace RpaWinUiComponents.AdvancedWinUiDataGrid
{
    /// <summary>
    /// Implementácia služby pre export funkcionalitu DataGrid komponentu.
    /// Podporuje export do DataTable, CSV, Excel a iných formátov.
    /// </summary>
    internal class ExportService : IExportService
    {
        #region Private fields

        private GridConfiguration? _configuration;
        private IDataManagementService? _dataService;
        private bool _isInitialized = false;
        private bool _disposed = false;

        // Event handling
        private readonly object _eventLock = new();

        // Custom formatters
        private readonly Dictionary<string, Func<object?, string>> _columnFormatters = new();

        #endregion

        #region Properties

        public ExportConfiguration Configuration { get; set; } = ExportConfiguration.Default;

        #endregion

        #region Events

        public event EventHandler<ExportProgressEventArgs>? ExportProgressChanged;
        public event EventHandler<ExportCompletedEventArgs>? ExportCompleted;

        #endregion

        #region IExportService - Inicializácia

        public async Task InitializeAsync(GridConfiguration configuration)
        {
            if (_isInitialized)
                throw new InvalidOperationException("ExportService je už inicializovaný");

            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));

            _isInitialized = true;
            await Task.CompletedTask;
        }

        public bool IsInitialized => _isInitialized;

        #endregion

        #region IExportService - Export do DataTable

        public async Task<DataTable> ExportToDataTableAsync(bool includeEmptyRows = false, CancellationToken cancellationToken = default)
        {
            EnsureInitialized();

            try
            {
                var dataTable = new DataTable();
                var allRows = _dataService!.GetAllRowsData().ToArray();

                OnProgressChanged(0, allRows.Length, "Pripravuje sa štruktúra DataTable...");

                // Pridať stĺpce (bez DeleteRows, s ValidAlerts)
                var columnsToExport = _configuration!.Columns
                    .Where(c => !c.IsDeleteColumn)
                    .ToArray();

                foreach (var column in columnsToExport)
                {
                    var dataColumn = new DataColumn(column.Name, GetDataTableType(column.DataType));
                    dataTable.Columns.Add(dataColumn);
                }

                // Pridať riadky
                int processedRows = 0;
                foreach (var rowData in allRows)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    var cellsArray = rowData.ToArray();
                    var isRowEmpty = IsRowEmpty(cellsArray);

                    if (!includeEmptyRows && isRowEmpty)
                        continue;

                    var dataRow = dataTable.NewRow();

                    foreach (var column in columnsToExport)
                    {
                        var columnIndex = _configuration.GetColumnIndex(column.Name);
                        if (columnIndex >= 0 && columnIndex < cellsArray.Length)
                        {
                            var cell = cellsArray[columnIndex];
                            var value = FormatCellValueForExport(cell, column.Name);
                            dataRow[column.Name] = value ?? DBNull.Value;
                        }
                    }

                    dataTable.Rows.Add(dataRow);
                    processedRows++;

                    // Progress update
                    if (processedRows % 100 == 0)
                    {
                        OnProgressChanged(processedRows, allRows.Length, $"Spracovaných {processedRows} riadkov...");
                    }
                }

                OnProgressChanged(processedRows, allRows.Length, "Export dokončený");
                OnExportCompleted(true, ExportFormat.DataTable, processedRows);

                return dataTable;
            }
            catch (Exception ex)
            {
                OnExportCompleted(false, ExportFormat.DataTable, 0, ex.Message);
                throw;
            }
        }

        public async Task<DataTable> ExportValidRowsToDataTableAsync(CancellationToken cancellationToken = default)
        {
            EnsureInitialized();

            // Pre túto implementáciu exportujeme všetky neprázdne riadky
            // V plnej implementácii by sme mali validačný service reference
            return await ExportToDataTableAsync(includeEmptyRows: false, cancellationToken);
        }

        public async Task<DataTable> ExportRowsToDataTableAsync(IEnumerable<int> rowIndices, CancellationToken cancellationToken = default)
        {
            EnsureInitialized();

            var dataTable = new DataTable();
            var indices = rowIndices.ToArray();

            OnProgressChanged(0, indices.Length, "Pripravuje sa export vybraných riadkov...");

            // Pridať stĺpce
            var columnsToExport = _configuration!.Columns
                .Where(c => !c.IsDeleteColumn)
                .ToArray();

            foreach (var column in columnsToExport)
            {
                var dataColumn = new DataColumn(column.Name, GetDataTableType(column.DataType));
                dataTable.Columns.Add(dataColumn);
            }

            // Pridať vybrané riadky
            int processedRows = 0;
            foreach (var rowIndex in indices)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var rowData = _dataService!.GetRowData(rowIndex);
                if (rowData == null) continue;

                var cellsArray = rowData.ToArray();
                var dataRow = dataTable.NewRow();

                foreach (var column in columnsToExport)
                {
                    var columnIndex = _configuration.GetColumnIndex(column.Name);
                    if (columnIndex >= 0 && columnIndex < cellsArray.Length)
                    {
                        var cell = cellsArray[columnIndex];
                        var value = FormatCellValueForExport(cell, column.Name);
                        dataRow[column.Name] = value ?? DBNull.Value;
                    }
                }

                dataTable.Rows.Add(dataRow);
                processedRows++;

                OnProgressChanged(processedRows, indices.Length, $"Spracovaných {processedRows}/{indices.Length} riadkov...");
            }

            OnExportCompleted(true, ExportFormat.DataTable, processedRows);
            return dataTable;
        }

        #endregion

        #region IExportService - Export do CSV

        public async Task<string> ExportToCsvAsync(bool includeHeaders = true, bool includeEmptyRows = false, string separator = ",", CancellationToken cancellationToken = default)
        {
            EnsureInitialized();

            try
            {
                var sb = new StringBuilder();
                var allRows = _dataService!.GetAllRowsData().ToArray();

                OnProgressChanged(0, allRows.Length, "Pripravuje sa CSV export...");

                // Stĺpce na export
                var columnsToExport = _configuration!.Columns
                    .Where(c => !c.IsDeleteColumn)
                    .ToArray();

                // Hlavičky
                if (includeHeaders)
                {
                    var headers = columnsToExport.Select(c => EscapeCsvValue(c.Header, separator));
                    sb.AppendLine(string.Join(separator, headers));
                }

                // Dáta
                int processedRows = 0;
                foreach (var rowData in allRows)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    var cellsArray = rowData.ToArray();
                    var isRowEmpty = IsRowEmpty(cellsArray);

                    if (!includeEmptyRows && isRowEmpty)
                        continue;

                    var values = new List<string>();
                    foreach (var column in columnsToExport)
                    {
                        var columnIndex = _configuration.GetColumnIndex(column.Name);
                        if (columnIndex >= 0 && columnIndex < cellsArray.Length)
                        {
                            var cell = cellsArray[columnIndex];
                            var value = FormatCellValueForExport(cell, column.Name);
                            values.Add(EscapeCsvValue(value?.ToString() ?? "", separator));
                        }
                        else
                        {
                            values.Add("");
                        }
                    }

                    sb.AppendLine(string.Join(separator, values));
                    processedRows++;

                    if (processedRows % 100 == 0)
                    {
                        OnProgressChanged(processedRows, allRows.Length, $"Spracovaných {processedRows} riadkov...");
                    }
                }

                OnExportCompleted(true, ExportFormat.Csv, processedRows);
                return sb.ToString();
            }
            catch (Exception ex)
            {
                OnExportCompleted(false, ExportFormat.Csv, 0, ex.Message);
                throw;
            }
        }

        #endregion

        #region IExportService - Export do TSV

        public async Task<string> ExportToTsvAsync(bool includeHeaders = true, bool includeEmptyRows = false, CancellationToken cancellationToken = default)
        {
            return await ExportToCsvAsync(includeHeaders, includeEmptyRows, "\t", cancellationToken);
        }

        #endregion

        #region IExportService - Export metadát

        public async Task<string> ExportMetadataAsync(CancellationToken cancellationToken = default)
        {
            EnsureInitialized();

            var metadata = new
            {
                Version = "1.0.0",
                Timestamp = DateTime.UtcNow,
                Configuration = new
                {
                    TotalColumns = _configuration!.TotalColumnCount,
                    DataColumns = _configuration.DataColumnCount,
                    HasDeleteColumn = _configuration.HasDeleteColumn,
                    EmptyRowsCount = _configuration.EmptyRowsCount,
                    ValidationRulesCount = _configuration.ValidationRules.Count
                },
                Columns = _configuration.Columns.Select(c => new
                {
                    c.Name,
                    c.Header,
                    DataType = c.DataType.Name,
                    c.Width,
                    c.MinWidth,
                    c.MaxWidth,
                    c.IsReadOnly,
                    c.IsVisible,
                    c.IsDataColumn,
                    c.IsDeleteColumn,
                    c.IsValidationColumn
                }),
                ValidationRules = _configuration.ValidationRules.Select(r => new
                {
                    r.ColumnName,
                    RuleType = r.RuleType.ToString(),
                    r.ErrorMessage,
                    r.Priority
                }),
                ThrottlingConfig = new
                {
                    _configuration.ThrottlingConfig.ValidationDebounceMs,
                    _configuration.ThrottlingConfig.UIUpdateDebounceMs,
                    _configuration.ThrottlingConfig.BatchSize,
                    _configuration.ThrottlingConfig.EnableValidationThrottling,
                    _configuration.ThrottlingConfig.EnableUIThrottling
                }
            };

            var json = JsonSerializer.Serialize(metadata, new JsonSerializerOptions { WriteIndented = true });
            await Task.CompletedTask;
            return json;
        }

        public async Task<GridStatistics> ExportStatisticsAsync(CancellationToken cancellationToken = default)
        {
            EnsureInitialized();

            var allRows = _dataService!.GetAllRowsData().ToArray();
            var nonEmptyRows = allRows.Where(row => !IsRowEmpty(row.ToArray())).ToArray();

            var statistics = new GridStatistics
            {
                TotalRows = allRows.Length,
                NonEmptyRows = nonEmptyRows.Length,
                ValidRows = nonEmptyRows.Length, // Simplified - would need validation service
                InvalidRows = 0,
                TotalColumns = _configuration!.TotalColumnCount,
                DataColumns = _configuration.DataColumnCount,
                ExportTimestamp = DateTime.Now
            };

            // Column statistics
            foreach (var column in _configuration.DataColumns)
            {
                var columnIndex = _configuration.GetColumnIndex(column.Name);
                var nonEmptyValues = 0;

                foreach (var row in nonEmptyRows)
                {
                    var cell = row.ElementAtOrDefault(columnIndex);
                    if (cell != null && !cell.IsEmpty)
                    {
                        nonEmptyValues++;
                    }
                }

                statistics.ColumnStatistics[column.Name] = nonEmptyValues;
            }

            await Task.CompletedTask;
            return statistics;
        }

        #endregion

        #region IExportService - Parciálny export

        public async Task<DataTable> ExportColumnsToDataTableAsync(IEnumerable<string> columnNames, bool includeEmptyRows = false, CancellationToken cancellationToken = default)
        {
            EnsureInitialized();

            var dataTable = new DataTable();
            var allRows = _dataService!.GetAllRowsData().ToArray();
            var requestedColumns = columnNames.ToArray();

            OnProgressChanged(0, allRows.Length, "Pripravuje sa export vybraných stĺpcov...");

            // Pridať iba požadované stĺpce
            var columnsToExport = _configuration!.Columns
                .Where(c => requestedColumns.Contains(c.Name, StringComparer.OrdinalIgnoreCase) && !c.IsDeleteColumn)
                .ToArray();

            foreach (var column in columnsToExport)
            {
                var dataColumn = new DataColumn(column.Name, GetDataTableType(column.DataType));
                dataTable.Columns.Add(dataColumn);
            }

            // Pridať riadky
            int processedRows = 0;
            foreach (var rowData in allRows)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var cellsArray = rowData.ToArray();
                var isRowEmpty = IsRowEmpty(cellsArray);

                if (!includeEmptyRows && isRowEmpty)
                    continue;

                var dataRow = dataTable.NewRow();

                foreach (var column in columnsToExport)
                {
                    var columnIndex = _configuration.GetColumnIndex(column.Name);
                    if (columnIndex >= 0 && columnIndex < cellsArray.Length)
                    {
                        var cell = cellsArray[columnIndex];
                        var value = FormatCellValueForExport(cell, column.Name);
                        dataRow[column.Name] = value ?? DBNull.Value;
                    }
                }

                dataTable.Rows.Add(dataRow);
                processedRows++;

                if (processedRows % 100 == 0)
                {
                    OnProgressChanged(processedRows, allRows.Length, $"Spracovaných {processedRows} riadkov...");
                }
            }

            OnExportCompleted(true, ExportFormat.DataTable, processedRows);
            return dataTable;
        }

        public async Task<DataTable> ExportRangeToDataTableAsync(int startRow, int startColumn, int endRow, int endColumn, CancellationToken cancellationToken = default)
        {
            EnsureInitialized();

            var dataTable = new DataTable();

            // Validovať rozsah
            if (startRow < 0 || startColumn < 0 || endRow < startRow || endColumn < startColumn)
                throw new ArgumentException("Neplatný rozsah pre export");

            var totalRows = endRow - startRow + 1;
            OnProgressChanged(0, totalRows, "Pripravuje sa export oblasti...");

            // Pripraviť stĺpce
            var columnsInRange = _configuration!.Columns
                .Skip(startColumn)
                .Take(endColumn - startColumn + 1)
                .Where(c => !c.IsDeleteColumn)
                .ToArray();

            foreach (var column in columnsInRange)
            {
                var dataColumn = new DataColumn(column.Name, GetDataTableType(column.DataType));
                dataTable.Columns.Add(dataColumn);
            }

            // Spracovať riadky v rozsahu
            for (int rowIndex = startRow; rowIndex <= endRow; rowIndex++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var rowData = _dataService!.GetRowData(rowIndex);
                if (rowData == null) continue;

                var cellsArray = rowData.ToArray();
                var dataRow = dataTable.NewRow();

                foreach (var column in columnsInRange)
                {
                    var columnIndex = _configuration.GetColumnIndex(column.Name);
                    if (columnIndex >= 0 && columnIndex < cellsArray.Length)
                    {
                        var cell = cellsArray[columnIndex];
                        var value = FormatCellValueForExport(cell, column.Name);
                        dataRow[column.Name] = value ?? DBNull.Value;
                    }
                }

                dataTable.Rows.Add(dataRow);

                var processed = rowIndex - startRow + 1;
                OnProgressChanged(processed, totalRows, $"Spracovaných {processed}/{totalRows} riadkov...");
            }

            OnExportCompleted(true, ExportFormat.DataTable, dataTable.Rows.Count);
            return dataTable;
        }

        #endregion

        #region IExportService - Konfigurácia

        public void SetColumnFormatter(string columnName, Func<object?, string> formatter)
        {
            _columnFormatters[columnName] = formatter ?? throw new ArgumentNullException(nameof(formatter));
        }

        public void RemoveColumnFormatter(string columnName)
        {
            _columnFormatters.Remove(columnName);
        }

        #endregion

        #region IExportService - Validácia

        public ExportValidationResult ValidateExport(ExportFormat format)
        {
            var warnings = new List<string>();

            if (!_isInitialized)
                return ExportValidationResult.Invalid("ExportService nie je inicializovaný");

            if (_dataService == null)
                return ExportValidationResult.Invalid("DataManagementService nie je dostupný");

            // Format-specific validácie
            switch (format)
            {
                case ExportFormat.DataTable:
                    if (_dataService.RowCount > 100000)
                        warnings.Add("Veľký počet riadkov môže ovplyvniť výkon DataTable exportu");
                    break;

                case ExportFormat.Csv:
                case ExportFormat.Tsv:
                    if (_configuration!.Columns.Any(c => c.Name.Contains(",")))
                        warnings.Add("Názvy stĺpcov obsahujú čiarky - môže ovplyvniť CSV formát");
                    break;
            }

            return ExportValidationResult.Valid(warnings);
        }

        #endregion

        #region Dependency injection

        /// <summary>
        /// Nastaví referenciu na DataManagementService (dependency injection).
        /// </summary>
        public void SetDataService(IDataManagementService dataService)
        {
            _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
        }

        #endregion

        #region Private helper methods

        private void EnsureInitialized()
        {
            if (!_isInitialized)
                throw new InvalidOperationException("ExportService nie je inicializovaný");
        }

        private Type GetDataTableType(Type originalType)
        {
            // DataTable nepodporuje nullable typy
            var nullableType = Nullable.GetUnderlyingType(originalType);
            return nullableType ?? originalType;
        }

        private bool IsRowEmpty(CellData[] rowCells)
        {
            return rowCells.Where(cell => cell.ColumnDefinition.IsDataColumn)
                          .All(cell => cell.IsEmpty);
        }

        private object? FormatCellValueForExport(CellData cell, string columnName)
        {
            // Custom formatter
            if (_columnFormatters.TryGetValue(columnName, out var formatter))
            {
                return formatter(cell.Value);
            }

            // Default formatting
            var value = cell.Value;
            if (value == null) return null;

            return value switch
            {
                DateTime dt => dt.ToString(Configuration.DateTimeFormat),
                decimal dec => dec.ToString(Configuration.DecimalFormat),
                double dbl => dbl.ToString(Configuration.DecimalFormat),
                float flt => flt.ToString(Configuration.DecimalFormat),
                _ => value
            };
        }

        private string EscapeCsvValue(string value, string separator)
        {
            if (string.IsNullOrEmpty(value))
                return value;

            // Escape ak obsahuje separator, quotes alebo newlines
            if (value.Contains(separator) || value.Contains("\"") || value.Contains("\n") || value.Contains("\r"))
            {
                // Escape quotes
                value = value.Replace("\"", "\"\"");
                // Wrap in quotes
                return $"\"{value}\"";
            }

            return value;
        }

        #endregion

        #region Event helpers

        private void OnProgressChanged(int processedRows, int totalRows, string operation)
        {
            lock (_eventLock)
            {
                ExportProgressChanged?.Invoke(this, new ExportProgressEventArgs(processedRows, totalRows, operation));
            }
        }

        private void OnExportCompleted(bool success, ExportFormat format, int exportedRows, string? errorMessage = null)
        {
            lock (_eventLock)
            {
                ExportCompleted?.Invoke(this, new ExportCompletedEventArgs(success, format, exportedRows, errorMessage));
            }
        }

        #endregion

        #region IDisposable

        public void Dispose()
        {
            if (_disposed) return;

            try
            {
                // Clear formatters
                _columnFormatters.Clear();

                // Clear events
                lock (_eventLock)
                {
                    ExportProgressChanged = null;
                    ExportCompleted = null;
                }

                _disposed = true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error during ExportService dispose: {ex.Message}");
            }
        }

        #endregion
    }
}