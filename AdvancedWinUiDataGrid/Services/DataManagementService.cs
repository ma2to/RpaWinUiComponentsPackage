// Services/DataManagementService.cs - ✅ OPRAVENÝ s Auto-Add riadkov funkciou a CS0165 fix
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
    /// Implementácia služby pre správu dát v DataGrid - ✅ INTERNAL
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

        #region ✅ OPRAVENÉ: Explicit Interface Implementation

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

                    // ✅ NOVÁ FUNKCIONALITA: Vytvor minimálny počet prázdnych riadkov
                    for (int i = 0; i < _configuration.EmptyRowsCount; i++)
                    {
                        _gridData.Add(CreateEmptyRow());
                    }
                }

                _isInitialized = true;
                _logger.LogInformation("DataManagementService inicializovaný s {ColumnCount} stĺpcami a {RowCount} minimálnymi riadkami",
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
        /// ✅ NOVÁ FUNKCIONALITA: Načíta dáta s auto-add riadkov logikou
        /// </summary>
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

        #region ✅ NOVÁ FUNKCIONALITA: Auto-Add Riadkov Implementation

        /// <summary>
        /// ✅ NOVÉ: Načíta dáta s automatickým pridávaním riadkov
        /// Ak má viac dát ako riadkov, vytvorí dodatočné riadky + vždy jeden prázdny na konci
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
                        _logger.LogInformation("Načítavajú sa dáta: {RowCount} riadkov s auto-add logikou", data.Count);

                        // ✅ NOVÁ LOGIKA: Zabezpečenie dostatočnej kapacity
                        var minimumRows = _configuration!.EmptyRowsCount;
                        var requiredCapacity = Math.Max(data.Count + 1, minimumRows); // +1 pre prázdny riadok na konci

                        _logger.LogDebug("Potrebná kapacita: {RequiredCapacity} riadkov (dáta: {DataCount}, minimum: {MinimumRows})",
                            requiredCapacity, data.Count, minimumRows);

                        // Vyčisti existujúce dáta
                        _gridData.Clear();

                        // Načítaj dáta a vytvor riadky
                        for (int i = 0; i < requiredCapacity; i++)
                        {
                            Dictionary<string, object?> processedRow;

                            if (i < data.Count)
                            {
                                // Načítaj skutočné dáta
                                processedRow = ProcessAndValidateRowData(data[i]);
                                _logger.LogDebug("Načítaný dátový riadok {Index}", i);
                            }
                            else
                            {
                                // Vytvor prázdny riadok
                                processedRow = CreateEmptyRow();
                                _logger.LogDebug("Vytvorený prázdny riadok {Index}", i);
                            }

                            _gridData.Add(processedRow);
                        }

                        _logger.LogInformation("Auto-add dáta načítané: {TotalRows} riadkov ({DataRows} s dátami, {EmptyRows} prázdnych)",
                            _gridData.Count, data.Count, _gridData.Count - data.Count);
                    }
                });

                // Vyvolaj garbage collection pre uvoľnenie pamäte
                await _cleanupHelper.ForceGarbageCollectionAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Chyba pri načítavaní dát s auto-add");
                throw;
            }
        }

        /// <summary>
        /// ✅ NOVÉ: Automaticky pridá nový prázdny riadok ak sa vyplní posledný
        /// </summary>
        public async Task EnsureEmptyRowAtEndAsync()
        {
            try
            {
                EnsureInitialized();

                await Task.Run(() =>
                {
                    lock (_dataLock)
                    {
                        // Skontroluj či posledný riadok nie je prázdny
                        if (_gridData.Count > 0)
                        {
                            var lastRow = _gridData[^1]; // C# 8 syntax
                            if (!IsRowEmpty(lastRow))
                            {
                                // Posledný riadok nie je prázdny - pridaj nový prázdny
                                var newEmptyRow = CreateEmptyRow();
                                _gridData.Add(newEmptyRow);
                                _logger.LogInformation("Automaticky pridaný nový prázdny riadok na index {Index}", _gridData.Count - 1);
                            }
                        }
                        else
                        {
                            // Žiadne riadky - pridaj aspoň jeden prázdny
                            var emptyRow = CreateEmptyRow();
                            _gridData.Add(emptyRow);
                            _logger.LogInformation("Pridaný prvý prázdny riadok");
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Chyba pri zabezpečovaní prázdneho riadku na konci");
                throw;
            }
        }

        /// <summary>
        /// ✅ OPRAVENÉ CS0165: Inicializuje newRowIndex a inteligentné pridanie riadku
        /// </summary>
        private async Task<int> AddRowInternalAsync(Dictionary<string, object?>? initialData)
        {
            try
            {
                EnsureInitialized();

                int newRowIndex = -1; // ✅ FIX CS0165: Inicializácia premennej

                await Task.Run(() =>
                {
                    lock (_dataLock)
                    {
                        // Kontrola maxRows limitu
                        if (_configuration!.MaxRows > 0 && _gridData.Count >= _configuration.MaxRows)
                        {
                            _logger.LogWarning("Dosiahnutý maximálny počet riadkov: {MaxRows}", _configuration.MaxRows);
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

                        _logger.LogDebug("Pridaný nový riadok na index {RowIndex} (celkom: {TotalRows})", newRowIndex, _gridData.Count);

                        // ✅ NOVÁ LOGIKA: Zabezpeč prázdny riadok na konci ak pridávame dáta
                        if (initialData != null) // Ak pridávame riadok s dátami
                        {
                            var emptyRow = CreateEmptyRow();
                            _gridData.Add(emptyRow);
                            _logger.LogDebug("Automaticky pridaný prázdny riadok na koniec");
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
        /// ✅ NOVÁ FUNKCIONALITA: Inteligentné mazanie s ochranou minimálneho počtu
        /// - Ak mám viac riadkov ako minimum: fyzicky zmaž riadok
        /// - Ak som na minimume: len vyčisti obsah riadku (ponechaj prázdny)
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
                            _logger.LogWarning("Neplatný index riadku pri mazaní: {RowIndex}", rowIndex);
                            return;
                        }

                        var minimumRows = _configuration!.EmptyRowsCount;

                        if (_gridData.Count > minimumRows)
                        {
                            // Máme viac ako minimum - fyzicky zmaž riadok
                            _gridData.RemoveAt(rowIndex);
                            _logger.LogDebug("Fyzicky zmazaný riadok {RowIndex} (zostalo: {RemainingRows})", rowIndex, _gridData.Count);
                        }
                        else
                        {
                            // Sme na minimume - len vyčisti obsah riadku
                            var emptyRow = CreateEmptyRow();
                            _gridData[rowIndex] = emptyRow;
                            _logger.LogDebug("Vyčistený obsah riadku {RowIndex} (zachované minimum)", rowIndex);
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

        #endregion

        #region Internal Implementation Methods

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

                    // ✅ NOVÁ FUNKCIA: Kontrola či je teraz posledný riadok vyplnený
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            await EnsureEmptyRowAtEndAsync();
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Chyba pri auto-add kontrole");
                        }
                    });
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

        /// <summary>
        /// ✅ UPRAVENÉ: ClearAllDataAsync s rešpektovaním minimálneho počtu riadkov
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
                        _logger.LogInformation("Vymazávajú sa všetky dáta ({RowCount} riadkov)", _gridData.Count);

                        // Vyčisti všetky dáta
                        _gridData.Clear();

                        // ✅ NOVÁ LOGIKA: Vytvor minimálny počet prázdnych riadkov z konfigurácie
                        var minimumRows = _configuration!.EmptyRowsCount;
                        for (int i = 0; i < minimumRows; i++)
                        {
                            _gridData.Add(CreateEmptyRow());
                        }

                        _logger.LogInformation("Všetky dáta vymazané, obnovených {MinimumRows} minimálnych riadkov",
                            minimumRows);
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
        /// ✅ UPRAVENÉ: CompactRowsInternalAsync s auto-add logikou
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
                        _logger.LogDebug("Spúšťa sa kompaktovanie riadkov s auto-add logikou");

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

                        // ✅ NOVÁ LOGIKA: Pridaj potrebný počet prázdnych riadkov
                        var minimumRows = _configuration!.EmptyRowsCount;
                        var requiredEmptyRows = Math.Max(minimumRows - nonEmptyRows.Count, 1); // Aspoň 1 prázdny

                        for (int i = 0; i < requiredEmptyRows; i++)
                        {
                            _gridData.Add(CreateEmptyRow());
                        }

                        _logger.LogDebug("Kompaktovanie dokončené: {NonEmptyRows} neprázdnych, {EmptyRows} prázdnych riadkov (celkom: {TotalRows})",
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