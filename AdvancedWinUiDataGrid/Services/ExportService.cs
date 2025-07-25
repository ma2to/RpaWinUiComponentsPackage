// Services/ExportService.cs - ✅ OPRAVENÝ CS1998 warnings
using Microsoft.Extensions.Logging;
using RpaWinUiComponents.AdvancedWinUiDataGrid.Models;
using RpaWinUiComponents.AdvancedWinUiDataGrid.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace RpaWinUiComponents.AdvancedWinUiDataGrid.Services
{
    /// <summary>
    /// Služba pre export dát z DataGrid
    /// </summary>
    public class ExportService : IExportService
    {
        private readonly ILogger<ExportService> _logger;
        private readonly IDataManagementService _dataManagementService;
        private GridConfiguration? _configuration;

        public ExportService(ILogger<ExportService> logger, IDataManagementService dataManagementService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _dataManagementService = dataManagementService ?? throw new ArgumentNullException(nameof(dataManagementService));
        }

        /// <summary>
        /// Inicializuje export službu s konfiguráciou
        /// </summary>
        public Task InitializeAsync(GridConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _logger.LogInformation("ExportService inicializovaný");
            return Task.CompletedTask;
        }

        /// <summary>
        /// Exportuje všetky dáta do DataTable (bez DeleteRows stĺpca, s ValidAlerts)
        /// </summary>
        public async Task<DataTable> ExportToDataTableAsync()
        {
            try
            {
                _logger.LogInformation("Začína export do DataTable");

                if (_configuration == null)
                    throw new InvalidOperationException("ExportService nie je inicializovaný");

                var dataTable = new DataTable("ExportedData");

                // Vytvor štruktúru stĺpcov (bez DeleteRows, s ValidAlerts)
                CreateDataTableStructure(dataTable);

                // Získaj dáta
                var allData = await _dataManagementService.GetAllDataAsync();

                // Naplň dáta
                await Task.Run(() => PopulateDataTable(dataTable, allData));

                _logger.LogInformation($"Export dokončený: {dataTable.Rows.Count} riadkov, {dataTable.Columns.Count} stĺpcov");
                return dataTable;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Chyba pri exporte do DataTable");
                throw;
            }
        }

        /// <summary>
        /// ✅ OPRAVENÉ CS1998: Exportuje len validné riadky do DataTable - pridané await
        /// </summary>
        public async Task<DataTable> ExportValidRowsOnlyAsync()
        {
            try
            {
                _logger.LogInformation("Začína export len validných riadkov");

                var fullDataTable = await ExportToDataTableAsync();
                var validDataTable = fullDataTable.Clone(); // Skopíruj štruktúru

                // ✅ OPRAVENÉ CS1998: Pridané await pre async operáciu
                await Task.Run(() =>
                {
                    foreach (DataRow row in fullDataTable.Rows)
                    {
                        // Skontroluj ValidAlerts stĺpec
                        var validAlerts = row["ValidAlerts"]?.ToString();
                        if (string.IsNullOrWhiteSpace(validAlerts))
                        {
                            validDataTable.ImportRow(row);
                        }
                    }
                });

                _logger.LogInformation($"Export validných riadkov dokončený: {validDataTable.Rows.Count} z {fullDataTable.Rows.Count} riadkov");
                return validDataTable;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Chyba pri exporte validných riadkov");
                throw;
            }
        }

        /// <summary>
        /// ✅ OPRAVENÉ CS1998: Exportuje len nevalidné riadky do DataTable - pridané await
        /// </summary>
        public async Task<DataTable> ExportInvalidRowsOnlyAsync()
        {
            try
            {
                _logger.LogInformation("Začína export len nevalidných riadkov");

                var fullDataTable = await ExportToDataTableAsync();
                var invalidDataTable = fullDataTable.Clone();

                // ✅ OPRAVENÉ CS1998: Pridané await pre async operáciu
                await Task.Run(() =>
                {
                    foreach (DataRow row in fullDataTable.Rows)
                    {
                        var validAlerts = row["ValidAlerts"]?.ToString();
                        if (!string.IsNullOrWhiteSpace(validAlerts))
                        {
                            invalidDataTable.ImportRow(row);
                        }
                    }
                });

                _logger.LogInformation($"Export nevalidných riadkov dokončený: {invalidDataTable.Rows.Count} z {fullDataTable.Rows.Count} riadkov");
                return invalidDataTable;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Chyba pri exporte nevalidných riadkov");
                throw;
            }
        }

        /// <summary>
        /// ✅ OPRAVENÉ CS1998: Exportuje len špecifické stĺpce - pridané await
        /// </summary>
        public async Task<DataTable> ExportSpecificColumnsAsync(string[] columnNames)
        {
            try
            {
                _logger.LogInformation($"Začína export špecifických stĺpcov: {string.Join(", ", columnNames)}");

                var fullDataTable = await ExportToDataTableAsync();
                var specificDataTable = new DataTable("SpecificColumnsExport");

                // ✅ OPRAVENÉ CS1998: Pridané await pre async operáciu
                await Task.Run(() =>
                {
                    // Vytvor stĺpce len pre požadované
                    foreach (var columnName in columnNames)
                    {
                        if (fullDataTable.Columns.Contains(columnName))
                        {
                            var originalColumn = fullDataTable.Columns[columnName]!;
                            specificDataTable.Columns.Add(columnName, originalColumn.DataType);
                        }
                    }

                    // Skopíruj dáta len pre požadované stĺpce
                    foreach (DataRow originalRow in fullDataTable.Rows)
                    {
                        var newRow = specificDataTable.NewRow();
                        foreach (var columnName in columnNames)
                        {
                            if (specificDataTable.Columns.Contains(columnName))
                            {
                                newRow[columnName] = originalRow[columnName];
                            }
                        }
                        specificDataTable.Rows.Add(newRow);
                    }
                });

                _logger.LogInformation($"Export špecifických stĺpcov dokončený: {specificDataTable.Columns.Count} stĺpcov, {specificDataTable.Rows.Count} riadkov");
                return specificDataTable;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Chyba pri exporte špecifických stĺpcov");
                throw;
            }
        }

        /// <summary>
        /// ✅ OPRAVENÉ CS1998: Exportuje dáta do CSV formátu - pridané await
        /// </summary>
        public async Task<string> ExportToCsvAsync(bool includeHeaders = true)
        {
            try
            {
                _logger.LogInformation("Začína export do CSV");

                var dataTable = await ExportToDataTableAsync();

                // ✅ OPRAVENÉ CS1998: Pridané await pre async operáciu
                var csvContent = await Task.Run(() => ConvertDataTableToCsv(dataTable, includeHeaders));

                _logger.LogInformation($"Export do CSV dokončený: {csvContent.Split('\n').Length} riadkov");
                return csvContent;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Chyba pri exporte do CSV");
                throw;
            }
        }

        /// <summary>
        /// ✅ OPRAVENÉ CS1998: Získa štatistiky exportovaných dát - pridané await
        /// </summary>
        public async Task<ExportStatistics> GetExportStatisticsAsync()
        {
            try
            {
                var dataTable = await ExportToDataTableAsync();

                // ✅ OPRAVENÉ CS1998: Pridané await pre async operáciu
                var statistics = await Task.Run(() => CalculateStatistics(dataTable));

                return statistics;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Chyba pri výpočte exportných štatistík");
                throw;
            }
        }

        #region Private Helper Methods

        private void CreateDataTableStructure(DataTable dataTable)
        {
            if (_configuration?.Columns == null) return;

            foreach (var column in _configuration.Columns)
            {
                // Preskač DeleteRows stĺpec
                if (column.Name == "DeleteRows") continue;

                dataTable.Columns.Add(column.Name, column.DataType);
            }

            // Pridaj ValidAlerts stĺpec
            dataTable.Columns.Add("ValidAlerts", typeof(string));
        }

        private void PopulateDataTable(DataTable dataTable, List<Dictionary<string, object?>> allData)
        {
            foreach (var rowData in allData)
            {
                var dataRow = dataTable.NewRow();

                foreach (DataColumn column in dataTable.Columns)
                {
                    if (rowData.ContainsKey(column.ColumnName))
                    {
                        var value = rowData[column.ColumnName];
                        dataRow[column.ColumnName] = value ?? DBNull.Value;
                    }
                    else
                    {
                        dataRow[column.ColumnName] = DBNull.Value;
                    }
                }

                dataTable.Rows.Add(dataRow);
            }
        }

        private string ConvertDataTableToCsv(DataTable dataTable, bool includeHeaders)
        {
            var lines = new List<string>();

            // Hlavičky
            if (includeHeaders)
            {
                var headers = dataTable.Columns.Cast<DataColumn>()
                    .Select(column => EscapeCsvField(column.ColumnName));
                lines.Add(string.Join(",", headers));
            }

            // Dáta
            foreach (DataRow row in dataTable.Rows)
            {
                var fields = row.ItemArray
                    .Select(field => EscapeCsvField(field?.ToString() ?? string.Empty));
                lines.Add(string.Join(",", fields));
            }

            return string.Join(Environment.NewLine, lines);
        }

        private string EscapeCsvField(string field)
        {
            if (string.IsNullOrEmpty(field)) return "\"\"";

            // Ak obsahuje čiarku, úvodzovky alebo nový riadok, obal do úvodzoviek
            if (field.Contains(',') || field.Contains('"') || field.Contains('\n') || field.Contains('\r'))
            {
                // Zdvojnásob úvodzovky vnútri
                field = field.Replace("\"", "\"\"");
                return $"\"{field}\"";
            }

            return field;
        }

        private ExportStatistics CalculateStatistics(DataTable dataTable)
        {
            var statistics = new ExportStatistics
            {
                TotalRows = dataTable.Rows.Count,
                TotalColumns = dataTable.Columns.Count,
                ValidRows = 0,
                InvalidRows = 0,
                EmptyRows = 0,
                ColumnStatistics = new Dictionary<string, ColumnStatistics>()
            };

            foreach (DataRow row in dataTable.Rows)
            {
                // Počítaj validné/nevalidné riadky
                var validAlerts = row["ValidAlerts"]?.ToString();
                if (string.IsNullOrWhiteSpace(validAlerts))
                {
                    statistics.ValidRows++;
                }
                else
                {
                    statistics.InvalidRows++;
                }

                // Počítaj prázdne riadky
                bool isEmpty = true;
                foreach (DataColumn column in dataTable.Columns)
                {
                    if (column.ColumnName == "ValidAlerts") continue;

                    var value = row[column.ColumnName];
                    if (value != null && value != DBNull.Value && !string.IsNullOrWhiteSpace(value.ToString()))
                    {
                        isEmpty = false;
                        break;
                    }
                }

                if (isEmpty)
                {
                    statistics.EmptyRows++;
                }
            }

            // Štatistiky stĺpcov
            foreach (DataColumn column in dataTable.Columns)
            {
                var columnStats = new ColumnStatistics
                {
                    ColumnName = column.ColumnName,
                    DataType = column.DataType.Name,
                    NonNullCount = 0,
                    NullCount = 0,
                    UniqueValueCount = 0
                };

                var uniqueValues = new HashSet<object>();

                foreach (DataRow row in dataTable.Rows)
                {
                    var value = row[column.ColumnName];
                    if (value == null || value == DBNull.Value)
                    {
                        columnStats.NullCount++;
                    }
                    else
                    {
                        columnStats.NonNullCount++;
                        uniqueValues.Add(value);
                    }
                }

                columnStats.UniqueValueCount = uniqueValues.Count;
                statistics.ColumnStatistics[column.ColumnName] = columnStats;
            }

            return statistics;
        }

        #endregion
    }

    /// <summary>
    /// Štatistiky exportu
    /// </summary>
    internal class ExportStatistics
    {
        public int TotalRows { get; set; }
        public int TotalColumns { get; set; }
        public int ValidRows { get; set; }
        public int InvalidRows { get; set; }
        public int EmptyRows { get; set; }
        public Dictionary<string, ColumnStatistics> ColumnStatistics { get; set; } = new();
    }

    /// <summary>
    /// Štatistiky stĺpca
    /// </summary>
    internal class ColumnStatistics
    {
        public string ColumnName { get; set; } = string.Empty;
        public string DataType { get; set; } = string.Empty;
        public int NonNullCount { get; set; }
        public int NullCount { get; set; }
        public int UniqueValueCount { get; set; }
    }
}