// Services/DataManagementService.cs - ✅ KOMPLETNÁ Auto-Add riadkov implementácia
using Microsoft.Extensions.Logging;
using RpaWinUiComponents.AdvancedWinUiDataGrid.Models;
using RpaWinUiComponents.AdvancedWinUiDataGrid.Services.Interfaces;
using RpaWinUiComponents.AdvancedWinUiDataGrid.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RpaWinUiComponents.AdvancedWinUiDataGrid.Services
{
    /// <summary>
    /// Implementácia služby pre správu dát v DataGrid s kompletnou Auto-Add funkciou - INTERNAL
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

        // ✅ NOVÉ: Auto-Add state tracking
        private int _minimumRowCount = 15;
        private bool _autoAddEnabled = true;

        public DataManagementService(ILogger<DataManagementService> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _cleanupHelper = new ResourceCleanupHelper();
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

        #region ✅ KOMPLETNÁ Auto-Add Implementation

        private async Task InitializeInternalAsync(GridConfiguration configuration)
        {
            try
            {
                _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));

                lock (_dataLock)
                {
                    // Vyčisti existujúce dáta
                    _gridData.Clear();
                    _columnTypes.Clear();

                    // ✅ Nastav Auto-Add parametre
                    _minimumRowCount = Math.Max(_configuration.EmptyRowsCount, 1);
                    _autoAddEnabled = _configuration.AutoAddNewRow;

                    // Načítaj typy stĺpcov z konfigurácie
                    foreach (var column in _configuration.Columns)
                    {
                        _columnTypes[column.Name] = column.DataType;
                    }

                    // ✅ KĽÚČOVÉ: Vytvor minimálny počet prázdnych riadkov + 1 extra pre auto-add
                    var initialRowCount = _autoAddEnabled ? _minimumRowCount + 1 : _minimumRowCount;
                    for (int i = 0; i < initialRowCount; i++)
                    {
                        _gridData.Add(CreateEmptyRow());
                    }
                }

                _isInitialized = true;
                _logger.LogInformation("DataManagementService inicializovaný s {ColumnCount} stĺpcami, {InitialRows} riadkami ({MinimumRows} minimum + {ExtraRows} extra), Auto-Add: {AutoAddEnabled}",
                    _configuration.Columns.Count, _gridData.Count, _minimumRowCount, _autoAddEnabled ? 1 : 0, _autoAddEnabled);

                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Chyba pri inicializácii DataManagementService");
                throw;
            }
        }

        /// <summary>
        /// ✅ KOMPLETNÁ Auto-Add logika pri načítaní dát
        /// </summary>
        private async Task LoadDataInternalAsync(List<Dictionary<string, object?>> data)
        {
            try
            {
                EnsureInitialized();

                if (data == null)
                {
                    _logger.LogWarning("Pokus o načítanie null dát");
                    return;
                }

                await Task.Run(() =>
                {
                    lock (_dataLock)
                    {
                        _logger.LogInformation("AUTO-ADD: Načítavajú sa dáta: {RowCount} riadkov (minimum: {MinimumRows})",
                            data.Count, _minimumRowCount);

                        // ✅ Auto-Add logika:
                        // 1. Ak má viac dát ako minimum → vytvor dáta + 1 prázdny
                        // 2. Ak má menej dát ako minimum → vytvor minimum riadkov + 1 prázdny
                        // 3. Vždy aspoň jeden prázdny riadok na konci

                        var dataRowsNeeded = data.Count;
                        var totalRowsNeeded = Math.Max(dataRowsNeeded + 1, _minimumRowCount + 1); // +1 pre prázdny

                        _logger.LogDebug("AUTO-ADD: Potrebných {TotalRows} riadkov ({DataRows} s dátami + {EmptyRows} prázdnych)",
                            totalRowsNeeded, dataRowsNeeded, totalRowsNeeded - dataRowsNeeded);

                        // Vyčisti existujúce dáta
                        _gridData.Clear();

                        // Načítaj skutočné dáta
                        for (int i = 0; i < dataRowsNeeded; i++)
                        {
                            var processedRow = ProcessAndValidateRowData(data[i]);
                            _gridData.Add(processedRow);
                            _logger.LogDebug("AUTO-ADD: Načítaný dátový riadok {Index}", i + 1);
                        }

                        // Pridaj potrebné prázdne riadky
                        var emptyRowsToAdd = totalRowsNeeded - dataRowsNeeded;
                        for (int i = 0; i < emptyRowsToAdd; i++)
                        {
                            _gridData.Add(CreateEmptyRow());
                            _logger.LogDebug("AUTO-ADD: Vytvorený prázdny riadok {Index}", dataRowsNeeded + i + 1);
                        }

                        _logger.LogInformation("AUTO-ADD dokončené: {TotalRows} riadkov ({DataRows} s dátami, {EmptyRows} prázdnych)",
                            _gridData.Count, dataRowsNeeded, emptyRowsToAdd);
                    }
                });

                // Vyvolaj garbage collection pre uvoľnenie pamäte
                await _cleanupHelper.ForceGarbageCollectionAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Chyba pri načítavaní dát s Auto-Add");
                throw;
            }
        }

        /// <summary>
        /// ✅ NOVÁ: Inteligentné nastavenie hodnoty bunky s Auto-Add kontrolou
        /// </summary>
        private async Task SetCellValueInternalAsync(int rowIndex, string columnName, object? value)
        {
            try
            {
                EnsureInitialized();

                if (string.IsNullOrWhiteSpace(columnName))
                    throw new ArgumentException("ColumnName nemôže byť prázdny", nameof(columnName));

                lock (_dataLock)
                {
                    if (rowIndex < 0 || rowIndex >= _gridData.Count)
                    {
                        _logger.LogWarning("AUTO-ADD: Neplatný index riadku pri nastavovaní hodnoty: {RowIndex}", rowIndex);
                        return;
                    }

                    var row = _gridData[rowIndex];
                    var convertedValue = ConvertValueToColumnType(columnName, value);
                    var oldValue = row.ContainsKey(columnName) ? row[columnName] : null;

                    row[columnName] = convertedValue;

                    _logger.LogDebug("AUTO-ADD: Nastavená hodnota bunky [{RowIndex}, {ColumnName}] = {Value}",
                        rowIndex, columnName, convertedValue);

                    // ✅ KĽÚČOVÁ Auto-Add logika: Kontrola či treba pridať nový prázdny riadok
                    if (_autoAddEnabled && !IsSpecialColumn(columnName))
                    {
                        // Ak editujeme posledný riadok a nie je už prázdny
                        if (rowIndex == _gridData.Count - 1 && !IsRowEmpty(row))
                        {
                            // Pridaj nový prázdny riadok
                            _gridData.Add(CreateEmptyRow());
                            _logger.LogDebug("AUTO-ADD: Vyplnený posledný riadok → pridaný nový prázdny (celkom: {TotalRows})", _gridData.Count);
                        }
                    }
                }

                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Chyba pri nastavovaní hodnoty bunky [{RowIndex}, {ColumnName}]", rowIndex, columnName);
                throw;
            }
        }

        /// <summary>
        /// ✅ KOMPLETNÁ: Inteligentné mazanie s Auto-Add ochranou
        /// </summary>
        private async Task DeleteRowInternalAsync(int rowIndex)
        {
            try
            {
                EnsureInitialized();

                await Task.Run(() =>
                {
                    lock (_dataLock)
                    {
                        if (rowIndex < 0 || rowIndex >= _gridData.Count)
                        {
                            _logger.LogWarning("AUTO-ADD: Neplatný index riadku pri mazaní: {RowIndex}", rowIndex);
                            return;
                        }

                        var currentRowCount = _gridData.Count;

                        // ✅ Auto-Add inteligentné mazanie:
                        if (currentRowCount > _minimumRowCount)
                        {
                            // Máme viac ako minimum → fyzicky zmaž riadok
                            _gridData.RemoveAt(rowIndex);
                            _logger.LogDebug("AUTO-ADD: Fyzicky zmazaný riadok {RowIndex} (zostalo: {RemainingRows}/{MinimumRows})",
                                rowIndex, _gridData.Count, _minimumRowCount);
                        }
                        else
                        {
                            // Sme na minimume → len vyčisti obsah riadku
                            var emptyRow = CreateEmptyRow();
                            _gridData[rowIndex] = emptyRow;
                            _logger.LogDebug("AUTO-ADD: Vyčistený obsah riadku {RowIndex} (zachované minimum {MinimumRows})",
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
                _logger.LogError(ex, "Chyba pri mazaní riadku {RowIndex}", rowIndex);
                throw;
            }
        }

        /// <summary>
        /// ✅ AKTUALIZOVANÉ: Pridanie riadku s Auto-Add logikou
        /// </summary>
        private async Task<int> AddRowInternalAsync(Dictionary<string, object?>? initialData)
        {
            try
            {
                EnsureInitialized();

                int newRowIndex = -1;

                await Task.Run(() =>
                {
                    lock (_dataLock)
                    {
                        // Kontrola maxRows limitu
                        if (_configuration!.MaxRows > 0 && _gridData.Count >= _configuration.MaxRows)
                        {
                            _logger.LogWarning("AUTO-ADD: Dosiahnutý maximálny počet riadkov: {MaxRows}", _configuration.MaxRows);
                            newRowIndex = -1;
                            return;
                        }

                        Dictionary<string, object?> newRow;

                        if (initialData != null)
                        {
                            newRow = ProcessAndValidateRowData(initialData);
                        }
                        else
                        {
                            newRow = CreateEmptyRow();
                        }

                        _gridData.Add(newRow);
                        newRowIndex = _gridData.Count - 1;

                        _logger.LogDebug("AUTO-ADD: Pridaný nový riadok na index {RowIndex} (celkom: {TotalRows})",
                            newRowIndex, _gridData.Count);

                        // ✅ Auto-Add logika: Ak pridávame dátový riadok, zabezpeč prázdny na konci
                        if (_autoAddEnabled && initialData != null)
                        {
                            CheckAndAddEmptyRowIfNeeded();
                        }
                    }
                });

                return newRowIndex;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Chyba pri pridávaní nového riadku");
                throw;
            }
        }

        /// <summary>
        /// ✅ AKTUALIZOVANÉ: Vymazanie všetkých dát s rešpektovaním minimálneho počtu
        /// </summary>
        private async Task ClearAllDataInternalAsync()
        {
            try
            {
                EnsureInitialized();

                await Task.Run(async () =>
                {
                    lock (_dataLock)
                    {
                        _logger.LogInformation("AUTO-ADD: Vymazávajú sa všetky dáta ({RowCount} riadkov)", _gridData.Count);

                        // Vyčisti všetky dáta
                        _gridData.Clear();

                        // ✅ Auto-Add logika: Vytvor minimálny počet prázdnych riadkov + 1 extra ak je auto-add
                        var requiredRows = _autoAddEnabled ? _minimumRowCount + 1 : _minimumRowCount;
                        for (int i = 0; i < requiredRows; i++)
                        {
                            _gridData.Add(CreateEmptyRow());
                        }

                        _logger.LogInformation("AUTO-ADD: Všetky dáta vymazané, obnovených {TotalRows} riadkov ({MinimumRows} minimum + {ExtraRows} extra)",
                            _gridData.Count, _minimumRowCount, _autoAddEnabled ? 1 : 0);
                    }

                    // Vyčisti pamäť
                    await _cleanupHelper.ForceGarbageCollectionAsync();
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Chyba pri vymazávaní všetkých dát");
                throw;
            }
        }

        /// <summary>
        /// ✅ AKTUALIZOVANÉ: Kompaktovanie s Auto-Add logikou
        /// </summary>
        private Task CompactRowsInternalAsync()
        {
            try
            {
                EnsureInitialized();

                return Task.Run(() =>
                {
                    lock (_dataLock)
                    {
                        _logger.LogDebug("AUTO-ADD: Spúšťa sa kompaktovanie riadkov");

                        var nonEmptyRows = new List<Dictionary<string, object?>>();

                        // Rozdeľ na neprázdne riadky
                        foreach (var row in _gridData)
                        {
                            if (!IsRowEmpty(row))
                            {
                                nonEmptyRows.Add(row);
                            }
                        }

                        // Vyčisti kolekciu a pridaj najprv neprázdne riadky
                        _gridData.Clear();
                        _gridData.AddRange(nonEmptyRows);

                        // ✅ Auto-Add logika: Pridaj potrebný počet prázdnych riadkov
                        var requiredEmptyRows = Math.Max(_minimumRowCount - nonEmptyRows.Count, 1); // Aspoň 1 prázdny

                        for (int i = 0; i < requiredEmptyRows; i++)
                        {
                            _gridData.Add(CreateEmptyRow());
                        }

                        _logger.LogDebug("AUTO-ADD kompaktovanie dokončené: {NonEmptyRows} neprázdnych, {EmptyRows} prázdnych riadkov (celkom: {TotalRows})",
                            nonEmptyRows.Count, requiredEmptyRows, _gridData.Count);
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Chyba pri kompaktovaní riadkov");
                throw;
            }
        }

        #endregion

        #region ✅ NOVÉ: Auto-Add Helper Methods

        /// <summary>
        /// Skontroluje či je stĺpec špeciálny (neráta sa do Auto-Add logiky)
        /// </summary>
        private bool IsSpecialColumn(string columnName)
        {
            return columnName == "DeleteRows" || columnName == "ValidAlerts";
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
                    return false;
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

                _logger.LogDebug("AUTO-ADD: Automaticky pridaný nový prázdny riadok na index {Index} (celkom: {TotalRows})",
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
                return _gridData.Count(row => !IsRowEmpty(row));
            }
        }

        /// <summary>
        /// Získa informácie o Auto-Add stave
        /// </summary>
        public string GetAutoAddStatus()
        {
            lock (_dataLock)
            {
                var nonEmptyCount = GetNonEmptyDataRowCount();
                var emptyCount = _gridData.Count - nonEmptyCount;

                return $"AUTO-ADD Status: {_gridData.Count} total rows ({nonEmptyCount} with data, {emptyCount} empty), minimum: {_minimumRowCount}, auto-add: {_autoAddEnabled}";
            }
        }

        #endregion

        #region Standard Implementation Methods (unchanged)

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

                _logger.LogDebug("AUTO-ADD: Získaných {RowCount} riadkov dát", result.Count);
                return Task.FromResult(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Chyba pri získavaní všetkých dát");
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
                        _logger.LogWarning("Neplatný index riadku: {RowIndex} (celkom {TotalRows} riadkov)", rowIndex, _gridData.Count);
                        return Task.FromResult(new Dictionary<string, object?>());
                    }

                    var result = new Dictionary<string, object?>(_gridData[rowIndex]);
                    _logger.LogDebug("Získané dáta riadku {RowIndex}", rowIndex);
                    return Task.FromResult(result);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Chyba pri získavaní dát riadku {RowIndex}", rowIndex);
                throw;
            }
        }

        private Task<object?> GetCellValueInternalAsync(int rowIndex, string columnName)
        {
            try
            {
                EnsureInitialized();

                if (string.IsNullOrWhiteSpace(columnName))
                    throw new ArgumentException("ColumnName nemôže byť prázdny", nameof(columnName));

                lock (_dataLock)
                {
                    if (rowIndex < 0 || rowIndex >= _gridData.Count)
                    {
                        _logger.LogWarning("Neplatný index riadku pri získavaní hodnoty: {RowIndex}", rowIndex);
                        return Task.FromResult<object?>(null);
                    }

                    var row = _gridData[rowIndex];
                    var value = row.ContainsKey(columnName) ? row[columnName] : null;

                    _logger.LogDebug("Získaná hodnota bunky [{RowIndex}, {ColumnName}] = {Value}",
                        rowIndex, columnName, value);

                    return Task.FromResult(value);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Chyba pri získavaní hodnoty bunky [{RowIndex}, {ColumnName}]", rowIndex, columnName);
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

                _logger.LogDebug("Počet neprázdnych riadkov: {Count}", count);
                return Task.FromResult(count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Chyba pri počítaní neprázdnych riadkov");
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
                        _logger.LogWarning("Neplatný index riadku pri kontrole prázdnosti: {RowIndex}", rowIndex);
                        return Task.FromResult(true);
                    }

                    var isEmpty = IsRowEmpty(_gridData[rowIndex]);
                    _logger.LogDebug("Riadok {RowIndex} je prázdny: {IsEmpty}", rowIndex, isEmpty);
                    return Task.FromResult(isEmpty);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Chyba pri kontrole prázdnosti riadku {RowIndex}", rowIndex);
                throw;
            }
        }

        #endregion

        #region Private Helper Methods

        private void EnsureInitialized()
        {
            if (!_isInitialized)
                throw new InvalidOperationException("DataManagementService nie je inicializovaný. Zavolajte InitializeAsync() najprv.");
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

            return row;
        }

        private Dictionary<string, object?> ProcessAndValidateRowData(Dictionary<string, object?> rowData)
        {
            var processedRow = CreateEmptyRow();

            foreach (var kvp in rowData)
            {
                var columnName = kvp.Key;
                var value = kvp.Value;

                // Konvertuj hodnotu na správny typ pre stĺpec
                var convertedValue = ConvertValueToColumnType(columnName, value);
                processedRow[columnName] = convertedValue;
            }

            return processedRow;
        }

        private object? ConvertValueToColumnType(string columnName, object? value)
        {
            if (value == null) return null;

            if (_columnTypes.ContainsKey(columnName))
            {
                var targetType = _columnTypes[columnName];
                return DataTypeConverter.ConvertValue(value, targetType);
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
        /// ✅ NOVÁ: Auto-Add informácie
        /// </summary>
        public bool IsAutoAddEnabled => _autoAddEnabled;
        public int MinimumRowCount => _minimumRowCount;

        #endregion
    }
}