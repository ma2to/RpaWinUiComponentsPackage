// Services/ExportService.cs - ✅ OPRAVENÝ accessibility
using Microsoft.Extensions.Logging.Abstractions;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Models.Grid;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Models.ImportExport;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Services.Operations
{
    /// <summary>
    /// Služba pre export dát z DataGrid s kompletným logovaním - INTERNAL
    /// </summary>
    internal class ExportService : IExportService
    {
        private readonly ILogger<ExportService> _logger;
        private readonly IDataManagementService _dataManagementService;
        private GridConfiguration? _configuration;

        // ✅ ROZŠÍRENÉ: Performance a operation tracking
        private readonly Dictionary<string, DateTime> _operationStartTimes = new();
        private readonly Dictionary<string, int> _operationCounters = new();
        private int _totalExportOperations = 0;
        private int _totalImportOperations = 0;
        private long _totalBytesExported = 0;
        private long _totalBytesImported = 0;
        private readonly string _serviceInstanceId = Guid.NewGuid().ToString("N")[..8];

        // ✅ NOVÉ: Import/Export enhancement
        private readonly Dictionary<string, ImportResult> _importHistory = new();
        private readonly Dictionary<string, string> _exportHistory = new();

        public ExportService(ILogger<ExportService> logger, IDataManagementService dataManagementService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _dataManagementService = dataManagementService ?? throw new ArgumentNullException(nameof(dataManagementService));

            _logger.LogInformation("🔧 ExportService created - InstanceId: {InstanceId}, LoggerType: {LoggerType}",
                _serviceInstanceId, _logger.GetType().Name);
        }

        /// <summary>
        /// Inicializuje export službu s konfiguráciou a kompletným logovaním
        /// </summary>
        public Task InitializeAsync(GridConfiguration configuration)
        {
            var operationId = StartOperation("InitializeAsync");
            
            try
            {
                _logger.LogInformation("📋 ExportService.InitializeAsync START - InstanceId: {InstanceId}",
                    _serviceInstanceId);

                _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));

                // Analyze configuration
                var totalColumns = _configuration.Columns?.Count ?? 0;
                var exportableColumns = _configuration.Columns?.Where(c => c.Name != "DeleteRows").Count() ?? 0;
                var validationRules = _configuration.ValidationRules?.Count ?? 0;
                
                _logger.LogInformation("📋 Configuration analyzed - TotalColumns: {TotalColumns}, " +
                    "ExportableColumns: {ExportableColumns} (excluding DeleteRows), " +
                    "ValidationRules: {ValidationRules}, HasValidAlerts: {HasValidAlerts}",
                    totalColumns, exportableColumns, validationRules, true);

                // Log column details
                if (_configuration.Columns?.Any() == true)
                {
                    var columnDetails = _configuration.Columns
                        .Where(c => c.Name != "DeleteRows")
                        .Select(c => $"{c.Name}({c.DataType.Name})")
                        .ToList();
                    
                    _logger.LogDebug("📋 Exportable columns - [{ColumnDetails}]",
                        string.Join(", ", columnDetails));
                }

                var duration = EndOperation(operationId);
                
                _logger.LogInformation("✅ ExportService INITIALIZED - Duration: {Duration}ms, " +
                    "ExportableColumns: {ExportableColumns}, Ready: {Ready}",
                    duration, exportableColumns, true);

                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ ERROR in ExportService.InitializeAsync - InstanceId: {InstanceId}",
                    _serviceInstanceId);
                throw;
            }
        }

        /// <summary>
        /// Exportuje všetky dáta do DataTable s kompletným data analysis logovaním (bez DeleteRows stĺpca, s ValidAlerts)
        /// </summary>
        public async Task<DataTable> ExportToDataTableAsync(bool includeValidAlerts = false)
        {
            var operationId = StartOperation("ExportToDataTableAsync");
            
            try
            {
                _logger.LogInformation("📊 ExportToDataTable START - InstanceId: {InstanceId}, TotalExportOps: {TotalOps}",
                    _serviceInstanceId, _totalExportOperations);

                if (_configuration == null)
                {
                    _logger.LogError("❌ Configuration is null - ExportService not initialized");
                    throw new InvalidOperationException("ExportService nie je inicializovaný");
                }

                var dataTable = new DataTable("ExportedData");

                // Vytvor štruktúru stĺpcov (bez DeleteRows, s ValidAlerts podľa parametra)
                _logger.LogDebug("📊 Creating DataTable structure - IncludeValidAlerts: {IncludeValidAlerts}", includeValidAlerts);
                CreateDataTableStructure(dataTable, includeValidAlerts);
                
                var columnsCreated = dataTable.Columns.Count;
                _logger.LogInformation("📊 DataTable structure created - Columns: {ColumnsCreated}, " +
                    "Structure: [{ColumnNames}]",
                    columnsCreated, string.Join(", ", dataTable.Columns.Cast<DataColumn>().Select(c => c.ColumnName)));

                // Získaj dáta
                _logger.LogDebug("📊 Retrieving data from DataManagementService");
                var allData = await _dataManagementService.GetAllDataAsync();

                if (allData == null)
                {
                    _logger.LogWarning("📊 DataManagementService returned null data");
                    allData = new List<Dictionary<string, object?>>();
                }

                var inputRowCount = allData.Count;
                var emptyRows = allData.Count(row => IsRowEmpty(row));
                var dataRows = inputRowCount - emptyRows;
                
                _logger.LogInformation("📊 Data retrieved - InputRows: {InputRows}, DataRows: {DataRows}, " +
                    "EmptyRows: {EmptyRows}, EmptyRatio: {EmptyRatio:F1}%",
                    inputRowCount, dataRows, emptyRows, 
                    inputRowCount > 0 ? (double)emptyRows / inputRowCount * 100 : 0);

                // Sample data analysis (first few rows)
                if (allData.Any() && _logger.IsEnabled(LogLevel.Debug))
                {
                    var sampleSize = Math.Min(3, allData.Count);
                    for (int i = 0; i < sampleSize; i++)
                    {
                        var row = allData[i];
                        var nonEmptyColumns = row.Count(kvp => kvp.Value != null && !string.IsNullOrWhiteSpace(kvp.Value.ToString()));
                        
                        _logger.LogDebug("📊 Sample row {Index} - TotalColumns: {TotalColumns}, " +
                            "NonEmptyColumns: {NonEmptyColumns}, IsEmpty: {IsEmpty}",
                            i, row.Count, nonEmptyColumns, IsRowEmpty(row));
                    }
                }

                // Naplň dáta
                _logger.LogDebug("📊 Populating DataTable with {InputRows} rows", inputRowCount);
                await Task.Run(() => PopulateDataTable(dataTable, allData, includeValidAlerts));

                var finalRowCount = dataTable.Rows.Count;
                var finalColumnCount = dataTable.Columns.Count;
                
                // Calculate data size estimation
                var estimatedBytes = EstimateDataTableSize(dataTable);
                TrackBytesExported(estimatedBytes);

                var duration = EndOperation(operationId);
                var processingRate = duration > 0 ? inputRowCount / duration : 0;

                _logger.LogInformation("✅ ExportToDataTable COMPLETED - Duration: {Duration}ms, " +
                    "InputRows: {InputRows}, OutputRows: {OutputRows}, Columns: {Columns}, " +
                    "EstimatedSize: {EstimatedSize:F1} KB, ProcessingRate: {ProcessingRate:F0} rows/ms, " +
                    "DataIntegrity: {DataIntegrity}",
                    duration, inputRowCount, finalRowCount, finalColumnCount,
                    estimatedBytes / 1024.0, processingRate, inputRowCount == finalRowCount ? "OK" : "WARNING");

                // Data quality analysis
                if (finalRowCount > 0)
                {
                    var nonEmptyRowsInTable = 0;
                    var validRowsInTable = 0;
                    
                    foreach (DataRow row in dataTable.Rows)
                    {
                        var isEmpty = IsDataRowEmpty(row);
                        if (!isEmpty) nonEmptyRowsInTable++;
                        
                        var validAlerts = row["ValidAlerts"]?.ToString();
                        if (string.IsNullOrWhiteSpace(validAlerts)) validRowsInTable++;
                    }
                    
                    _logger.LogDebug("📊 Export quality analysis - NonEmptyRows: {NonEmptyRows}, " +
                        "ValidRows: {ValidRows}, InvalidRows: {InvalidRows}, " +
                        "ValidRatio: {ValidRatio:F1}%",
                        nonEmptyRowsInTable, validRowsInTable, finalRowCount - validRowsInTable,
                        finalRowCount > 0 ? (double)validRowsInTable / finalRowCount * 100 : 0);
                }

                return dataTable;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ CRITICAL ERROR in ExportToDataTableAsync - InstanceId: {InstanceId}, " +
                    "TotalExportOps: {TotalOps}", _serviceInstanceId, _totalExportOperations);
                throw;
            }
        }

        /// <summary>
        /// Exportuje len validné riadky do DataTable s komplexným validation filtering logovaním
        /// </summary>
        public async Task<DataTable> ExportValidRowsOnlyAsync(bool includeValidAlerts = false)
        {
            var operationId = StartOperation("ExportValidRowsOnlyAsync");
            
            try
            {
                _logger.LogInformation("✅ ExportValidRowsOnly START - InstanceId: {InstanceId}, TotalExportOps: {TotalOps}",
                    _serviceInstanceId, _totalExportOperations);

                var fullDataTable = await ExportToDataTableAsync(includeValidAlerts);
                var validDataTable = fullDataTable.Clone();
                
                var totalRows = fullDataTable.Rows.Count;
                _logger.LogInformation("✅ Full data retrieved - TotalRows: {TotalRows}, FilteringForValid: {FilterCriteria}",
                    totalRows, "ValidAlerts IS NULL OR EMPTY");

                var filteringResult = await Task.Run(() =>
                {
                    var validCount = 0;
                    var invalidCount = 0;
                    var emptyValidAlertsCount = 0;
                    var validationErrors = new Dictionary<string, int>();

                    foreach (DataRow row in fullDataTable.Rows)
                    {
                        var validAlerts = row["ValidAlerts"]?.ToString();
                        
                        if (string.IsNullOrWhiteSpace(validAlerts))
                        {
                            validDataTable.ImportRow(row);
                            validCount++;
                            
                            if (string.IsNullOrEmpty(validAlerts))
                                emptyValidAlertsCount++;
                        }
                        else
                        {
                            invalidCount++;
                            
                            // Analyzuj typy validation errors (sample first 100 invalid rows)
                            if (invalidCount <= 100)
                            {
                                var errors = validAlerts.Split(';', StringSplitOptions.RemoveEmptyEntries);
                                foreach (var error in errors)
                                {
                                    var errorType = error.Split(':')[0].Trim();
                                    validationErrors[errorType] = validationErrors.GetValueOrDefault(errorType, 0) + 1;
                                }
                            }
                        }
                    }

                    return new { ValidCount = validCount, InvalidCount = invalidCount, 
                                EmptyValidAlertsCount = emptyValidAlertsCount, ValidationErrors = validationErrors };
                });

                // Calculate data quality metrics
                var validRatio = totalRows > 0 ? (double)filteringResult.ValidCount / totalRows * 100 : 0;
                var filterEfficiency = filteringResult.ValidCount;
                var estimatedBytes = EstimateDataTableSize(validDataTable);
                TrackBytesExported(estimatedBytes);

                var duration = EndOperation(operationId);
                var processingRate = duration > 0 ? totalRows / duration : 0;

                _logger.LogInformation("✅ ExportValidRowsOnly COMPLETED - Duration: {Duration}ms, " +
                    "InputRows: {InputRows}, ValidRows: {ValidRows}, InvalidRows: {InvalidRows}, " +
                    "ValidRatio: {ValidRatio:F1}%, FilterEfficiency: {FilterEfficiency}, " +
                    "EstimatedSize: {EstimatedSize:F1} KB, ProcessingRate: {ProcessingRate:F0} rows/ms",
                    duration, totalRows, filteringResult.ValidCount, filteringResult.InvalidCount,
                    validRatio, filterEfficiency, estimatedBytes / 1024.0, processingRate);

                // Log validation error analysis
                if (filteringResult.ValidationErrors.Any())
                {
                    var topErrors = filteringResult.ValidationErrors
                        .OrderByDescending(kvp => kvp.Value)
                        .Take(5)
                        .Select(kvp => $"{kvp.Key}: {kvp.Value}")
                        .ToList();
                    
                    _logger.LogDebug("✅ Validation error analysis - TopErrors: [{TopErrors}], " +
                        "TotalErrorTypes: {TotalErrorTypes}",
                        string.Join(", ", topErrors), filteringResult.ValidationErrors.Count);
                }

                _logger.LogDebug("✅ Data filtering completed - EmptyValidAlerts: {EmptyAlerts}, " +
                    "DataQuality: {Quality}%, FilterPrecision: {Precision:F1}%",
                    filteringResult.EmptyValidAlertsCount, validRatio, 
                    totalRows > 0 ? (double)filteringResult.ValidCount / (filteringResult.ValidCount + filteringResult.InvalidCount) * 100 : 0);

                return validDataTable;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ CRITICAL ERROR in ExportValidRowsOnlyAsync - InstanceId: {InstanceId}",
                    _serviceInstanceId);
                throw;
            }
        }

        /// <summary>
        /// Exportuje len nevalidné riadky do DataTable s komplexnou error analysis
        /// </summary>
        public async Task<DataTable> ExportInvalidRowsOnlyAsync(bool includeValidAlerts = true)
        {
            var operationId = StartOperation("ExportInvalidRowsOnlyAsync");
            
            try
            {
                _logger.LogInformation("❌ ExportInvalidRowsOnly START - InstanceId: {InstanceId}, TotalExportOps: {TotalOps}",
                    _serviceInstanceId, _totalExportOperations);

                var fullDataTable = await ExportToDataTableAsync(includeValidAlerts);
                var invalidDataTable = fullDataTable.Clone();
                
                var totalRows = fullDataTable.Rows.Count;
                _logger.LogInformation("❌ Full data retrieved - TotalRows: {TotalRows}, FilteringForInvalid: {FilterCriteria}",
                    totalRows, "ValidAlerts IS NOT NULL AND NOT EMPTY");

                var errorAnalysis = await Task.Run(() =>
                {
                    var invalidCount = 0;
                    var validCount = 0;
                    var errorTypeFrequency = new Dictionary<string, int>();
                    var errorSeverityDistribution = new Dictionary<string, int>();
                    var columnErrorFrequency = new Dictionary<string, int>();
                    var maxErrorsPerRow = 0;
                    var totalErrorsCount = 0;

                    foreach (DataRow row in fullDataTable.Rows)
                    {
                        var validAlerts = row["ValidAlerts"]?.ToString();
                        
                        if (!string.IsNullOrWhiteSpace(validAlerts))
                        {
                            invalidDataTable.ImportRow(row);
                            invalidCount++;
                            
                            // Detailná analýza validation errors
                            var errors = validAlerts.Split(';', StringSplitOptions.RemoveEmptyEntries);
                            var rowErrorCount = errors.Length;
                            totalErrorsCount += rowErrorCount;
                            
                            if (rowErrorCount > maxErrorsPerRow)
                                maxErrorsPerRow = rowErrorCount;

                            foreach (var error in errors)
                            {
                                var parts = error.Split(':', 2);
                                if (parts.Length >= 2)
                                {
                                    var columnName = parts[0].Trim();
                                    var errorMessage = parts[1].Trim();
                                    
                                    // Count errors per column
                                    columnErrorFrequency[columnName] = columnErrorFrequency.GetValueOrDefault(columnName, 0) + 1;
                                    
                                    // Categorize error types
                                    var errorType = CategorizeErrorType(errorMessage);
                                    errorTypeFrequency[errorType] = errorTypeFrequency.GetValueOrDefault(errorType, 0) + 1;
                                    
                                    // Categorize error severity
                                    var severity = CategorizeErrorSeverity(errorMessage);
                                    errorSeverityDistribution[severity] = errorSeverityDistribution.GetValueOrDefault(severity, 0) + 1;
                                }
                            }
                        }
                        else
                        {
                            validCount++;
                        }
                    }

                    var avgErrorsPerInvalidRow = invalidCount > 0 ? (double)totalErrorsCount / invalidCount : 0;

                    return new { 
                        InvalidCount = invalidCount, ValidCount = validCount,
                        ErrorTypeFrequency = errorTypeFrequency, ErrorSeverityDistribution = errorSeverityDistribution,
                        ColumnErrorFrequency = columnErrorFrequency, MaxErrorsPerRow = maxErrorsPerRow,
                        TotalErrorsCount = totalErrorsCount, AvgErrorsPerInvalidRow = avgErrorsPerInvalidRow
                    };
                });

                // Calculate error analysis metrics
                var invalidRatio = totalRows > 0 ? (double)errorAnalysis.InvalidCount / totalRows * 100 : 0;
                var errorDensity = totalRows > 0 ? (double)errorAnalysis.TotalErrorsCount / totalRows : 0;
                var estimatedBytes = EstimateDataTableSize(invalidDataTable);
                TrackBytesExported(estimatedBytes);

                var duration = EndOperation(operationId);
                var processingRate = duration > 0 ? totalRows / duration : 0;

                _logger.LogInformation("❌ ExportInvalidRowsOnly COMPLETED - Duration: {Duration}ms, " +
                    "InputRows: {InputRows}, InvalidRows: {InvalidRows}, ValidRows: {ValidRows}, " +
                    "InvalidRatio: {InvalidRatio:F1}%, TotalErrors: {TotalErrors}, " +
                    "AvgErrorsPerInvalidRow: {AvgErrors:F1}, MaxErrorsPerRow: {MaxErrors}, " +
                    "ErrorDensity: {ErrorDensity:F2} errors/row, EstimatedSize: {EstimatedSize:F1} KB",
                    duration, totalRows, errorAnalysis.InvalidCount, errorAnalysis.ValidCount,
                    invalidRatio, errorAnalysis.TotalErrorsCount, errorAnalysis.AvgErrorsPerInvalidRow,
                    errorAnalysis.MaxErrorsPerRow, errorDensity, estimatedBytes / 1024.0);

                // Log detailed error analysis
                if (errorAnalysis.ErrorTypeFrequency.Any())
                {
                    var topErrorTypes = errorAnalysis.ErrorTypeFrequency
                        .OrderByDescending(kvp => kvp.Value)
                        .Take(5)
                        .Select(kvp => $"{kvp.Key}: {kvp.Value}")
                        .ToList();
                    
                    _logger.LogDebug("❌ Error type analysis - TopErrorTypes: [{TopTypes}], " +
                        "TotalErrorTypeCount: {TotalTypes}",
                        string.Join(", ", topErrorTypes), errorAnalysis.ErrorTypeFrequency.Count);
                }

                if (errorAnalysis.ColumnErrorFrequency.Any())
                {
                    var problematicColumns = errorAnalysis.ColumnErrorFrequency
                        .OrderByDescending(kvp => kvp.Value)
                        .Take(5)
                        .Select(kvp => $"{kvp.Key}: {kvp.Value}")
                        .ToList();
                    
                    _logger.LogDebug("❌ Column error analysis - ProblematicColumns: [{ProblematicColumns}]",
                        string.Join(", ", problematicColumns));
                }

                if (errorAnalysis.ErrorSeverityDistribution.Any())
                {
                    var severityBreakdown = errorAnalysis.ErrorSeverityDistribution
                        .Select(kvp => $"{kvp.Key}: {kvp.Value}")
                        .ToList();
                    
                    _logger.LogDebug("❌ Error severity distribution - [{SeverityBreakdown}]",
                        string.Join(", ", severityBreakdown));
                }

                return invalidDataTable;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ CRITICAL ERROR in ExportInvalidRowsOnlyAsync - InstanceId: {InstanceId}",
                    _serviceInstanceId);
                throw;
            }
        }

        /// <summary>
        /// Exportuje len špecifické stĺpce s komplexnou column mapping a data transformation analýzou
        /// </summary>
        public async Task<DataTable> ExportSpecificColumnsAsync(string[] columnNames, bool includeValidAlerts = false)
        {
            var operationId = StartOperation("ExportSpecificColumnsAsync");
            
            try
            {
                _logger.LogInformation("📊 ExportSpecificColumns START - InstanceId: {InstanceId}, " +
                    "RequestedColumns: [{RequestedColumns}], ColumnCount: {ColumnCount}, TotalExportOps: {TotalOps}",
                    _serviceInstanceId, string.Join(", ", columnNames), columnNames.Length, _totalExportOperations);

                if (columnNames == null || !columnNames.Any())
                {
                    _logger.LogWarning("📊 No columns specified for export - returning empty DataTable");
                    return new DataTable("EmptySpecificColumnsExport");
                }

                var fullDataTable = await ExportToDataTableAsync(includeValidAlerts);
                var specificDataTable = new DataTable("SpecificColumnsExport");
                
                var totalRows = fullDataTable.Rows.Count;
                var availableColumns = fullDataTable.Columns.Cast<DataColumn>().Select(c => c.ColumnName).ToList();
                
                _logger.LogInformation("📊 Full data retrieved - TotalRows: {TotalRows}, " +
                    "AvailableColumns: {AvailableColumnCount}, AvailableColumnList: [{AvailableColumns}]",
                    totalRows, availableColumns.Count, string.Join(", ", availableColumns));

                var columnMapping = await Task.Run(() =>
                {
                    var validColumns = new List<string>();
                    var missingColumns = new List<string>();
                    var columnDataTypes = new Dictionary<string, string>();
                    var columnNullCounts = new Dictionary<string, int>();
                    var columnNonNullCounts = new Dictionary<string, int>();

                    // Validate and create columns
                    foreach (var columnName in columnNames)
                    {
                        if (fullDataTable.Columns.Contains(columnName))
                        {
                            var originalColumn = fullDataTable.Columns[columnName]!;
                            specificDataTable.Columns.Add(columnName, originalColumn.DataType);
                            validColumns.Add(columnName);
                            columnDataTypes[columnName] = originalColumn.DataType.Name;
                        }
                        else
                        {
                            missingColumns.Add(columnName);
                        }
                    }

                    // Copy data and analyze column content
                    foreach (DataRow originalRow in fullDataTable.Rows)
                    {
                        var newRow = specificDataTable.NewRow();
                        
                        foreach (var columnName in validColumns)
                        {
                            var value = originalRow[columnName];
                            newRow[columnName] = value;
                            
                            // Track null/non-null counts
                            if (value == null || value == DBNull.Value)
                            {
                                columnNullCounts[columnName] = columnNullCounts.GetValueOrDefault(columnName, 0) + 1;
                            }
                            else
                            {
                                columnNonNullCounts[columnName] = columnNonNullCounts.GetValueOrDefault(columnName, 0) + 1;
                            }
                        }
                        
                        specificDataTable.Rows.Add(newRow);
                    }

                    return new {
                        ValidColumns = validColumns, MissingColumns = missingColumns,
                        ColumnDataTypes = columnDataTypes, ColumnNullCounts = columnNullCounts,
                        ColumnNonNullCounts = columnNonNullCounts
                    };
                });

                // Calculate transformation metrics
                var columnMappingSuccess = columnNames.Length > 0 ? (double)columnMapping.ValidColumns.Count / columnNames.Length * 100 : 0;
                var dataRetentionRatio = totalRows > 0 ? (double)specificDataTable.Rows.Count / totalRows * 100 : 0;
                var estimatedBytes = EstimateDataTableSize(specificDataTable);
                TrackBytesExported(estimatedBytes);

                var duration = EndOperation(operationId);
                var processingRate = duration > 0 ? totalRows / duration : 0;

                _logger.LogInformation("✅ ExportSpecificColumns COMPLETED - Duration: {Duration}ms, " +
                    "InputRows: {InputRows}, OutputRows: {OutputRows}, " +
                    "RequestedColumns: {RequestedColumns}, ValidColumns: {ValidColumns}, " +
                    "MissingColumns: {MissingColumns}, ColumnMappingSuccess: {MappingSuccess:F1}%, " +
                    "DataRetention: {DataRetention:F1}%, EstimatedSize: {EstimatedSize:F1} KB, " +
                    "ProcessingRate: {ProcessingRate:F0} rows/ms",
                    duration, totalRows, specificDataTable.Rows.Count, 
                    columnNames.Length, columnMapping.ValidColumns.Count, columnMapping.MissingColumns.Count,
                    columnMappingSuccess, dataRetentionRatio, estimatedBytes / 1024.0, processingRate);

                // Log missing columns warning
                if (columnMapping.MissingColumns.Any())
                {
                    _logger.LogWarning("📊 Missing columns detected - MissingColumns: [{MissingColumns}], " +
                        "Impact: {ImpactPercent:F1}% of requested columns not found",
                        string.Join(", ", columnMapping.MissingColumns),
                        columnNames.Length > 0 ? (double)columnMapping.MissingColumns.Count / columnNames.Length * 100 : 0);
                }

                // Log column data quality analysis
                if (columnMapping.ValidColumns.Any() && _logger.IsEnabled(LogLevel.Debug))
                {
                    var columnQualityReport = columnMapping.ValidColumns.Select(col => 
                    {
                        var nullCount = columnMapping.ColumnNullCounts.GetValueOrDefault(col, 0);
                        var nonNullCount = columnMapping.ColumnNonNullCounts.GetValueOrDefault(col, 0);
                        var fillRatio = (nullCount + nonNullCount) > 0 ? (double)nonNullCount / (nullCount + nonNullCount) * 100 : 0;
                        return $"{col}({columnMapping.ColumnDataTypes[col]}): {fillRatio:F1}% filled";
                    }).ToList();
                    
                    _logger.LogDebug("📊 Column quality analysis - [{ColumnQuality}]",
                        string.Join(", ", columnQualityReport));
                }

                // Data transformation summary
                var originalColumnCount = fullDataTable.Columns.Count;
                var reductionRatio = originalColumnCount > 0 ? (double)(originalColumnCount - specificDataTable.Columns.Count) / originalColumnCount * 100 : 0;
                
                _logger.LogDebug("📊 Data transformation summary - OriginalColumns: {OriginalColumns}, " +
                    "ExportedColumns: {ExportedColumns}, ColumnReduction: {ColumnReduction:F1}%, " +
                    "DataIntegrity: {DataIntegrity}",
                    originalColumnCount, specificDataTable.Columns.Count, reductionRatio,
                    specificDataTable.Rows.Count == totalRows ? "OK" : "WARNING");

                return specificDataTable;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ CRITICAL ERROR in ExportSpecificColumnsAsync - InstanceId: {InstanceId}, " +
                    "RequestedColumns: [{RequestedColumns}]", _serviceInstanceId, 
                    columnNames != null ? string.Join(", ", columnNames) : "NULL");
                throw;
            }
        }

        /// <summary>
        /// Exportuje dáta do CSV formátu s komplexnou format conversion a encoding analýzou
        /// </summary>
        public async Task<string> ExportToCsvAsync(bool includeHeaders = true, bool includeValidAlerts = false)
        {
            var operationId = StartOperation("ExportToCsvAsync");
            
            try
            {
                _logger.LogInformation("📄 ExportToCsv START - InstanceId: {InstanceId}, " +
                    "IncludeHeaders: {IncludeHeaders}, IncludeValidAlerts: {IncludeValidAlerts}, TotalExportOps: {TotalOps}",
                    _serviceInstanceId, includeHeaders, includeValidAlerts, _totalExportOperations);

                var dataTable = await ExportToDataTableAsync(includeValidAlerts);
                
                var totalRows = dataTable.Rows.Count;
                var totalColumns = dataTable.Columns.Count;
                
                _logger.LogInformation("📄 DataTable retrieved for CSV conversion - " +
                    "Rows: {Rows}, Columns: {Columns}, IncludeHeaders: {IncludeHeaders}",
                    totalRows, totalColumns, includeHeaders);

                var csvConversion = await Task.Run(() =>
                {
                    var csvContent = ConvertDataTableToCsv(dataTable, includeHeaders);
                    
                    // Analyze CSV content
                    var lines = csvContent.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
                    var actualLineCount = lines.Length;
                    var expectedLineCount = totalRows + (includeHeaders ? 1 : 0);
                    var csvSizeBytes = System.Text.Encoding.UTF8.GetByteCount(csvContent);
                    
                    // Character analysis
                    var commaCount = csvContent.Count(c => c == ',');
                    var quoteCount = csvContent.Count(c => c == '"');
                    var specialCharCount = csvContent.Count(c => c == '\n' || c == '\r');
                    
                    // Estimate complexity
                    var fieldsWithSpecialChars = 0;
                    var fieldsWithQuotes = 0;
                    var emptyFields = 0;
                    
                    foreach (DataRow row in dataTable.Rows)
                    {
                        foreach (var item in row.ItemArray)
                        {
                            var fieldValue = item?.ToString() ?? "";
                            
                            if (string.IsNullOrEmpty(fieldValue))
                            {
                                emptyFields++;
                            }
                            else
                            {
                                if (fieldValue.Contains(',') || fieldValue.Contains('\n') || fieldValue.Contains('\r'))
                                    fieldsWithSpecialChars++;
                                if (fieldValue.Contains('"'))
                                    fieldsWithQuotes++;
                            }
                        }
                    }
                    
                    return new {
                        CsvContent = csvContent, ActualLineCount = actualLineCount, 
                        ExpectedLineCount = expectedLineCount, CsvSizeBytes = csvSizeBytes,
                        CommaCount = commaCount, QuoteCount = quoteCount, SpecialCharCount = specialCharCount,
                        FieldsWithSpecialChars = fieldsWithSpecialChars, FieldsWithQuotes = fieldsWithQuotes,
                        EmptyFields = emptyFields
                    };
                });

                // Calculate conversion metrics
                var conversionAccuracy = csvConversion.ExpectedLineCount > 0 ? 
                    (double)csvConversion.ActualLineCount / csvConversion.ExpectedLineCount * 100 : 0;
                var avgBytesPerRow = totalRows > 0 ? (double)csvConversion.CsvSizeBytes / totalRows : 0;
                var compressionRatio = (double)(dataTable.Rows.Count * dataTable.Columns.Count * 10); // Rough estimate
                compressionRatio = compressionRatio > 0 ? (double)csvConversion.CsvSizeBytes / compressionRatio : 1.0;
                
                TrackBytesExported(csvConversion.CsvSizeBytes);

                var duration = EndOperation(operationId);
                var processingRate = duration > 0 ? totalRows / duration : 0;

                _logger.LogInformation("✅ ExportToCsv COMPLETED - Duration: {Duration}ms, " +
                    "InputRows: {InputRows}, OutputLines: {OutputLines}, " +
                    "ExpectedLines: {ExpectedLines}, ConversionAccuracy: {ConversionAccuracy:F1}%, " +
                    "CsvSize: {CsvSize:F1} KB, AvgBytesPerRow: {AvgBytesPerRow:F1}, " +
                    "CompressionRatio: {CompressionRatio:F2}, ProcessingRate: {ProcessingRate:F0} rows/ms",
                    duration, totalRows, csvConversion.ActualLineCount, csvConversion.ExpectedLineCount,
                    conversionAccuracy, csvConversion.CsvSizeBytes / 1024.0, avgBytesPerRow,
                    compressionRatio, processingRate);

                // Log CSV complexity analysis
                var totalFields = totalRows * totalColumns;
                if (totalFields > 0)
                {
                    var specialCharRatio = (double)csvConversion.FieldsWithSpecialChars / totalFields * 100;
                    var quoteFieldRatio = (double)csvConversion.FieldsWithQuotes / totalFields * 100;
                    var emptyFieldRatio = (double)csvConversion.EmptyFields / totalFields * 100;
                    
                    _logger.LogDebug("📄 CSV complexity analysis - TotalFields: {TotalFields}, " +
                        "SpecialCharFields: {SpecialFields} ({SpecialRatio:F1}%), " +
                        "QuoteFields: {QuoteFields} ({QuoteRatio:F1}%), " +
                        "EmptyFields: {EmptyFields} ({EmptyRatio:F1}%), " +
                        "Commas: {Commas}, Quotes: {Quotes}",
                        totalFields, csvConversion.FieldsWithSpecialChars, specialCharRatio,
                        csvConversion.FieldsWithQuotes, quoteFieldRatio,
                        csvConversion.EmptyFields, emptyFieldRatio,
                        csvConversion.CommaCount, csvConversion.QuoteCount);
                }

                // Log format validation
                var formatValidation = csvConversion.ActualLineCount == csvConversion.ExpectedLineCount ? "VALID" : "WARNING";
                _logger.LogDebug("📄 CSV format validation - Status: {Status}, " +
                    "HeadersIncluded: {HeadersIncluded}, Encoding: UTF8, " +
                    "LineEndings: {LineEndings}, FieldSeparator: comma",
                    formatValidation, includeHeaders, Environment.NewLine.Length == 2 ? "CRLF" : "LF");

                return csvConversion.CsvContent;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ CRITICAL ERROR in ExportToCsvAsync - InstanceId: {InstanceId}, " +
                    "IncludeHeaders: {IncludeHeaders}", _serviceInstanceId, includeHeaders);
                throw;
            }
        }

        /// <summary>
        /// Získa štatistiky exportovaných dát
        /// </summary>
        public async Task<Models.ImportExport.ExportStatistics> GetExportStatisticsAsync(bool includeValidAlerts = false)
        {
            try
            {
                var dataTable = await ExportToDataTableAsync(includeValidAlerts);
                var statistics = await Task.Run(() => CalculateStatistics(dataTable));
                return statistics;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Chyba pri výpočte exportných štatistík");
                throw;
            }
        }

        #region ✅ NOVÉ: Import/Export Enhancement Methods

        /// <summary>
        /// Importuje dáta zo súboru s komplexným file format detection a validation - ✅ PUBLIC API
        /// </summary>
        public async Task<ImportResult> ImportFromFileAsync(string filePath, ImportExportConfiguration? config = null)
        {
            var operationId = StartOperation("ImportFromFileAsync");
            var result = new ImportResult
            {
                FilePath = filePath,
                StartTime = DateTime.UtcNow
            };

            try
            {
                _logger.LogInformation("📥 ImportFromFile START - InstanceId: {InstanceId}, " +
                    "FilePath: {FilePath}, TotalImportOps: {TotalOps}",
                    _serviceInstanceId, filePath, _totalImportOperations);

                config ??= ImportExportConfiguration.DefaultCsv;
                result.Format = config.Format;

                // Validation of file
                if (!System.IO.File.Exists(filePath))
                {
                    result.AddError($"Súbor sa nenašiel: {filePath}", severity: ErrorSeverity.Critical);
                    result.FinalizeImport();
                    return result;
                }

                var fileInfo = new System.IO.FileInfo(filePath);
                result.FileSizeBytes = fileInfo.Length;

                _logger.LogInformation("📥 File validated - Size: {FileSizeKB:F1} KB, " +
                    "LastModified: {LastModified}, Format: {Format}",
                    result.FileSizeBytes / 1024.0, fileInfo.LastWriteTime, config.Format);

                // Format detection if auto-detect
                var detectedFormat = DetectFileFormat(filePath, config.Format);
                if (detectedFormat != config.Format)
                {
                    _logger.LogInformation("📥 Format detection - Requested: {RequestedFormat}, " +
                        "Detected: {DetectedFormat}, Using: {UsingFormat}",
                        config.Format, detectedFormat, detectedFormat);
                    result.Format = detectedFormat;
                }

                // Import based on format
                switch (result.Format)
                {
                    case ExportFormat.CSV:
                        await ImportFromCsvAsync(filePath, config, result);
                        break;
                    case ExportFormat.TSV:
                        var tsvConfig = new ImportExportConfiguration
                        {
                            Format = ExportFormat.TSV,
                            CsvSeparator = "\t",
                            IncludeHeaders = config.IncludeHeaders,
                            SkipEmptyRows = config.SkipEmptyRows,
                            ValidateOnImport = config.ValidateOnImport
                        };
                        await ImportFromCsvAsync(filePath, tsvConfig, result);
                        break;
                    case ExportFormat.JSON:
                        await ImportFromJsonAsync(filePath, config, result);
                        break;
                    case ExportFormat.Excel:
                        result.AddError("Excel import nie je v tejto verzii podporovaný", severity: ErrorSeverity.Critical);
                        break;
                    case ExportFormat.XML:
                        result.AddError("XML import nie je v tejto verzii podporovaný", severity: ErrorSeverity.Critical);
                        break;
                }

                // Backup if requested
                if (config.BackupExistingFile && System.IO.File.Exists(filePath))
                {
                    await CreateFileBackupAsync(filePath);
                }

                // Store in import history
                _importHistory[result.ImportId] = result;
                TrackBytesImported(result.FileSizeBytes);

                result.FinalizeImport();
                var duration = EndOperation(operationId);

                _logger.LogInformation("✅ ImportFromFile COMPLETED - Duration: {Duration}ms, " +
                    "ImportId: {ImportId}, Status: {Status}, " +
                    "ProcessedRows: {ProcessedRows}, SuccessfulRows: {SuccessfulRows}, " +
                    "ErrorRows: {ErrorRows}, SuccessRate: {SuccessRate:F1}%",
                    duration, result.ImportId, result.IsSuccessful ? "SUCCESS" : "FAILED",
                    result.TotalRowsInFile, result.SuccessfullyImportedRows,
                    result.ErrorRows, result.SuccessRate);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ CRITICAL ERROR in ImportFromFileAsync - InstanceId: {InstanceId}, " +
                    "FilePath: {FilePath}", _serviceInstanceId, filePath);
                result.AddError($"Kritická chyba pri importe: {ex.Message}", severity: ErrorSeverity.Critical);
                result.FinalizeImport();
                return result;
            }
        }

        /// <summary>
        /// Exportuje dáta do súboru s komplexnou configuration a format handling - ✅ PUBLIC API
        /// </summary>
        public async Task<string> ExportToFileAsync(string filePath, ImportExportConfiguration? config = null)
        {
            var operationId = StartOperation("ExportToFileAsync");
            
            try
            {
                _logger.LogInformation("📤 ExportToFile START - InstanceId: {InstanceId}, " +
                    "FilePath: {FilePath}, TotalExportOps: {TotalOps}",
                    _serviceInstanceId, filePath, _totalExportOperations);

                config ??= ImportExportConfiguration.DefaultCsv;

                // Backup existing file if requested
                if (config.BackupExistingFile && System.IO.File.Exists(filePath))
                {
                    await CreateFileBackupAsync(filePath);
                }

                string exportedContent;
                long estimatedBytes = 0;

                // Export based on format
                switch (config.Format)
                {
                    case ExportFormat.CSV:
                    case ExportFormat.TSV:
                        exportedContent = await ExportToCsvAsync(config.IncludeHeaders);
                        if (config.Format == ExportFormat.TSV)
                        {
                            exportedContent = exportedContent.Replace(",", "\t");
                        }
                        break;
                    case ExportFormat.JSON:
                        exportedContent = await ExportToJsonAsync(config);
                        break;
                    case ExportFormat.Excel:
                        throw new NotSupportedException("Excel export nie je v tejto verzii podporovaný");
                    case ExportFormat.XML:
                        throw new NotSupportedException("XML export nie je v tejto verzii podporovaný");
                    default:
                        exportedContent = await ExportToCsvAsync(config.IncludeHeaders);
                        break;
                }

                // Write to file
                await System.IO.File.WriteAllTextAsync(filePath, exportedContent, 
                    System.Text.Encoding.GetEncoding(config.Encoding));
                
                estimatedBytes = System.Text.Encoding.GetEncoding(config.Encoding).GetByteCount(exportedContent);
                TrackBytesExported(estimatedBytes);

                // Store in export history
                _exportHistory[DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")] = filePath;

                // Auto-open if requested
                if (config.AutoOpenFile && System.IO.File.Exists(filePath))
                {
                    try
                    {
                        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                        {
                            FileName = filePath,
                            UseShellExecute = true
                        });
                        _logger.LogDebug("📤 File auto-opened - FilePath: {FilePath}", filePath);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "📤 Failed to auto-open file - FilePath: {FilePath}", filePath);
                    }
                }

                var duration = EndOperation(operationId);

                _logger.LogInformation("✅ ExportToFile COMPLETED - Duration: {Duration}ms, " +
                    "FilePath: {FilePath}, Format: {Format}, Size: {SizeKB:F1} KB, " +
                    "Encoding: {Encoding}, AutoOpened: {AutoOpened}",
                    duration, filePath, config.Format, estimatedBytes / 1024.0,
                    config.Encoding, config.AutoOpenFile);

                return filePath;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ CRITICAL ERROR in ExportToFileAsync - InstanceId: {InstanceId}, " +
                    "FilePath: {FilePath}", _serviceInstanceId, filePath);
                throw;
            }
        }

        /// <summary>
        /// Získa import history - ✅ PUBLIC API
        /// </summary>
        public Dictionary<string, ImportResult> GetImportHistory()
        {
            return new Dictionary<string, ImportResult>(_importHistory);
        }

        /// <summary>
        /// Získa export history - ✅ PUBLIC API
        /// </summary>
        public Dictionary<string, string> GetExportHistory()
        {
            return new Dictionary<string, string>(_exportHistory);
        }

        /// <summary>
        /// Vyčistí import/export history - ✅ PUBLIC API
        /// </summary>
        public void ClearHistory()
        {
            _importHistory.Clear();
            _exportHistory.Clear();
            _logger.LogInformation("🧹 Import/Export history cleared - InstanceId: {InstanceId}", _serviceInstanceId);
        }

        #endregion

        #region ✅ PRIVATE Import/Export Helper Methods

        /// <summary>
        /// Detekuje formát súboru na základe extension a obsahu
        /// </summary>
        private ExportFormat DetectFileFormat(string filePath, ExportFormat requestedFormat)
        {
            try
            {
                var extension = System.IO.Path.GetExtension(filePath).ToLowerInvariant();
                
                // Automatic detection based on extension
                var detectedFormat = extension switch
                {
                    ".csv" => ExportFormat.CSV,
                    ".tsv" => ExportFormat.TSV,
                    ".json" => ExportFormat.JSON,
                    ".xlsx" or ".xls" => ExportFormat.Excel,
                    ".xml" => ExportFormat.XML,
                    _ => requestedFormat
                };

                // Content-based validation for text files
                if (detectedFormat == ExportFormat.CSV || detectedFormat == ExportFormat.TSV)
                {
                    var sampleContent = System.IO.File.ReadLines(filePath).Take(3).ToList();
                    if (sampleContent.Any())
                    {
                        var firstLine = sampleContent.First();
                        var tabCount = firstLine.Count(c => c == '\t');
                        var commaCount = firstLine.Count(c => c == ',');
                        
                        if (tabCount > commaCount && tabCount > 0)
                        {
                            detectedFormat = ExportFormat.TSV;
                        }
                        else if (commaCount > 0)
                        {
                            detectedFormat = ExportFormat.CSV;
                        }
                    }
                }

                return detectedFormat;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "📥 File format detection failed - using requested format: {RequestedFormat}", 
                    requestedFormat);
                return requestedFormat;
            }
        }

        /// <summary>
        /// Importuje z CSV súboru
        /// </summary>
        private async Task ImportFromCsvAsync(string filePath, ImportExportConfiguration config, ImportResult result)
        {
            var lines = await System.IO.File.ReadAllLinesAsync(filePath, System.Text.Encoding.GetEncoding(config.Encoding));
            result.TotalRowsInFile = lines.Length;

            if (config.IncludeHeaders && lines.Length > 0)
            {
                result.TotalRowsInFile--; // Subtract header row
                lines = lines.Skip(1).ToArray();
            }

            var separator = config.CsvSeparator;
            var headers = new List<string>();

            // Parse headers if available
            if (config.IncludeHeaders && result.TotalRowsInFile >= 0)
            {
                var headerLine = await System.IO.File.ReadAllLinesAsync(filePath, System.Text.Encoding.GetEncoding(config.Encoding));
                if (headerLine.Length > 0)
                {
                    headers = ParseCsvLine(headerLine[0], separator).ToList();
                    result.ColumnsCount = headers.Count;
                }
            }

            var rowIndex = 0;
            foreach (var line in lines)
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(line))
                    {
                        if (config.SkipEmptyRows)
                        {
                            result.SkippedRows++;
                            continue;
                        }
                    }

                    var fields = ParseCsvLine(line, separator).ToList();
                    var rowData = new Dictionary<string, object?>();

                    // Map fields to headers or use generic column names
                    for (int i = 0; i < fields.Count; i++)
                    {
                        var columnName = headers.Count > i ? headers[i] : $"Column{i + 1}";
                        rowData[columnName] = fields[i];
                    }

                    if (result.ColumnsCount == 0)
                        result.ColumnsCount = fields.Count;

                    // Validation if enabled
                    if (config.ValidateOnImport)
                    {
                        var validationErrors = ValidateImportRow(rowData, rowIndex);
                        if (validationErrors.Any())
                        {
                            result.ErrorRows++;
                            foreach (var error in validationErrors)
                            {
                                result.AddError(error, rowIndex);
                            }
                            
                            if (!config.ContinueOnErrors)
                            {
                                result.AddError($"Import zastavený kvôli chybám na riadku {rowIndex}", 
                                    severity: ErrorSeverity.Critical);
                                break;
                            }
                        }
                        else
                        {
                            result.SuccessfullyImportedRows++;
                            result.ImportedData.Add(rowData);
                        }
                    }
                    else
                    {
                        result.SuccessfullyImportedRows++;
                        result.ImportedData.Add(rowData);
                    }
                }
                catch (Exception ex)
                {
                    result.ErrorRows++;
                    result.AddError($"Chyba pri parsovaní riadku: {ex.Message}", rowIndex);
                    
                    if (!config.ContinueOnErrors)
                    {
                        result.AddError($"Import zastavený kvôli kritickej chybe na riadku {rowIndex}", 
                            severity: ErrorSeverity.Critical);
                        break;
                    }
                }

                rowIndex++;
            }
        }

        /// <summary>
        /// Importuje z JSON súboru
        /// </summary>
        private async Task ImportFromJsonAsync(string filePath, ImportExportConfiguration config, ImportResult result)
        {
            var jsonContent = await System.IO.File.ReadAllTextAsync(filePath, System.Text.Encoding.GetEncoding(config.Encoding));
            
            try
            {
                using var document = System.Text.Json.JsonDocument.Parse(jsonContent);
                
                if (document.RootElement.ValueKind == System.Text.Json.JsonValueKind.Array)
                {
                    result.TotalRowsInFile = document.RootElement.GetArrayLength();
                    var rowIndex = 0;

                    foreach (var item in document.RootElement.EnumerateArray())
                    {
                        try
                        {
                            var rowData = new Dictionary<string, object?>();
                            
                            foreach (var property in item.EnumerateObject())
                            {
                                object? value = property.Value.ValueKind switch
                                {
                                    System.Text.Json.JsonValueKind.String => property.Value.GetString(),
                                    System.Text.Json.JsonValueKind.Number => property.Value.GetDecimal(),
                                    System.Text.Json.JsonValueKind.True => true,
                                    System.Text.Json.JsonValueKind.False => false,
                                    System.Text.Json.JsonValueKind.Null => null,
                                    _ => property.Value.ToString()
                                };
                                
                                rowData[property.Name] = value;
                            }

                            if (result.ColumnsCount == 0)
                                result.ColumnsCount = rowData.Count;

                            // Validation if enabled
                            if (config.ValidateOnImport)
                            {
                                var validationErrors = ValidateImportRow(rowData, rowIndex);
                                if (validationErrors.Any())
                                {
                                    result.ErrorRows++;
                                    foreach (var error in validationErrors)
                                    {
                                        result.AddError(error, rowIndex);
                                    }
                                }
                                else
                                {
                                    result.SuccessfullyImportedRows++;
                                    result.ImportedData.Add(rowData);
                                }
                            }
                            else
                            {
                                result.SuccessfullyImportedRows++;
                                result.ImportedData.Add(rowData);
                            }
                        }
                        catch (Exception ex)
                        {
                            result.ErrorRows++;
                            result.AddError($"Chyba pri parsovaní JSON objektu: {ex.Message}", rowIndex);
                        }

                        rowIndex++;
                    }
                }
                else
                {
                    result.AddError("JSON súbor neobsahuje array objektov", severity: ErrorSeverity.Critical);
                }
            }
            catch (Exception ex)
            {
                result.AddError($"Chyba pri parsovaní JSON súboru: {ex.Message}", severity: ErrorSeverity.Critical);
            }
        }

        /// <summary>
        /// Exportuje do JSON formátu
        /// </summary>
        private async Task<string> ExportToJsonAsync(ImportExportConfiguration config)
        {
            var dataTable = await ExportToDataTableAsync();
            var jsonArray = new List<Dictionary<string, object?>>();

            foreach (DataRow row in dataTable.Rows)
            {
                var jsonObject = new Dictionary<string, object?>();
                
                foreach (DataColumn column in dataTable.Columns)
                {
                    var value = row[column.ColumnName];
                    jsonObject[column.ColumnName] = value == DBNull.Value ? null : value;
                }
                
                jsonArray.Add(jsonObject);
            }

            var options = new System.Text.Json.JsonSerializerOptions
            {
                WriteIndented = config.JsonFormatting == JsonFormatting.Indented,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };

            return System.Text.Json.JsonSerializer.Serialize(jsonArray, options);
        }

        /// <summary>
        /// Parsuje CSV riadok s rešpektovaním quoted fields
        /// </summary>
        private IEnumerable<string> ParseCsvLine(string line, string separator)
        {
            var fields = new List<string>();
            var currentField = new System.Text.StringBuilder();
            var inQuotes = false;
            var i = 0;

            while (i < line.Length)
            {
                var c = line[i];

                if (c == '"')
                {
                    if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                    {
                        // Escaped quote
                        currentField.Append('"');
                        i += 2;
                    }
                    else
                    {
                        // Toggle quote state
                        inQuotes = !inQuotes;
                        i++;
                    }
                }
                else if (!inQuotes && line.Substring(i).StartsWith(separator))
                {
                    // Field separator
                    fields.Add(currentField.ToString());
                    currentField.Clear();
                    i += separator.Length;
                }
                else
                {
                    currentField.Append(c);
                    i++;
                }
            }

            // Add last field
            fields.Add(currentField.ToString());
            return fields;
        }

        /// <summary>
        /// Validuje importovaný riadok
        /// </summary>
        private List<string> ValidateImportRow(Dictionary<string, object?> rowData, int rowIndex)
        {
            var errors = new List<string>();

            // Basic validation - can be extended based on configuration
            foreach (var kvp in rowData)
            {
                if (kvp.Value == null || string.IsNullOrWhiteSpace(kvp.Value.ToString()))
                {
                    // Only validate if column seems required (contains "id", "name", etc.)
                    var columnName = kvp.Key.ToLowerInvariant();
                    if (columnName.Contains("id") || columnName.Contains("name") || columnName.Contains("email"))
                    {
                        errors.Add($"{kvp.Key}: Pole je povinné");
                    }
                }
            }

            return errors;
        }

        /// <summary>
        /// Vytvorí backup súboru
        /// </summary>
        private async Task CreateFileBackupAsync(string filePath)
        {
            try
            {
                var backupPath = $"{filePath}.backup_{DateTime.Now:yyyyMMdd_HHmmss}";
                await Task.Run(() => System.IO.File.Copy(filePath, backupPath, overwrite: true));
                
                _logger.LogDebug("💾 File backup created - Original: {OriginalPath}, Backup: {BackupPath}", 
                    filePath, backupPath);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "💾 Failed to create file backup - FilePath: {FilePath}", filePath);
            }
        }

        /// <summary>
        /// Aktualizuje počítadlo importovaných bajtov
        /// </summary>
        private void TrackBytesImported(long bytes)
        {
            _totalBytesImported += bytes;
            _totalImportOperations++;
            _logger.LogTrace("📥 Bytes imported tracked - Current: {CurrentBytes}, Total: {TotalBytes}, TotalImportOps: {TotalOps}", 
                bytes, _totalBytesImported, _totalImportOperations);
        }

        #endregion

        #region ✅ Performance Tracking Helper Methods

        /// <summary>
        /// Spustí sledovanie operácie a vráti jej ID
        /// </summary>
        private string StartOperation(string operationName)
        {
            var operationId = $"{operationName}_{Guid.NewGuid():N}"[..16];
            _operationStartTimes[operationId] = DateTime.UtcNow;
            _operationCounters[operationName] = _operationCounters.GetValueOrDefault(operationName, 0) + 1;
            _totalExportOperations++;
            
            _logger.LogTrace("⏱️ Export Operation START - {OperationName} (ID: {OperationId}), " +
                "TotalCalls: {TotalCalls}, TotalExportOps: {TotalOps}",
                operationName, operationId, _operationCounters[operationName], _totalExportOperations);
                
            return operationId;
        }

        /// <summary>
        /// Ukončí sledovanie operácie a vráti dobu trvania v ms
        /// </summary>
        private double EndOperation(string operationId)
        {
            if (_operationStartTimes.TryGetValue(operationId, out var startTime))
            {
                var duration = (DateTime.UtcNow - startTime).TotalMilliseconds;
                _operationStartTimes.Remove(operationId);
                
                _logger.LogTrace("⏱️ Export Operation END - ID: {OperationId}, Duration: {Duration:F2}ms", 
                    operationId, duration);
                    
                return duration;
            }
            
            _logger.LogWarning("⏱️ Export Operation END - Unknown operation ID: {OperationId}", operationId);
            return 0;
        }

        /// <summary>
        /// Aktualizuje počítadlo exportovaných bajtov
        /// </summary>
        private void TrackBytesExported(long bytes)
        {
            _totalBytesExported += bytes;
            _logger.LogTrace("📊 Bytes exported tracked - Current: {CurrentBytes}, Total: {TotalBytes}", 
                bytes, _totalBytesExported);
        }

        #endregion

        #region Private Helper Methods

        /// <summary>
        /// Kontroluje či je riadok prázdny (Dictionary version)
        /// </summary>
        private bool IsRowEmpty(Dictionary<string, object?> row)
        {
            return row.Where(kvp => kvp.Key != "DeleteRows" && kvp.Key != "ValidAlerts")
                     .All(kvp => kvp.Value == null || string.IsNullOrWhiteSpace(kvp.Value.ToString()));
        }

        /// <summary>
        /// Kontroluje či je DataRow prázdny
        /// </summary>
        private bool IsDataRowEmpty(DataRow row)
        {
            foreach (DataColumn column in row.Table.Columns)
            {
                if (column.ColumnName == "ValidAlerts") continue;
                
                var value = row[column.ColumnName];
                if (value != null && value != DBNull.Value && !string.IsNullOrWhiteSpace(value.ToString()))
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Odhaduje veľkosť DataTable v bajtoch
        /// </summary>
        private long EstimateDataTableSize(DataTable dataTable)
        {
            long estimatedSize = 0;
            
            // Column headers
            estimatedSize += dataTable.Columns.Count * 50; // Average column name length
            
            // Row data
            foreach (DataRow row in dataTable.Rows)
            {
                foreach (var item in row.ItemArray)
                {
                    if (item != null && item != DBNull.Value)
                    {
                        estimatedSize += item.ToString()?.Length ?? 0;
                        estimatedSize += 10; // Overhead per cell
                    }
                }
            }
            
            return estimatedSize;
        }

        /// <summary>
        /// Kategorizuje typ validation error na základe správy
        /// </summary>
        private string CategorizeErrorType(string errorMessage)
        {
            var lowerMessage = errorMessage.ToLowerInvariant();
            
            if (lowerMessage.Contains("required") || lowerMessage.Contains("povinné"))
                return "Required";
            if (lowerMessage.Contains("email"))
                return "Email";
            if (lowerMessage.Contains("range") || lowerMessage.Contains("rozsah"))
                return "Range";
            if (lowerMessage.Contains("length") || lowerMessage.Contains("dĺžka"))
                return "Length";
            if (lowerMessage.Contains("pattern") || lowerMessage.Contains("formát"))
                return "Pattern";
            if (lowerMessage.Contains("custom"))
                return "Custom";
            
            return "Other";
        }

        /// <summary>
        /// Kategorizuje závažnosť validation error
        /// </summary>
        private string CategorizeErrorSeverity(string errorMessage)
        {
            var lowerMessage = errorMessage.ToLowerInvariant();
            
            if (lowerMessage.Contains("critical") || lowerMessage.Contains("kritické"))
                return "Critical";
            if (lowerMessage.Contains("required") || lowerMessage.Contains("povinné"))
                return "High";
            if (lowerMessage.Contains("invalid") || lowerMessage.Contains("neplatné"))
                return "Medium";
            
            return "Low";
        }

        private void CreateDataTableStructure(DataTable dataTable, bool includeValidAlerts = false)
        {
            if (_configuration?.Columns == null) return;

            foreach (var column in _configuration.Columns)
            {
                // Preskač DeleteRows stĺpec a CheckBoxState (ak nie je explicitne požadovaný)
                if (column.Name == "DeleteRows") continue;
                if (column.Name == "CheckBoxState") continue;

                dataTable.Columns.Add(column.Name, column.DataType);
            }

            // Pridaj ValidAlerts stĺpec len ak je požadovaný
            if (includeValidAlerts)
            {
                dataTable.Columns.Add("ValidAlerts", typeof(string));
            }
        }

        private void PopulateDataTable(DataTable dataTable, List<Dictionary<string, object?>> allData, bool includeValidAlerts = false)
        {
            foreach (var rowData in allData)
            {
                var dataRow = dataTable.NewRow();

                foreach (DataColumn column in dataTable.Columns)
                {
                    // Skip ValidAlerts if not included
                    if (!includeValidAlerts && column.ColumnName == "ValidAlerts") continue;
                    
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

        private Models.ImportExport.ExportStatistics CalculateStatistics(DataTable dataTable)
        {
            var statistics = new Models.ImportExport.ExportStatistics
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
                    ValueCount = 0,
                    NullCount = 0,
                    UniqueCount = 0,
                    EmptyCount = 0
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
                        columnStats.ValueCount++;
                        uniqueValues.Add(value);
                    }
                }

                columnStats.UniqueCount = uniqueValues.Count;
                statistics.ColumnStatistics[column.ColumnName] = columnStats;
            }

            return statistics;
        }

        #endregion
    }

    /// <summary>
    /// ✅ OPRAVENÉ: Štatistiky exportu - INTERNAL (nie PUBLIC)
    /// </summary>
    internal class ExportStatistics
    {
        public int TotalRows { get; set; }
        public int TotalColumns { get; set; }
        public int ValidRows { get; set; }
        public int InvalidRows { get; set; }
        public int EmptyRows { get; set; }
        public Dictionary<string, ColumnStatistics> ColumnStatistics { get; set; } = new();

        public override string ToString()
        {
            return $"Export: {TotalRows} rows, {TotalColumns} columns, {ValidRows} valid, {InvalidRows} invalid";
        }
    }

}