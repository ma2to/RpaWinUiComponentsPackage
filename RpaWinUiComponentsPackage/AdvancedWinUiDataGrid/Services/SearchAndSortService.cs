// Services/SearchAndSortService.cs - ✅ OPRAVENÉ pre PUBLIC SortDirection enum
using Microsoft.Extensions.Logging;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Services
{
    /// <summary>
    /// Služba pre Search, Sort a Zebra Rows funkcionalitu - ✅ INTERNAL
    /// </summary>
    internal class SearchAndSortService : IDisposable
    {
        #region Private Fields

        private readonly Dictionary<string, string> _columnSearchFilters = new();
        private readonly Dictionary<string, SortDirection> _columnSortStates = new();
        private string? _currentSortColumn;
        private bool _isDisposed = false;

        // ✅ NOVÉ: Zebra rows (alternating row colors)
        private bool _zebraRowsEnabled = true;

        #endregion

        #region Constructor

        // ✅ OPRAVENÉ: Konštruktor bez ILogger parametra (to spôsobovalo CS1503 chybu)
        public SearchAndSortService()
        {
            // Inicializácia bez loggera - používame Debug.WriteLine pre diagnostiku
            System.Diagnostics.Debug.WriteLine("SearchAndSortService initialized without logger dependency");
        }

        #endregion

        #region ✅ Search Functionality

        /// <summary>
        /// Nastaví search filter pre stĺpec
        /// </summary>
        public void SetColumnSearchFilter(string columnName, string searchText)
        {
            if (string.IsNullOrWhiteSpace(columnName)) return;

            if (string.IsNullOrWhiteSpace(searchText))
            {
                _columnSearchFilters.Remove(columnName);
                System.Diagnostics.Debug.WriteLine($"Search filter pre {columnName} odstránený");
            }
            else
            {
                _columnSearchFilters[columnName] = searchText.Trim();
                System.Diagnostics.Debug.WriteLine($"Search filter pre {columnName} nastavený na '{searchText}'");
            }
        }

        /// <summary>
        /// Získa aktuálny search filter pre stĺpec
        /// </summary>
        public string GetColumnSearchFilter(string columnName)
        {
            return _columnSearchFilters.TryGetValue(columnName, out var filter) ? filter : string.Empty;
        }

        /// <summary>
        /// Vyčistí všetky search filtre
        /// </summary>
        public void ClearAllSearchFilters()
        {
            _columnSearchFilters.Clear();
            System.Diagnostics.Debug.WriteLine("Všetky search filtre vyčistené");
        }

        /// <summary>
        /// Aplikuje search filtre na dáta
        /// </summary>
        public async Task<List<Dictionary<string, object?>>> ApplySearchFiltersAsync(List<Dictionary<string, object?>> data)
        {
            if (!_columnSearchFilters.Any())
                return data;

            return await Task.Run(() =>
            {
                var filteredData = new List<Dictionary<string, object?>>();

                foreach (var row in data)
                {
                    // Skontroluj či je riadok prázdny - prázdne riadky sa vždy pridajú na koniec
                    var isEmpty = IsRowEmpty(row);

                    if (isEmpty)
                    {
                        // Prázdne riadky pridaj neskôr
                        continue;
                    }

                    var matchesAllFilters = true;

                    foreach (var filter in _columnSearchFilters)
                    {
                        var columnName = filter.Key;
                        var searchText = filter.Value;

                        if (row.TryGetValue(columnName, out var cellValue))
                        {
                            var cellText = cellValue?.ToString() ?? string.Empty;

                            // Case-insensitive obsahuje search
                            if (!cellText.Contains(searchText, StringComparison.OrdinalIgnoreCase))
                            {
                                matchesAllFilters = false;
                                break;
                            }
                        }
                        else
                        {
                            // Ak stĺpec neexistuje, riadok nevyhovuje filtru
                            matchesAllFilters = false;
                            break;
                        }
                    }

                    if (matchesAllFilters)
                    {
                        filteredData.Add(row);
                    }
                }

                // ✅ KĽÚČOVÉ: Pridaj všetky prázdne riadky na koniec
                var emptyRows = data.Where(IsRowEmpty).ToList();
                filteredData.AddRange(emptyRows);

                System.Diagnostics.Debug.WriteLine($"Search filtre aplikované: {data.Count} → {filteredData.Count} riadkov");

                return filteredData;
            });
        }

        #endregion

        #region ✅ Sort Functionality s Header Click

        /// <summary>
        /// ✅ OPRAVENÉ: Togglene sort pre stĺpec pri kliknutí na header (None → Ascending → Descending → None)
        /// </summary>
        public SortDirection ToggleColumnSort(string columnName)
        {
            if (string.IsNullOrWhiteSpace(columnName))
                return SortDirection.None;

            // Ak je iný stĺpec sortovaný, vyčisti ho
            if (_currentSortColumn != null && _currentSortColumn != columnName)
            {
                _columnSortStates.Remove(_currentSortColumn);
            }

            // Toggle current column: None → Asc → Desc → None
            var currentDirection = _columnSortStates.TryGetValue(columnName, out var direction) ? direction : SortDirection.None;
            var newDirection = currentDirection switch
            {
                SortDirection.None => SortDirection.Ascending,
                SortDirection.Ascending => SortDirection.Descending,
                SortDirection.Descending => SortDirection.None,
                _ => SortDirection.None
            };

            if (newDirection == SortDirection.None)
            {
                _columnSortStates.Remove(columnName);
                _currentSortColumn = null;
            }
            else
            {
                _columnSortStates[columnName] = newDirection;
                _currentSortColumn = columnName;
            }

            System.Diagnostics.Debug.WriteLine($"Header click sort toggle pre {columnName}: {currentDirection} → {newDirection}");

            return newDirection;
        }

        /// <summary>
        /// Získa aktuálny sort direction pre stĺpec
        /// </summary>
        public SortDirection GetColumnSortDirection(string columnName)
        {
            return _columnSortStates.TryGetValue(columnName, out var direction) ? direction : SortDirection.None;
        }

        /// <summary>
        /// Vyčistí všetky sort stavy
        /// </summary>
        public void ClearAllSorts()
        {
            _columnSortStates.Clear();
            _currentSortColumn = null;
            System.Diagnostics.Debug.WriteLine("Všetky sort stavy vyčistené");
        }

        /// <summary>
        /// Aplikuje sorting na dáta (prázdne riadky vždy na konci)
        /// </summary>
        public async Task<List<Dictionary<string, object?>>> ApplySortingAsync(List<Dictionary<string, object?>> data)
        {
            if (_currentSortColumn == null || !_columnSortStates.ContainsKey(_currentSortColumn))
                return data;

            return await Task.Run(() =>
            {
                var sortColumn = _currentSortColumn;
                var sortDirection = _columnSortStates[sortColumn];

                // Rozdel dáta na neprázdne a prázdne riadky
                var nonEmptyRows = data.Where(row => !IsRowEmpty(row)).ToList();
                var emptyRows = data.Where(IsRowEmpty).ToList();

                // Sort iba neprázdne riadky
                var sortedNonEmptyRows = sortDirection == SortDirection.Ascending
                    ? nonEmptyRows.OrderBy(row => GetSortValue(row, sortColumn)).ToList()
                    : nonEmptyRows.OrderByDescending(row => GetSortValue(row, sortColumn)).ToList();

                // ✅ KĽÚČOVÉ: Prázdne riadky vždy na koniec
                var result = new List<Dictionary<string, object?>>();
                result.AddRange(sortedNonEmptyRows);
                result.AddRange(emptyRows);

                System.Diagnostics.Debug.WriteLine($"Sort aplikovaný na {sortColumn} ({sortDirection}): {sortedNonEmptyRows.Count} neprázdnych + {emptyRows.Count} prázdnych");

                return result;
            });
        }

        #endregion

        #region ✅ NOVÉ: Zebra Rows (Alternating Row Colors)

        /// <summary>
        /// Povolí/zakáže zebra rows effect
        /// </summary>
        public void SetZebraRowsEnabled(bool enabled)
        {
            _zebraRowsEnabled = enabled;
            System.Diagnostics.Debug.WriteLine($"Zebra rows {(enabled ? "enabled" : "disabled")}");
        }

        /// <summary>
        /// Kontroluje či sú zebra rows povolené
        /// </summary>
        public bool IsZebraRowsEnabled => _zebraRowsEnabled;

        /// <summary>
        /// Určuje či je riadok "párny" pre zebra effect (iba neprázdne riadky sa počítajú)
        /// </summary>
        public bool IsEvenRowForZebra(int actualRowIndex, List<Dictionary<string, object?>> allData)
        {
            if (!_zebraRowsEnabled) return false;

            // Spočítaj iba neprázdne riadky pred týmto riadkom
            var nonEmptyRowsBefore = 0;
            for (int i = 0; i < actualRowIndex && i < allData.Count; i++)
            {
                if (!IsRowEmpty(allData[i]))
                {
                    nonEmptyRowsBefore++;
                }
            }

            // Párne indexy (0, 2, 4...) budú mať zebra effect
            return nonEmptyRowsBefore % 2 == 0;
        }

        /// <summary>
        /// ✅ NOVÉ: Aplikuje zebra row background na dáta
        /// </summary>
        public async Task<List<RowDisplayInfo>> ApplyZebraRowStylingAsync(List<Dictionary<string, object?>> data)
        {
            return await Task.Run(() =>
            {
                var result = new List<RowDisplayInfo>();
                var nonEmptyRowIndex = 0;

                for (int i = 0; i < data.Count; i++)
                {
                    var row = data[i];
                    var isEmpty = IsRowEmpty(row);
                    var isZebraRow = false;

                    if (!isEmpty && _zebraRowsEnabled)
                    {
                        // Iba neprázdne riadky majú zebra effect
                        isZebraRow = nonEmptyRowIndex % 2 == 1; // Každý druhý neprázdny riadok
                        nonEmptyRowIndex++;
                    }

                    result.Add(new RowDisplayInfo
                    {
                        RowIndex = i,
                        Data = row,
                        IsEmpty = isEmpty,
                        IsZebraRow = isZebraRow,
                        IsEvenNonEmptyRow = !isEmpty && (nonEmptyRowIndex - 1) % 2 == 0
                    });
                }

                System.Diagnostics.Debug.WriteLine($"Zebra styling aplikované: {result.Count} riadkov, {nonEmptyRowIndex} neprázdnych");

                return result;
            });
        }

        #endregion

        #region ✅ Combined Search + Sort + Zebra

        /// <summary>
        /// Aplikuje search, potom sort a nakoniec zebra styling na dáta
        /// </summary>
        public async Task<List<RowDisplayInfo>> ApplyAllFiltersAndStylingAsync(List<Dictionary<string, object?>> data)
        {
            // Najprv aplikuj search
            var searchedData = await ApplySearchFiltersAsync(data);

            // Potom aplikuj sort
            var sortedData = await ApplySortingAsync(searchedData);

            // Nakoniec aplikuj zebra styling
            var styledData = await ApplyZebraRowStylingAsync(sortedData);

            return styledData;
        }

        #endregion

        #region Private Helper Methods

        private bool IsRowEmpty(Dictionary<string, object?> row)
        {
            foreach (var kvp in row)
            {
                var columnName = kvp.Key;
                var value = kvp.Value;

                // Ignoruj špeciálne stĺpce
                if (columnName == "DeleteRows" || columnName == "ValidAlerts")
                    continue;

                // Ak má nejakú hodnotu, riadok nie je prázdny
                if (value != null && !string.IsNullOrWhiteSpace(value.ToString()))
                    return false;
            }

            return true;
        }

        private object GetSortValue(Dictionary<string, object?> row, string columnName)
        {
            if (!row.TryGetValue(columnName, out var value) || value == null)
                return string.Empty;

            // Pre string porovnanie case-insensitive
            if (value is string str)
                return str.ToLowerInvariant();

            return value;
        }

        #endregion

        #region ✅ Status Properties

        /// <summary>
        /// Má aktívne search filtre
        /// </summary>
        public bool HasActiveSearchFilters => _columnSearchFilters.Any();

        /// <summary>
        /// Má aktívny sort
        /// </summary>
        public bool HasActiveSort => _currentSortColumn != null;

        /// <summary>
        /// Získa aktuálne sortovaný stĺpec a direction
        /// </summary>
        public (string? Column, SortDirection Direction) GetCurrentSort()
        {
            if (_currentSortColumn != null && _columnSortStates.TryGetValue(_currentSortColumn, out var direction))
            {
                return (_currentSortColumn, direction);
            }
            return (null, SortDirection.None);
        }

        /// <summary>
        /// Získa status info pre debugging
        /// </summary>
        public string GetStatusInfo()
        {
            var searchCount = _columnSearchFilters.Count;
            var sortInfo = _currentSortColumn != null
                ? $"{_currentSortColumn} ({_columnSortStates[_currentSortColumn]})"
                : "None";

            return $"Search: {searchCount} filters, Sort: {sortInfo}, Zebra: {_zebraRowsEnabled}";
        }

        #endregion

        #region IDisposable

        public void Dispose()
        {
            if (_isDisposed) return;

            _columnSearchFilters.Clear();
            _columnSortStates.Clear();
            _currentSortColumn = null;
            _isDisposed = true;

            System.Diagnostics.Debug.WriteLine("SearchAndSortService disposed");
        }

        #endregion
    }

    /// <summary>
    /// ✅ NOVÁ: Informácie o zobrazení riadku s zebra styling
    /// </summary>
    internal class RowDisplayInfo
    {
        public int RowIndex { get; set; }
        public Dictionary<string, object?> Data { get; set; } = new();
        public bool IsEmpty { get; set; }
        public bool IsZebraRow { get; set; }
        public bool IsEvenNonEmptyRow { get; set; }

        /// <summary>
        /// CSS class alebo style name pre tento riadok
        /// </summary>
        public string GetRowStyleClass()
        {
            if (IsEmpty) return "empty-row";
            if (IsZebraRow) return "zebra-row";
            return "normal-row";
        }

        public override string ToString()
        {
            return $"Row {RowIndex}: {(IsEmpty ? "Empty" : "Data")}, Zebra: {IsZebraRow}";
        }
    }
}