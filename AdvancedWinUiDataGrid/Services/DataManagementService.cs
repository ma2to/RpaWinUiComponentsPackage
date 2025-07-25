// Services/DataManagementService.cs - ✅ OPRAVENÝ CS0051 - explicit interface implementation
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
    /// Implementácia služby pre správu dát v DataGrid - ✅ OPRAVENÝ accessibility
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

        public DataManagementService(ILogger<DataManagementService> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _cleanupHelper = new ResourceCleanupHelper();
        }

        #region ✅ OPRAVENÉ CS0051: Explicit Interface Implementation

        /// <summary>
        /// ✅ OPRAVENÉ: Explicit interface implementation pre GridConfiguration accessibility
        /// </summary>
        Task IDataManagementService.InitializeAsync(GridConfiguration configuration)
        {
            try
            {
                _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));

                lock (_dataLock)
                {
                    // Vyčisti existujúce dáta
                    _gridData.Clear();
                    _columnTypes.Clear();

                    // Načítaj typy stĺpcov z konfigurácie
                    foreach (var column in _configuration.Columns)
                    {
                        _columnTypes[column.Name] = column.DataType;
                    }

                    // Vytvor prázdne riadky podľa konfigurácie
                    for (int i = 0; i < _configuration.EmptyRowsCount; i++)
                    {
                        _gridData.Add(CreateEmptyRow());
                    }
                }

                _isInitialized = true;
                _logger.LogInformation("DataManagementService inicializovaný s {ColumnCount} stĺpcami a {RowCount} riadkami",
                    _configuration.Columns.Count, _configuration.EmptyRowsCount);

                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Chyba pri inicializácii DataManagementService");
                throw;
            }
        }

        /// <summary>
        /// Načíta dáta do gridu
        /// </summary>
        Task IDataManagementService.LoadDataAsync(List<Dictionary<string, object?>> data)
        {
            return LoadDataInternalAsync(data);
        }

        /// <summary>
        /// Získa všetky dáta z gridu
        /// </summary>
        Task<List<Dictionary<string, object?>>> IDataManagementService.GetAllDataAsync()
        {
            return GetAllDataInternalAsync();
        }

        /// <summary>
        /// Získa dáta špecifického riadku
        /// </summary>
        Task<Dictionary<string, object?>> IDataManagementService.GetRowDataAsync(int rowIndex)
        {
            return GetRowDataInternalAsync(rowIndex);
        }

        /// <summary>
        /// Nastaví hodnotu bunky
        /// </summary>
        Task IDataManagementService.SetCellValueAsync(int rowIndex, string columnName, object? value)
        {
            return SetCellValueInternalAsync(rowIndex, columnName, value);
        }

        /// <summary>
        /// Získa hodnotu bunky
        /// </summary>
        Task<object?> IDataManagementService.GetCellValueAsync(int rowIndex, string columnName)
        {
            return GetCellValueInternalAsync(rowIndex, columnName);
        }

        /// <summary>
        /// Pridá nový riadok
        /// </summary>
        Task<int> IDataManagementService.AddRowAsync(Dictionary<string, object?>? initialData)
        {
            return AddRowInternalAsync(initialData);
        }

        /// <summary>
        /// Zmaže riadok (vyčisti jeho obsah)
        /// </summary>
        Task IDataManagementService.DeleteRowAsync(int rowIndex)
        {
            return DeleteRowInternalAsync(rowIndex);
        }

        /// <summary>
        /// Vymaže všetky dáta
        /// </summary>
        Task IDataManagementService.ClearAllDataAsync()
        {
            return ClearAllDataInternalAsync();
        }

        /// <summary>
        /// Kompaktuje riadky (odstráni prázdne medzery)
        /// </summary>
        Task IDataManagementService.CompactRowsAsync()
        {
            return CompactRowsInternalAsync();
        }

        /// <summary>
        /// Získa počet neprázdnych riadkov
        /// </summary>
        Task<int> IDataManagementService.GetNonEmptyRowCountAsync()
        {
            return GetNonEmptyRowCountInternalAsync();
        }

        /// <summary>
        /// Kontroluje či je riadok prázdny
        /// </summary>
        Task<bool> IDataManagementService.IsRowEmptyAsync(int rowIndex)
        {
            return IsRowEmptyInternalAsync(rowIndex);
        }

        #endregion

        #region Internal Implementation Methods

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
                        _logger.LogInformation("Načítavajú sa dáta: {RowCount} riadkov", data.Count);

                        // Vyčisti existujúce dáta
                        _gridData.Clear();

                        // Načítaj nové dáta
                        foreach (var rowData in data)
                        {
                            var processedRow = ProcessAndValidateRowData(rowData);
                            _gridData.Add(processedRow);
                        }

                        // Pridaj dodatočné prázdne riadky ak je potrebné
                        var currentRowCount = _gridData.Count;
                        var requiredRowCount = Math.Max(currentRowCount, _configuration!.EmptyRowsCount);

                        for (int i = currentRowCount; i < requiredRowCount; i++)
                        {
                            _gridData.Add(CreateEmptyRow());
                        }

                        _logger.LogInformation("Dáta načítané: {TotalRows} riadkov ({DataRows} s dátami, {EmptyRows} prázdnych)",
                            _gridData.Count, data.Count, _gridData.Count - data.Count);
                    }
                });

                // Vyvolaj garbage collection pre uvoľnenie pamäte
                await _cleanupHelper.ForceGarbageCollectionAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Chyba pri načítavaní dát");
                throw;
            }
        }

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

                _logger.LogDebug("Získaných {RowCount} riadkov dát", result.Count);
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

        private Task SetCellValueInternalAsync(int rowIndex, string columnName, object? value)
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
                        _logger.LogWarning("Neplatný index riadku pri nastavovaní hodnoty: {RowIndex}", rowIndex);
                        return Task.CompletedTask;
                    }

                    var row = _gridData[rowIndex];
                    var convertedValue = ConvertValueToColumnType(columnName, value);
                    row[columnName] = convertedValue;

                    _logger.LogDebug("Nastavená hodnota bunky [{RowIndex}, {ColumnName}] = {Value}",
                        rowIndex, columnName, convertedValue);
                }

                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Chyba pri nastavovaní hodnoty bunky [{RowIndex}, {ColumnName}]", rowIndex, columnName);
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

        private Task<int> AddRowInternalAsync(Dictionary<string, object?>? initialData)
        {
            try
            {
                EnsureInitialized();

                int newRowIndex;

                lock (_dataLock)
                {
                    // Kontrola maxRows limitu
                    if (_configuration!.MaxRows > 0 && _gridData.Count >= _configuration.MaxRows)
                    {
                        _logger.LogWarning("Dosiahnutý maximálny počet riadkov: {MaxRows}", _configuration.MaxRows);
                        return Task.FromResult(-1);
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

                    _logger.LogDebug("Pridaný nový riadok na index {RowIndex}", newRowIndex);
                }

                return Task.FromResult(newRowIndex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Chyba pri pridávaní nového riadku");
                throw;
            }
        }

        private async Task DeleteRowInternalAsync(int rowIndex)
        {
            try
            {
                EnsureInitialized();

                lock (_dataLock)
                {
                    if (rowIndex < 0 || rowIndex >= _gridData.Count)
                    {
                        _logger.LogWarning("Neplatný index riadku pri mazaní: {RowIndex}", rowIndex);
                        return;
                    }

                    // Vyčisti obsah riadku (ale nenechávaj ho prázdny - nahraď prázdnym riadkom)
                    var emptyRow = CreateEmptyRow();
                    _gridData[rowIndex] = emptyRow;

                    _logger.LogDebug("Vymazaný riadok {RowIndex}", rowIndex);
                }

                // Kompaktuj riadky
                await CompactRowsInternalAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Chyba pri mazaní riadku {RowIndex}", rowIndex);
                throw;
            }
        }

        private async Task ClearAllDataInternalAsync()
        {
            try
            {
                EnsureInitialized();

                await Task.Run(async () =>
                {
                    lock (_dataLock)
                    {
                        _logger.LogInformation("Vymazávajú sa všetky dáta ({RowCount} riadkov)", _gridData.Count);

                        // Vyčisti všetky dáta
                        _gridData.Clear();

                        // Vytvor nové prázdne riadky
                        for (int i = 0; i < _configuration!.EmptyRowsCount; i++)
                        {
                            _gridData.Add(CreateEmptyRow());
                        }

                        _logger.LogInformation("Všetky dáta vymazané, vytvorených {EmptyRows} prázdnych riadkov",
                            _configuration.EmptyRowsCount);
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

        private Task CompactRowsInternalAsync()
        {
            try
            {
                EnsureInitialized();

                return Task.Run(() =>
                {
                    lock (_dataLock)
                    {
                        _logger.LogDebug("Spúšťa sa kompaktovanie riadkov");

                        var nonEmptyRows = new List<Dictionary<string, object?>>();
                        var emptyRowCount = 0;

                        // Rozdeľ na neprázdne a prázdne riadky
                        foreach (var row in _gridData)
                        {
                            if (IsRowEmpty(row))
                            {
                                emptyRowCount++;
                            }
                            else
                            {
                                nonEmptyRows.Add(row);
                            }
                        }

                        // Vyčisti kolekciu a pridaj najprv neprázdne riadky
                        _gridData.Clear();
                        _gridData.AddRange(nonEmptyRows);

                        // Pridaj potrebný počet prázdnych riadkov
                        var requiredEmptyRows = Math.Max(_configuration!.EmptyRowsCount, emptyRowCount);
                        for (int i = 0; i < requiredEmptyRows; i++)
                        {
                            _gridData.Add(CreateEmptyRow());
                        }

                        _logger.LogDebug("Kompaktovanie dokončené: {NonEmptyRows} neprázdnych, {EmptyRows} prázdnych riadkov",
                            nonEmptyRows.Count, requiredEmptyRows);
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Chyba pri kompaktovaní riadkov");
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
                    count = _gridData.Count(row => !IsRowEmpty(row));
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

        private bool IsRowEmpty(Dictionary<string, object?> row)
        {
            foreach (var kvp in row)
            {
                var columnName = kvp.Key;
                var value = kvp.Value;

                // Ignoruj špeciálne stĺpce
                if (columnName == "DeleteRows" || columnName == "ValidAlerts")
                    continue;

                // Ak je nejaká hodnota vyplnená, riadok nie je prázdny
                if (value != null && !string.IsNullOrWhiteSpace(value.ToString()))
                    return false;
            }

            return true;
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

        #endregion
    }
}