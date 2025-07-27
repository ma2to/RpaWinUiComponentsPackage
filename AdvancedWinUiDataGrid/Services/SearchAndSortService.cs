// Services/SearchAndSortService.cs - ✅ INTERNAL Search & Sort functionality
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RpaWinUiComponents.AdvancedWinUiDataGrid.Services
{
    /// <summary>
    /// Služba pre Search a Sort funkcionalitu - ✅ INTERNAL
    /// </summary>
    internal class SearchAndSortService : IDisposable
    {
        private readonly ILogger<SearchAndSortService>? _logger;
        private readonly Dictionary<string, string> _columnSearchFilters = new();
        private readonly Dictionary<string, SortDirection> _columnSortStates = new();
        private string? _currentSortColumn;
        private bool _isDisposed = false;

        public SearchAndSortService(ILogger<SearchAndSortService>? logger = null)
        {
            _logger = logger;
        }

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
                _logger?.LogDebug("Search filter pre {ColumnName} odstránený", columnName);
            }
            else
            {
                _columnSearchFilters[columnName] = searchText.Trim();
                _logger?.LogDebug("Search filter pre {ColumnName} nastavený na '{SearchText}'", columnName, searchText);
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
            _logger?.LogDebug("Všetky search filtre vyčistené");
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

                _logger?.LogDebug("Search filtre aplikované: {OriginalCount} → {FilteredCount} riadkov",
                    data.Count, filteredData.Count);

                return filteredData;
            });
        }

        #endregion

        #region ✅ Sort Functionality

        /// <summary>
        /// Togglene sort pre stĺpec (None → Ascending → Descending → None)
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

            _logger?.LogDebug("Sort toggle pre {ColumnName}: {OldDirection} → {NewDirection}",
                columnName, currentDirection, newDirection);

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
            _logger?.LogDebug("Všetky sort stavy vyčistené");
        }

        /// <summary>
        /// Aplikuje sorting na dáta
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

                _logger?.LogDebug("Sort aplikovaný na {ColumnName} ({Direction}): {NonEmptyCount} neprázdnych + {EmptyCount} prázdnych",
                    sortColumn, sortDirection, sortedNonEmptyRows.Count, emptyRows.Count);

                return result;
            });
        }

        #endregion

        #region ✅ Combined Search + Sort

        /// <summary>
        /// Aplikuje search a potom sort na dáta
        /// </summary>
        public async Task<List<Dictionary<string, object?>>> ApplySearchAndSortAsync(List<Dictionary<string, object?>> data)
        {
            // Najprv aplikuj search
            var searchedData = await ApplySearchFiltersAsync(data);

            // Potom aplikuj sort
            var sortedData = await ApplySortingAsync(searchedData);

            return sortedData;
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
        /// Získa status info pre debugging
        /// </summary>
        public string GetStatusInfo()
        {
            var searchCount = _columnSearchFilters.Count;
            var sortInfo = _currentSortColumn != null
                ? $"{_currentSortColumn} ({_columnSortStates[_currentSortColumn]})"
                : "None";

            return $"Search: {searchCount} filters, Sort: {sortInfo}";
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

            _logger?.LogDebug("SearchAndSortService disposed");
        }

        #endregion
    }

    /// <summary>
    /// Sort direction enum - ✅ INTERNAL
    /// </summary>
    internal enum SortDirection
    {
        None,
        Ascending,
        Descending
    }
}