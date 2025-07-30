// Services/DataManagementService.cs - ✅ NEZÁVISLÝ s ILogger<T>
using Microsoft.Extensions.Logging;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Models;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Services.Interfaces;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Services
{
    /// <summary>
    /// Implementácia služby pre správu dát v DataGrid s Auto-Add funkciou - INTERNAL
    /// ✅ NEZÁVISLÝ KOMPONENT s ILogger<DataManagementService>
    /// </summary>
    internal class DataManagementService : IDataManagementService
    {
        private readonly ILogger<DataManagementService> _logger;
        private readonly List<Dictionary<string, object?>> _gridData = new();
        private readonly Dictionary<string, Type> _columnTypes = new();
        private readonly ResourceCleanupHelper _cleanupHelper;

        private GridConfiguration? _configuration;
        private bool _isInitialized = false;
        private readonly object _dataLock = new object();

        // ✅ Auto-Add state tracking
        private int _minimumRowCount = 15;
        private bool _autoAddEnabled = true;

        public DataManagementService(ILogger<DataManagementService> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _cleanupHelper = new ResourceCleanupHelper();

            _logger.LogDebug("🔧 DataManagementService created with logger: {LoggerType}", logger.GetType().Name);
        }

        #region Explicit Interface Implementation

        Task IDataManagementService.InitializeAsync(GridConfiguration configuration)
        {
            return InitializeInternalAsync(configuration);
        }

        Task IDataManagementService.LoadDataAsync(List<Dictionary<string, object?>> data)
        {
            return LoadDataInternalAsync(data);
        }

        Task<List<Dictionary<string, object?>>> IDataManagementService.GetAllDataAsync()
        {
            return GetAllDataInternalAsync();
        }

        Task<Dictionary<string, object?>> IDataManagementService.GetRowDataAsync(int rowIndex)
        {
            return GetRowDataInternalAsync(rowIndex);
        }

        Task IDataManagementService.SetCellValueAsync(int rowIndex, string columnName, object? value)
        {
            return SetCellValueInternalAsync(rowIndex, columnName, value);
        }

        Task<object?> IDataManagementService.GetCellValueAsync(int rowIndex, string columnName)
        {
            return GetCellValueInternalAsync(rowIndex, columnName);
        }

        Task<int> IDataManagementService.AddRowAsync(Dictionary<string, object?>? initialData)
        {
            return AddRowInternalAsync(initialData);
        }

        Task IDataManagementService.DeleteRowAsync(int rowIndex)
        {
            return DeleteRowInternalAsync(rowIndex);
        }

        Task IDataManagementService.ClearAllDataAsync()
        {
            return ClearAllDataInternalAsync();
        }

        Task IDataManagementService.CompactRowsAsync()
        {
            return CompactRowsInternalAsync();
        }

        Task<int> IDataManagementService.GetNonEmptyRowCountAsync()
        {
            return GetNonEmptyRowCountInternalAsync();
        }

        Task<bool> IDataManagementService.IsRowEmptyAsync(int rowIndex)
        {
            return IsRowEmptyInternalAsync(rowIndex);
        }

        #endregion

        #region ✅ KOMPLETNÁ Auto-Add Implementation s logovaním

        private async Task InitializeInternalAsync(GridConfiguration configuration)
        {
            try
            {
                _logger.LogInformation("🚀 DataManagementService.InitializeAsync START - Columns: {ColumnCount}",
                    configuration?.Columns?.Count ?? 0);

                _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));

                lock (_dataLock)
                {
                    // Vyčisti existujúce dáta
                    var oldDataCount = _gridData.Count;
                    var oldColumnCount = _columnTypes.Count;

                    _gridData.Clear();
                    _columnTypes.Clear();

                    _logger.LogDebug("🧹 Cleared existing data - Rows: {OldRows}, Columns: {OldColumns}",
                        oldDataCount, oldColumnCount);

                    // ✅ Nastav Auto-Add parametre
                    _minimumRowCount = Math.Max(_configuration.EmptyRowsCount, 1);
                    _autoAddEnabled = _configuration.AutoAddNewRow;

                    _logger.LogInformation("⚙️ Auto-Add configured - MinRows: {MinRows}, Enabled: {Enabled}",
                        _minimumRowCount, _autoAddEnabled);

                    // Načítaj typy stĺpcov z konfigurácie
                    foreach (var column in _configuration.Columns)
                    {
                        _columnTypes[column.Name] = column.DataType;
                        _logger.LogDebug("📊 Column registered: {ColumnName} ({DataType})", column.Name, column.DataType.Name);
                    }

                    // ✅ KĽÚČOVÉ: Vytvor minimálny počet prázdnych riadkov + 1 extra pre auto-add
                    var initialRowCount = _autoAddEnabled ? _minimumRowCount + 1 : _minimumRowCount;
                    for (int i = 0; i < initialRowCount; i++)
                    {
                        _gridData.Add(CreateEmptyRow());
                    }

                    _logger.LogInformation("📄 Created {InitialRows} initial rows ({MinRows} minimum + {ExtraRows} extra)",
                        _gridData.Count, _minimumRowCount, _autoAddEnabled ? 1 : 0);
                }

                _isInitialized = true;
                _logger.LogInformation("✅ DataManagementService initialized successfully - Total rows: {TotalRows}", _gridData.Count);

                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ CRITICAL ERROR during DataManagementService initialization");
                throw;
            }
        }

        /// <summary>
        /// ✅ KOMPLETNÁ Auto-Add logika pri načítaní dát s detailným logovaním
        /// </summary>
        private async Task LoadDataInternalAsync(List<Dictionary<string, object?>> data)
        {
            try
            {
                EnsureInitialized();

                if (data == null)
                {
                    _logger.LogWarning("⚠️ LoadDataAsync: Null data provided, using empty list");
                    data = new List<Dictionary<string, object?>>();
                }

                _logger.LogInformation("📊 LoadDataAsync START - Input rows: {InputRows}, Minimum required: {MinRows}",
                    data.Count, _minimumRowCount);

                await Task.Run(() =>
                {
                    lock (_dataLock)
                    {
                        // ✅ Auto-Add logika:
                        var dataRowsNeeded = data.Count;
                        var totalRowsNeeded = Math.Max(dataRowsNeeded + 1, _minimumRowCount + 1); // +1 pre prázdny

                        _logger.LogDebug("📐 Auto-Add calculation - Data rows: {DataRows}, Total needed: {TotalRows}",
                            dataRowsNeeded, totalRowsNeeded);

                        // Vyčisti existujúce dáta
                        var oldRowCount = _gridData.Count;
                        _gridData.Clear();

                        // Načítaj skutočné dáta s validáciou
                        for (int i = 0; i < dataRowsNeeded; i++)
                        {
                            try
                            {
                                var processedRow = ProcessAndValidateRowData(data[i]);
                                _gridData.Add(processedRow);

                                // Log sample data pre debugging
                                if (i < 3) // Log prvých 3 riadkov
                                {
                                    var sampleData = string.Join(", ", processedRow.Take(3).Select(kvp => $"{kvp.Key}={kvp.Value}"));
                                    _logger.LogDebug("📝 Row[{RowIndex}] sample: {SampleData}...", i, sampleData);
                                }
                            }
                            catch (Exception rowEx)
                            {
                                _logger.LogError(rowEx, "❌ Error processing row {RowIndex}", i);
                                // Pridaj prázdny riadok namiesto chybného
                                _gridData.Add(CreateEmptyRow());
                            }
                        }

                        // Pridaj potrebné prázdne riadky
                        var emptyRowsToAdd = totalRowsNeeded - dataRowsNeeded;
                        for (int i = 0; i < emptyRowsToAdd; i++)
                        {
                            _gridData.Add(CreateEmptyRow());
                        }

                        _logger.LogInformation("✅ LoadDataAsync COMPLETED - {OldRows} → {NewRows} rows ({DataRows} with data, {EmptyRows} empty)",
                            oldRowCount, _gridData.Count, dataRowsNeeded, emptyRowsToAdd);
                    }
                });

                // Memory cleanup
                await _cleanupHelper.ForceGarbageCollectionAsync();
                _logger.LogDebug("🧹 Memory cleanup completed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ CRITICAL ERROR in LoadDataAsync");
                throw;
            }
        }

        /// <summary>
        /// ✅ Inteligentné nastavenie hodnoty bunky s Auto-Add kontrolou a logovaním
        /// </summary>
        private async Task SetCellValueInternalAsync(int rowIndex, string columnName, object? value)
        {
            try
            {
                EnsureInitialized();

                if (string.IsNullOrWhiteSpace(columnName))
                {
                    _logger.LogError("❌ SetCellValueAsync: Empty column name provided");
                    throw new ArgumentException("ColumnName nemôže byť prázdny", nameof(columnName));
                }

                _logger.LogDebug("📝 SetCellValue START - [{RowIndex}, {ColumnName}] = '{Value}' (Type: {ValueType})",
                    rowIndex, columnName, value, value?.GetType().Name ?? "null");

                lock (_dataLock)
                {
                    if (rowIndex < 0 || rowIndex >= _gridData.Count)
                    {
                        _logger.LogError("❌ SetCellValue: Invalid row index {RowIndex} (valid range: 0-{MaxIndex})",
                            rowIndex, _gridData.Count - 1);
                        return;
                    }

                    var row = _gridData[rowIndex];
                    var convertedValue = ConvertValueToColumnType(columnName, value);
                    var oldValue = row.ContainsKey(columnName) ? row[columnName] : null;

                    row[columnName] = convertedValue;

                    // Log value change ak sa skutočne zmenila
                    if (!Equals(oldValue, convertedValue))
                    {
                        _logger.LogDebug("💾 Cell value changed: [{RowIndex}, {ColumnName}] '{OldValue}' → '{NewValue}'",
                            rowIndex, columnName, oldValue, convertedValue);
                    }

                    // ✅ KĽÚČOVÁ Auto-Add logika: Kontrola či treba pridať nový prázdny riadok
                    if (_autoAddEnabled && !IsSpecialColumn(columnName))
                    {
                        // Ak editujeme posledný riadok a nie je už prázdny
                        if (rowIndex == _gridData.Count - 1 && !IsRowEmpty(row))
                        {
                            // Pridaj nový prázdny riadok
                            _gridData.Add(CreateEmptyRow());
                            _logger.LogInformation("🔄 Auto-Add: Last row filled → added new empty row (total: {TotalRows})", _gridData.Count);
                        }
                    }
                }

                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ ERROR in SetCellValueAsync [{RowIndex}, {ColumnName}]", rowIndex, columnName);
                throw;
            }
        }

        /// <summary>
        /// ✅ Inteligentné mazanie s Auto-Add ochranou a detailným logovaním
        /// </summary>
        private async Task DeleteRowInternalAsync(int rowIndex)
        {
            try
            {
                EnsureInitialized();

                _logger.LogInformation("🗑️ DeleteRowAsync START - RowIndex: {RowIndex}, CurrentRows: {CurrentRows}, MinRows: {MinRows}",
                    rowIndex, _gridData.Count, _minimumRowCount);

                await Task.Run(() =>
                {
                    lock (_dataLock)
                    {
                        if (rowIndex < 0 || rowIndex >= _gridData.Count)
                        {
                            _logger.LogError("❌ DeleteRow: Invalid row index {RowIndex} (valid range: 0-{MaxIndex})",
                                rowIndex, _gridData.Count - 1);
                            return;
                        }

                        var currentRowCount = _gridData.Count;
                        var isRowEmpty = IsRowEmpty(_gridData[rowIndex]);

                        _logger.LogDebug("📊 Row analysis - Index: {RowIndex}, IsEmpty: {IsEmpty}, CanPhysicallyDelete: {CanDelete}",
                            rowIndex, isRowEmpty, currentRowCount > _minimumRowCount);

                        // ✅ Auto-Add inteligentné mazanie:
                        if (currentRowCount > _minimumRowCount)
                        {
                            // Máme viac ako minimum → fyzicky zmaž riadok
                            _gridData.RemoveAt(rowIndex);
                            _logger.LogInformation("🗑️ Auto-Add: Row physically deleted - {RowIndex} removed (remaining: {RemainingRows})",
                                rowIndex, _gridData.Count);
                        }
                        else
                        {
                            // Sme na minimume → len vyčisti obsah riadku
                            var emptyRow = CreateEmptyRow();
                            _gridData[rowIndex] = emptyRow;
                            _logger.LogInformation("🧹 Auto-Add: Row content cleared - {RowIndex} (minimum {MinRows} preserved)",
                                rowIndex, _minimumRowCount);
                        }

                        // ✅ Zabezpeč že je aspoň jeden prázdny riadok na konci
                        if (_autoAddEnabled)
                        {
                            CheckAndAddEmptyRowIfNeeded();
                        }
                    }
                });

                // Kompaktuj riadky po mazaní
                await CompactRowsInternalAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ ERROR in DeleteRowAsync - RowIndex: {RowIndex}", rowIndex);
                throw;
            }
        }

        /// <summary>
        /// ✅ Vymazanie všetkých dát s rešpektovaním minimálneho počtu a logovaním
        /// </summary>
        private async Task ClearAllDataInternalAsync()
        {
            try
            {
                EnsureInitialized();

                _logger.LogInformation("🧹 ClearAllDataAsync START - Current rows: {CurrentRows}", _gridData.Count);

                await Task.Run(async () =>
                {
                    lock (_dataLock)
                    {
                        var oldRowCount = _gridData.Count;

                        // Vyčisti všetky dáta
                        _gridData.Clear();

                        // ✅ Auto-Add logika: Vytvor minimálny počet prázdnych riadkov + 1 extra ak je auto-add
                        var requiredRows = _autoAddEnabled ? _minimumRowCount + 1 : _minimumRowCount;
                        for (int i = 0; i < requiredRows; i++)
                        {
                            _gridData.Add(CreateEmptyRow());
                        }

                        _logger.LogInformation("✅ ClearAllData COMPLETED - {OldRows} → {NewRows} rows (reset to initial state)",
                            oldRowCount, _gridData.Count);
                    }

                    // Memory cleanup
                    await _cleanupHelper.ForceGarbageCollectionAsync();
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ ERROR in ClearAllDataAsync");
                throw;
            }
        }

        /// <summary>
        /// ✅ Kompaktovanie s Auto-Add logikou a logovaním
        /// </summary>
        private Task CompactRowsInternalAsync()
        {
            try
            {
                EnsureInitialized();

                _logger.LogDebug("🔄 CompactRowsAsync START - Current rows: {CurrentRows}", _gridData.Count);

                return Task.Run(() =>
                {
                    lock (_dataLock)
                    {
                        var nonEmptyRows = new List<Dictionary<string, object?>>();

                        // Rozdeľ na neprázdne a prázdne riadky
                        foreach (var row in _gridData)
                        {
                            if (!IsRowEmpty(row))
                            {
                                nonEmptyRows.Add(row);
                            }
                        }

                        _logger.LogDebug("📊 Compacting analysis - Total: {TotalRows}, NonEmpty: {NonEmptyRows}, Empty: {EmptyRows}",
                            _gridData.Count, nonEmptyRows.Count, _gridData.Count - nonEmptyRows.Count);

                        // Vyčisti kolekciu a pridaj najprv neprázdne riadky
                        _gridData.Clear();
                        _gridData.AddRange(nonEmptyRows);

                        // ✅ Auto-Add logika: Pridaj potrebný počet prázdnych riadkov
                        var requiredEmptyRows = Math.Max(_minimumRowCount - nonEmptyRows.Count, 1); // Aspoň 1 prázdny

                        for (int i = 0; i < requiredEmptyRows; i++)
                        {
                            _gridData.Add(CreateEmptyRow());
                        }

                        _logger.LogInformation("✅ CompactRows COMPLETED - {NonEmptyRows} data + {EmptyRows} empty = {TotalRows} rows",
                            nonEmptyRows.Count, requiredEmptyRows, _gridData.Count);
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ ERROR in CompactRowsAsync");
                throw;
            }
        }

        #endregion

        #region ✅ Auto-Add Helper Methods s logovaním

        /// <summary>
        /// Skontroluje či je stĺpec špeciálny (neráta sa do Auto-Add logiky)
        /// </summary>
        private bool IsSpecialColumn(string columnName)
        {
            var isSpecial = columnName == "DeleteRows" || columnName == "ValidAlerts";
            if (isSpecial)
            {
                _logger.LogTrace("🔍 Special column detected: {ColumnName}", columnName);
            }
            return isSpecial;
        }

        /// <summary>
        /// Kontroluje či je riadok úplne prázdny (pre Auto-Add logiku)
        /// </summary>
        private bool IsRowEmpty(Dictionary<string, object?> row)
        {
            foreach (var kvp in row)
            {
                var columnName = kvp.Key;
                var value = kvp.Value;

                // Ignoruj špeciálne stĺpce
                if (IsSpecialColumn(columnName))
                    continue;

                // Ak je nejaká hodnota vyplnená, riadok nie je prázdny
                if (value != null && !string.IsNullOrWhiteSpace(value.ToString()))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// ✅ KĽÚČOVÁ: Kontroluje či treba pridať nový prázdny riadok na koniec
        /// </summary>
        private void CheckAndAddEmptyRowIfNeeded()
        {
            if (!_autoAddEnabled || _gridData.Count == 0) return;

            // Kontrola: Je posledný riadok prázdny?
            var lastRow = _gridData[^1]; // C# 8.0 syntax
            var isLastRowEmpty = IsRowEmpty(lastRow);

            if (!isLastRowEmpty)
            {
                // Posledný riadok nie je prázdny → pridaj nový prázdny
                var newEmptyRow = CreateEmptyRow();
                _gridData.Add(newEmptyRow);

                _logger.LogDebug("🔄 Auto-Add: Empty row added automatically at index {Index} (total: {TotalRows})",
                    _gridData.Count - 1, _gridData.Count);
            }
        }

        /// <summary>
        /// Získa count neprázdnych dátových riadkov
        /// </summary>
        private int GetNonEmptyDataRowCount()
        {
            lock (_dataLock)
            {
                var count = _gridData.Count(row => !IsRowEmpty(row));
                _logger.LogTrace("📊 Non-empty row count: {Count}", count);
                return count;
            }
        }

        /// <summary>
        /// Získa informácie o Auto-Add stave pre diagnostiku
        /// </summary>
        public string GetAutoAddStatus()
        {
            lock (_dataLock)
            {
                var nonEmptyCount = GetNonEmptyDataRowCount();
                var emptyCount = _gridData.Count - nonEmptyCount;

                return $"AUTO-ADD Status: {_gridData.Count} total ({nonEmptyCount} data, {emptyCount} empty), min: {_minimumRowCount}, enabled: {_autoAddEnabled}";
            }
        }

        #endregion

        #region Standard Implementation Methods s logovaním

        private Task<List<Dictionary<string, object?>>> GetAllDataInternalAsync()
        {
            try
            {
                EnsureInitialized();

                List<Dictionary<string, object?>> result;

                lock (_dataLock)
                {
                    // Vytvor deep copy dát
                    result = _gridData.Select(row => new Dictionary<string, object?>(row)).ToList();
                }

                _logger.LogDebug("📤 GetAllDataAsync returning {RowCount} rows", result.Count);
                return Task.FromResult(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ ERROR in GetAllDataAsync");
                throw;
            }
        }

        private Task<Dictionary<string, object?>> GetRowDataInternalAsync(int rowIndex)
        {
            try
            {
                EnsureInitialized();

                lock (_dataLock)
                {
                    if (rowIndex < 0 || rowIndex >= _gridData.Count)
                    {
                        _logger.LogWarning("⚠️ GetRowData: Invalid row index {RowIndex} (valid: 0-{MaxIndex})",
                            rowIndex, _gridData.Count - 1);
                        return Task.FromResult(new Dictionary<string, object?>());
                    }

                    var result = new Dictionary<string, object?>(_gridData[rowIndex]);
                    _logger.LogDebug("📤 GetRowData[{RowIndex}] - {CellCount} cells", rowIndex, result.Count);
                    return Task.FromResult(result);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ ERROR in GetRowDataAsync - RowIndex: {RowIndex}", rowIndex);
                throw;
            }
        }

        private Task<object?> GetCellValueInternalAsync(int rowIndex, string columnName)
        {
            try
            {
                EnsureInitialized();

                if (string.IsNullOrWhiteSpace(columnName))
                {
                    _logger.LogError("❌ GetCellValue: Empty column name");
                    throw new ArgumentException("ColumnName nemôže byť prázdny", nameof(columnName));
                }

                lock (_dataLock)
                {
                    if (rowIndex < 0 || rowIndex >= _gridData.Count)
                    {
                        _logger.LogWarning("⚠️ GetCellValue: Invalid row index {RowIndex}", rowIndex);
                        return Task.FromResult<object?>(null);
                    }

                    var row = _gridData[rowIndex];
                    var value = row.ContainsKey(columnName) ? row[columnName] : null;

                    _logger.LogTrace("📤 GetCellValue[{RowIndex}, {ColumnName}] = '{Value}'", rowIndex, columnName, value);
                    return Task.FromResult(value);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ ERROR in GetCellValueAsync [{RowIndex}, {ColumnName}]", rowIndex, columnName);
                throw;
            }
        }

        private Task<int> AddRowInternalAsync(Dictionary<string, object?>? initialData)
        {
            try
            {
                EnsureInitialized();

                _logger.LogDebug("➕ AddRowAsync START - HasInitialData: {HasData}", initialData != null);

                int newRowIndex = -1;

                await Task.Run(() =>
                {
                    lock (_dataLock)
                    {
                        // Kontrola maxRows limitu
                        if (_configuration!.MaxRows > 0 && _gridData.Count >= _configuration.MaxRows)
                        {
                            _logger.LogWarning("⚠️ AddRow: Maximum row limit reached ({MaxRows})", _configuration.MaxRows);
                            newRowIndex = -1;
                            return;
                        }

                        Dictionary<string, object?> newRow;

                        if (initialData != null)
                        {
                            newRow = ProcessAndValidateRowData(initialData);
                            _logger.LogDebug("📝 AddRow: Processing row with {CellCount} initial values", initialData.Count);
                        }
                        else
                        {
                            newRow = CreateEmptyRow();
                            _logger.LogDebug("📄 AddRow: Creating empty row");
                        }

                        _gridData.Add(newRow);
                        newRowIndex = _gridData.Count - 1;

                        // ✅ Auto-Add logika: Ak pridávame dátový riadok, zabezpeč prázdny na konci
                        if (_autoAddEnabled && initialData != null)
                        {
                            CheckAndAddEmptyRowIfNeeded();
                        }

                        _logger.LogInformation("✅ AddRow COMPLETED - New row at index {RowIndex} (total: {TotalRows})",
                            newRowIndex, _gridData.Count);
                    }
                });

                return newRowIndex;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ ERROR in AddRowAsync");
                throw;
            }
        }

        private Task<int> GetNonEmptyRowCountInternalAsync()
        {
            try
            {
                EnsureInitialized();

                int count;

                lock (_dataLock)
                {
                    count = GetNonEmptyDataRowCount();
                }

                _logger.LogDebug("📊 NonEmptyRowCount: {Count}", count);
                return Task.FromResult(count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ ERROR in GetNonEmptyRowCountAsync");
                throw;
            }
        }

        private Task<bool> IsRowEmptyInternalAsync(int rowIndex)
        {
            try
            {
                EnsureInitialized();

                lock (_dataLock)
                {
                    if (rowIndex < 0 || rowIndex >= _gridData.Count)
                    {
                        _logger.LogWarning("⚠️ IsRowEmpty: Invalid row index {RowIndex}", rowIndex);
                        return Task.FromResult(true);
                    }

                    var isEmpty = IsRowEmpty(_gridData[rowIndex]);
                    _logger.LogDebug("🔍 IsRowEmpty[{RowIndex}]: {IsEmpty}", rowIndex, isEmpty);
                    return Task.FromResult(isEmpty);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ ERROR in IsRowEmptyAsync - RowIndex: {RowIndex}", rowIndex);
                throw;
            }
        }

        #endregion

        #region Private Helper Methods

        private void EnsureInitialized()
        {
            if (!_isInitialized)
            {
                _logger.LogError("❌ DataManagementService not initialized - call InitializeAsync() first");
                throw new InvalidOperationException("DataManagementService nie je inicializovaný. Zavolajte InitializeAsync() najprv.");
            }
        }

        private Dictionary<string, object?> CreateEmptyRow()
        {
            var row = new Dictionary<string, object?>();

            if (_configuration?.Columns != null)
            {
                foreach (var column in _configuration.Columns)
                {
                    row[column.Name] = column.DefaultValue;
                }

                // Pridaj ValidAlerts stĺpec
                row["ValidAlerts"] = string.Empty;
            }

            _logger.LogTrace("📄 Created empty row with {CellCount} cells", row.Count);
            return row;
        }

        private Dictionary<string, object?> ProcessAndValidateRowData(Dictionary<string, object?> rowData)
        {
            var processedRow = CreateEmptyRow();

            foreach (var kvp in rowData)
            {
                var columnName = kvp.Key;
                var value = kvp.Value;

                try
                {
                    // Konvertuj hodnotu na správny typ pre stĺpec
                    var convertedValue = ConvertValueToColumnType(columnName, value);
                    processedRow[columnName] = convertedValue;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "⚠️ Failed to convert value for column {ColumnName}: {Value}", columnName, value);
                    // Použij originálnu hodnotu ak konverzia zlyhá
                    processedRow[columnName] = value;
                }
            }

            return processedRow;
        }

        private object? ConvertValueToColumnType(string columnName, object? value)
        {
            if (value == null) return null;

            if (_columnTypes.ContainsKey(columnName))
            {
                var targetType = _columnTypes[columnName];
                try
                {
                    var convertedValue = DataTypeConverter.ConvertValue(value, targetType);
                    _logger.LogTrace("🔄 Type conversion: {ColumnName} {OriginalType} → {TargetType}",
                        columnName, value.GetType().Name, targetType.Name);
                    return convertedValue;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "⚠️ Type conversion failed for {ColumnName}: {Value} → {TargetType}",
                        columnName, value, targetType.Name);
                }
            }

            return value;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Celkový počet riadkov
        /// </summary>
        public int TotalRowCount
        {
            get
            {
                lock (_dataLock)
                {
                    return _gridData.Count;
                }
            }
        }

        /// <summary>
        /// Počet stĺpcov
        /// </summary>
        public int ColumnCount => _columnTypes.Count;

        /// <summary>
        /// Názvy všetkých stĺpcov
        /// </summary>
        public IReadOnlyList<string> ColumnNames => _columnTypes.Keys.ToList();

        /// <summary>
        /// Auto-Add informácie
        /// </summary>
        public bool IsAutoAddEnabled => _autoAddEnabled;
        public int MinimumRowCount => _minimumRowCount;

        #endregion
    }
}