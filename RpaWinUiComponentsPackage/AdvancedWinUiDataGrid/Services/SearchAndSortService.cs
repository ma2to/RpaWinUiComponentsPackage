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
    /// Služba pre Search, Sort a Zebra Rows funkcionalitu - ✅ INTERNAL s kompletným logovaním
    /// </summary>
    internal class SearchAndSortService : IDisposable
    {
        #region Private Fields

        private readonly ILogger<SearchAndSortService> _logger;
        private readonly Dictionary<string, string> _columnSearchFilters = new();
        private readonly Dictionary<string, SortDirection> _columnSortStates = new();
        private string? _currentSortColumn;
        private bool _isDisposed = false;

        // ✅ NOVÉ: Zebra rows (alternating row colors)
        private bool _zebraRowsEnabled = true;

        // ✅ ROZŠÍRENÉ: Performance a state tracking
        private readonly Dictionary<string, DateTime> _operationStartTimes = new();
        private readonly Dictionary<string, int> _operationCounters = new();
        private int _totalSearchOperations = 0;
        private int _totalSortOperations = 0;
        private int _totalZebraOperations = 0;

        #endregion

        #region Constructor

        /// <summary>
        /// Vytvorí SearchAndSortService s loggerom pre kompletné sledovanie operácií
        /// </summary>
        public SearchAndSortService(ILogger<SearchAndSortService> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _logger.LogInformation("🔧 SearchAndSortService initialized - LoggerType: {LoggerType}, ZebraEnabled: {ZebraEnabled}",
                _logger.GetType().Name, _zebraRowsEnabled);
        }

        #endregion

        #region ✅ Search Functionality

        /// <summary>
        /// Nastaví search filter pre stĺpec s kompletným logovaním
        /// </summary>
        public void SetColumnSearchFilter(string columnName, string searchText)
        {
            var operationId = StartOperation("SetColumnSearchFilter");
            
            try
            {
                if (string.IsNullOrWhiteSpace(columnName))
                {
                    _logger.LogWarning("🔍 SetColumnSearchFilter - Invalid columnName provided (null/empty)");
                    return;
                }

                var previousFilter = _columnSearchFilters.TryGetValue(columnName, out var existing) ? existing : null;
                var activeFiltersBefore = _columnSearchFilters.Count;

                if (string.IsNullOrWhiteSpace(searchText))
                {
                    _columnSearchFilters.Remove(columnName);
                    _logger.LogInformation("🔍 Search filter REMOVED - Column: {ColumnName}, PreviousFilter: '{PreviousFilter}', " +
                        "ActiveFilters: {FilterCountBefore} → {FilterCountAfter}",
                        columnName, previousFilter, activeFiltersBefore, _columnSearchFilters.Count);
                }
                else
                {
                    var trimmedText = searchText.Trim();
                    _columnSearchFilters[columnName] = trimmedText;
                    
                    _logger.LogInformation("🔍 Search filter SET - Column: {ColumnName}, Filter: '{SearchText}', " +
                        "PreviousFilter: '{PreviousFilter}', ActiveFilters: {FilterCountBefore} → {FilterCountAfter}, " +
                        "FilterLength: {FilterLength}",
                        columnName, trimmedText, previousFilter, activeFiltersBefore, _columnSearchFilters.Count, trimmedText.Length);
                }

                // Track filter complexity
                var totalFilterLength = _columnSearchFilters.Values.Sum(f => f.Length);
                _logger.LogDebug("🔍 Filter complexity - TotalActiveFilters: {ActiveFilters}, TotalFilterLength: {TotalLength}, " +
                    "AvgFilterLength: {AvgLength:F1}",
                    _columnSearchFilters.Count, totalFilterLength, 
                    _columnSearchFilters.Count > 0 ? (double)totalFilterLength / _columnSearchFilters.Count : 0);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ ERROR in SetColumnSearchFilter - Column: {ColumnName}, SearchText: '{SearchText}'",
                    columnName, searchText);
                throw;
            }  
            finally
            {
                EndOperation(operationId);
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
        /// Vyčistí všetky search filtre s logovaním
        /// </summary>
        public void ClearAllSearchFilters()
        {
            var operationId = StartOperation("ClearAllSearchFilters");
            
            try
            {
                var clearedCount = _columnSearchFilters.Count;
                var clearedFilters = _columnSearchFilters.Keys.ToList();
                
                _columnSearchFilters.Clear();
                
                _logger.LogInformation("🔍 All search filters CLEARED - ClearedCount: {ClearedCount}, " +
                    "ClearedColumns: [{ClearedColumns}]",
                    clearedCount, string.Join(", ", clearedFilters));
                
                if (clearedCount > 0)
                {
                    _logger.LogDebug("🔍 Filter state reset - Previous active filters: {FilterDetails}",
                        string.Join("; ", clearedFilters.Select(c => $"{c}: '{_columnSearchFilters.GetValueOrDefault(c, "")}'").ToArray()));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ ERROR in ClearAllSearchFilters");
                throw;
            }
            finally
            {
                EndOperation(operationId);
            }
        }

        /// <summary>
        /// Aplikuje search filtre na dáta s kompletným performance a content logovaním
        /// </summary>
        public async Task<List<Dictionary<string, object?>>> ApplySearchFiltersAsync(List<Dictionary<string, object?>> data)
        {
            var operationId = StartOperation("ApplySearchFiltersAsync");
            _totalSearchOperations++;
            
            try
            {
                _logger.LogInformation("🔍 ApplySearchFilters START - InputRows: {InputRows}, ActiveFilters: {ActiveFilters}, " +
                    "Filters: [{FilterDetails}], TotalSearchOps: {TotalOps}",
                    data?.Count ?? 0, _columnSearchFilters.Count,
                    string.Join(", ", _columnSearchFilters.Select(f => $"{f.Key}:'{f.Value}'")),
                    _totalSearchOperations);

                if (!_columnSearchFilters.Any())
                {
                    _logger.LogDebug("🔍 No active filters - returning original data unchanged");
                    return data ?? new List<Dictionary<string, object?>>();
                }

                var result = await Task.Run(() =>
                {
                    var filteredData = new List<Dictionary<string, object?>>();
                    var totalRows = data?.Count ?? 0;
                    var matchedRows = 0;
                    var emptyRowsCount = 0;
                    var filterMisses = new Dictionary<string, int>();

                    if (data == null)
                    {
                        _logger.LogWarning("🔍 Null data provided to ApplySearchFiltersAsync");
                        return new List<Dictionary<string, object?>>();
                    }

                    foreach (var row in data)
                    {
                        // Skontroluj či je riadok prázdny - prázdne riadky sa vždy pridajú na koniec
                        var isEmpty = IsRowEmpty(row);

                        if (isEmpty)
                        {
                            emptyRowsCount++;
                            continue; // Prázdne riadky pridaj neskôr
                        }

                        var matchesAllFilters = true;
                        var rowMatchDetails = new List<string>();

                        foreach (var filter in _columnSearchFilters)
                        {
                            var columnName = filter.Key;
                            var searchText = filter.Value;

                            if (row.TryGetValue(columnName, out var cellValue))
                            {
                                var cellText = cellValue?.ToString() ?? string.Empty;

                                // Case-insensitive obsahuje search
                                var matches = cellText.Contains(searchText, StringComparison.OrdinalIgnoreCase);
                                rowMatchDetails.Add($"{columnName}:{matches}");

                                if (!matches)
                                {
                                    matchesAllFilters = false;
                                    filterMisses[columnName] = filterMisses.GetValueOrDefault(columnName, 0) + 1;
                                    break;
                                }
                            }
                            else
                            {
                                // Ak stĺpec neexistuje, riadok nevyhovuje filtru
                                matchesAllFilters = false;
                                filterMisses[$"{columnName}(missing)"] = filterMisses.GetValueOrDefault($"{columnName}(missing)", 0) + 1;
                                rowMatchDetails.Add($"{columnName}:missing");
                                break;
                            }
                        }

                        if (matchesAllFilters)
                        {
                            filteredData.Add(row);
                            matchedRows++;
                            
                            // Sample logging for first few matches
                            if (matchedRows <= 3 && _logger.IsEnabled(LogLevel.Debug))
                            {
                                _logger.LogDebug("🔍 Row MATCHED - RowIndex: {Index}, MatchDetails: [{MatchDetails}]",
                                    data.IndexOf(row), string.Join(", ", rowMatchDetails));
                            }
                        }
                    }

                    // ✅ KĽÚČOVÉ: Pridaj všetky prázdne riadky na koniec
                    var emptyRows = data.Where(IsRowEmpty).ToList();
                    filteredData.AddRange(emptyRows);

                    // Comprehensive result logging
                    var duration = EndOperation(operationId);
                    var filteredCount = filteredData.Count;
                    var dataRows = totalRows - emptyRowsCount;
                    var filterEfficiency = dataRows > 0 ? (double)matchedRows / dataRows * 100 : 0;

                    _logger.LogInformation("✅ ApplySearchFilters COMPLETED - Duration: {Duration}ms, " +
                        "Input: {InputRows} ({DataRows} data + {EmptyRows} empty), " +
                        "Output: {OutputRows} ({MatchedRows} matched + {EmptyRows} empty), " +
                        "FilterEfficiency: {Efficiency:F1}%, PerformanceRate: {Rate:F0} rows/ms",
                        duration, totalRows, dataRows, emptyRowsCount,
                        filteredCount, matchedRows, emptyRowsCount,
                        filterEfficiency, duration > 0 ? totalRows / duration : 0);

                    // Log filter effectiveness
                    if (filterMisses.Any())
                    {
                        _logger.LogDebug("🔍 Filter miss analysis - {MissDetails}",
                            string.Join(", ", filterMisses.Select(kvp => $"{kvp.Key}: {kvp.Value} misses")));
                    }

                    return filteredData;
                });

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ CRITICAL ERROR in ApplySearchFiltersAsync - InputRows: {InputRows}, " +
                    "ActiveFilters: {ActiveFilters}", data?.Count ?? 0, _columnSearchFilters.Count);
                throw;
            }
        }

        #endregion

        #region ✅ Sort Functionality s Header Click

        /// <summary>
        /// Togglene sort pre stĺpec pri kliknutí na header s kompletným logovaním (None → Ascending → Descending → None)
        /// </summary>
        public SortDirection ToggleColumnSort(string columnName)
        {
            var operationId = StartOperation("ToggleColumnSort");
            
            try
            {
                if (string.IsNullOrWhiteSpace(columnName))
                {
                    _logger.LogWarning("🔀 ToggleColumnSort - Invalid columnName provided (null/empty)");
                    return SortDirection.None;
                }

                var previousSortColumn = _currentSortColumn;
                var currentDirection = _columnSortStates.TryGetValue(columnName, out var direction) ? direction : SortDirection.None;
                var activeSortsBefore = _columnSortStates.Count;

                // Ak je iný stĺpec sortovaný, vyčisti ho
                if (_currentSortColumn != null && _currentSortColumn != columnName)
                {
                    var previousDirection = _columnSortStates.GetValueOrDefault(_currentSortColumn, SortDirection.None);
                    _columnSortStates.Remove(_currentSortColumn);
                    
                    _logger.LogInformation("🔀 Previous sort CLEARED - PreviousColumn: {PreviousColumn}, " +
                        "PreviousDirection: {PreviousDirection}, NewColumn: {NewColumn}",
                        _currentSortColumn, previousDirection, columnName);
                }

                // Toggle current column: None → Asc → Desc → None
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
                    
                    _logger.LogInformation("🔀 Sort REMOVED - Column: {ColumnName}, PreviousDirection: {PreviousDirection}, " +
                        "ActiveSorts: {SortsBefore} → {SortsAfter}",
                        columnName, currentDirection, activeSortsBefore, _columnSortStates.Count);
                }
                else
                {
                    _columnSortStates[columnName] = newDirection;
                    _currentSortColumn = columnName;
                    
                    _logger.LogInformation("🔀 Sort SET - Column: {ColumnName}, Direction: {CurrentDirection} → {NewDirection}, " +
                        "ActiveSorts: {SortsBefore} → {SortsAfter}, SortTransition: {Transition}",
                        columnName, currentDirection, newDirection, activeSortsBefore, _columnSortStates.Count,
                        $"{currentDirection}→{newDirection}");
                }

                var duration = EndOperation(operationId);
                
                _logger.LogDebug("🔀 Sort state updated - Duration: {Duration}ms, CurrentSortColumn: {CurrentColumn}, " +
                    "SortDirection: {Direction}, PreviousColumn: {PreviousColumn}",
                    duration, _currentSortColumn, newDirection, previousSortColumn);

                return newDirection;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ ERROR in ToggleColumnSort - Column: {ColumnName}", columnName);
                throw;
            }
        }

        /// <summary>
        /// Získa aktuálny sort direction pre stĺpec
        /// </summary>
        public SortDirection GetColumnSortDirection(string columnName)
        {
            return _columnSortStates.TryGetValue(columnName, out var direction) ? direction : SortDirection.None;
        }

        /// <summary>
        /// Vyčistí všetky sort stavy s logovaním
        /// </summary>
        public void ClearAllSorts()
        {
            var operationId = StartOperation("ClearAllSorts");
            
            try
            {
                var clearedCount = _columnSortStates.Count;
                var clearedSorts = _columnSortStates.Select(kvp => $"{kvp.Key}:{kvp.Value}").ToList();
                var previousSortColumn = _currentSortColumn;
                
                _columnSortStates.Clear();
                _currentSortColumn = null;
                
                _logger.LogInformation("🔀 All sorts CLEARED - ClearedCount: {ClearedCount}, " +
                    "PreviousSortColumn: {PreviousSortColumn}, ClearedSorts: [{ClearedSorts}]",
                    clearedCount, previousSortColumn, string.Join(", ", clearedSorts));
                
                if (clearedCount > 0)
                {
                    _logger.LogDebug("🔀 Sort state reset - Previous active sorts: {SortDetails}",
                        string.Join("; ", clearedSorts));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ ERROR in ClearAllSorts");
                throw;
            }
            finally
            {
                EndOperation(operationId);
            }
        }

        /// <summary>
        /// Aplikuje sorting na dáta s kompletným performance a data analysis logovaním (prázdne riadky vždy na konci)
        /// </summary>
        public async Task<List<Dictionary<string, object?>>> ApplySortingAsync(List<Dictionary<string, object?>> data)
        {
            var operationId = StartOperation("ApplySortingAsync");
            _totalSortOperations++;
            
            try
            {
                _logger.LogInformation("🔀 ApplySorting START - InputRows: {InputRows}, CurrentSortColumn: {SortColumn}, " +
                    "SortDirection: {SortDirection}, TotalSortOps: {TotalOps}",
                    data?.Count ?? 0, _currentSortColumn, 
                    _currentSortColumn != null ? _columnSortStates.GetValueOrDefault(_currentSortColumn, SortDirection.None) : SortDirection.None,
                    _totalSortOperations);

                if (_currentSortColumn == null || !_columnSortStates.ContainsKey(_currentSortColumn))
                {
                    _logger.LogDebug("🔀 No active sort - returning original data unchanged");
                    return data ?? new List<Dictionary<string, object?>>();
                }

                if (data == null)
                {
                    _logger.LogWarning("🔀 Null data provided to ApplySortingAsync");
                    return new List<Dictionary<string, object?>>();
                }

                var result = await Task.Run(() =>
                {
                    var sortColumn = _currentSortColumn;
                    var sortDirection = _columnSortStates[sortColumn];
                    var totalRows = data.Count;

                    // Rozdel dáta na neprázdne a prázdne riadky
                    var nonEmptyRows = data.Where(row => !IsRowEmpty(row)).ToList();
                    var emptyRows = data.Where(IsRowEmpty).ToList();

                    _logger.LogDebug("🔀 Data segmentation - Total: {TotalRows}, NonEmpty: {NonEmptyRows}, " +
                        "Empty: {EmptyRows}, SortableData: {SortablePercent:F1}%",
                        totalRows, nonEmptyRows.Count, emptyRows.Count,
                        totalRows > 0 ? (double)nonEmptyRows.Count / totalRows * 100 : 0);

                    // Analyzuj sort column data types pre performance insight
                    var sortValueTypes = new Dictionary<string, int>();
                    var nullSortValues = 0;
                    
                    foreach (var row in nonEmptyRows.Take(10)) // Sample first 10 rows
                    {
                        var sortValue = GetSortValue(row, sortColumn);
                        if (sortValue == null)
                        {
                            nullSortValues++;
                        }
                        else
                        {
                            var typeName = sortValue.GetType().Name;
                            sortValueTypes[typeName] = sortValueTypes.GetValueOrDefault(typeName, 0) + 1;
                        }
                    }

                    if (sortValueTypes.Any())
                    {
                        _logger.LogDebug("🔀 Sort data analysis - Column: {SortColumn}, ValueTypes: [{ValueTypes}], " +
                            "NullValues: {NullValues}/10 sampled",
                            sortColumn, string.Join(", ", sortValueTypes.Select(kvp => $"{kvp.Key}:{kvp.Value}")), nullSortValues);
                    }

                    // Sort iba neprázdne riadky
                    List<Dictionary<string, object?>> sortedNonEmptyRows;
                    try
                    {
                        sortedNonEmptyRows = sortDirection == SortDirection.Ascending
                            ? nonEmptyRows.OrderBy(row => GetSortValue(row, sortColumn)).ToList()
                            : nonEmptyRows.OrderByDescending(row => GetSortValue(row, sortColumn)).ToList();
                            
                        _logger.LogDebug("🔀 Sort operation completed successfully - Direction: {Direction}, SortedRows: {SortedCount}",
                            sortDirection, sortedNonEmptyRows.Count);
                    }
                    catch (Exception sortEx)
                    {
                        _logger.LogError(sortEx, "❌ Sort operation failed - Column: {SortColumn}, Direction: {Direction}, " +
                            "DataRows: {DataRows}", sortColumn, sortDirection, nonEmptyRows.Count);
                        throw;
                    }

                    // ✅ KĽÚČOVÉ: Prázdne riadky vždy na koniec
                    var finalResult = new List<Dictionary<string, object?>>();
                    finalResult.AddRange(sortedNonEmptyRows);
                    finalResult.AddRange(emptyRows);

                    // Performance a result analysis
                    var duration = EndOperation(operationId);
                    var sortEfficiency = nonEmptyRows.Count > 0 ? duration / nonEmptyRows.Count : 0;

                    _logger.LogInformation("✅ ApplySorting COMPLETED - Duration: {Duration}ms, " +
                        "Input: {InputRows} ({NonEmptyRows} sortable + {EmptyRows} empty), " +
                        "Output: {OutputRows}, SortColumn: {SortColumn} ({SortDirection}), " +
                        "SortEfficiency: {Efficiency:F3}ms/row, PerformanceRate: {Rate:F0} rows/ms",
                        duration, totalRows, nonEmptyRows.Count, emptyRows.Count,
                        finalResult.Count, sortColumn, sortDirection,
                        sortEfficiency, duration > 0 ? nonEmptyRows.Count / duration : 0);

                    // Log sort result verification (sample first and last few rows)
                    if (sortedNonEmptyRows.Count > 0 && _logger.IsEnabled(LogLevel.Debug))
                    {
                        var firstValue = GetSortValue(sortedNonEmptyRows.First(), sortColumn);
                        var lastValue = sortedNonEmptyRows.Count > 1 ? GetSortValue(sortedNonEmptyRows.Last(), sortColumn) : firstValue;
                        
                        _logger.LogDebug("🔀 Sort verification - FirstValue: '{FirstValue}', LastValue: '{LastValue}', " +
                            "Direction: {Direction}, ProperOrder: {ProperOrder}",
                            firstValue, lastValue, sortDirection,
                            sortDirection == SortDirection.Ascending ? "First≤Last" : "First≥Last");
                    }

                    return finalResult;
                });

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ CRITICAL ERROR in ApplySortingAsync - InputRows: {InputRows}, " +
                    "SortColumn: {SortColumn}, SortDirection: {SortDirection}",
                    data?.Count ?? 0, _currentSortColumn, 
                    _currentSortColumn != null ? _columnSortStates.GetValueOrDefault(_currentSortColumn, SortDirection.None) : SortDirection.None);
                throw;
            }
        }

        #endregion

        #region ✅ NOVÉ: Zebra Rows (Alternating Row Colors)

        /// <summary>
        /// Povolí/zakáže zebra rows effect s kompletným logovaním
        /// </summary>
        public void SetZebraRowsEnabled(bool enabled)
        {
            var operationId = StartOperation("SetZebraRowsEnabled");
            
            try
            {
                var previousState = _zebraRowsEnabled;
                _zebraRowsEnabled = enabled;
                
                _logger.LogInformation("🦓 Zebra rows state CHANGED - PreviousState: {PreviousState}, " +
                    "NewState: {NewState}, StateTransition: {Transition}",
                    previousState, enabled, $"{previousState}→{enabled}");
                
                if (previousState != enabled)
                {
                    _logger.LogDebug("🦓 Zebra configuration updated - Enabled: {Enabled}, " +
                        "RequiresRerender: {RequiresRerender}",
                        enabled, true);
                }
                else
                {
                    _logger.LogDebug("🦓 Zebra state unchanged - Current: {Current}", enabled);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ ERROR in SetZebraRowsEnabled - Enabled: {Enabled}", enabled);
                throw;
            }  
            finally
            {
                EndOperation(operationId);
            }
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
        /// Aplikuje zebra row background na dáta s kompletným styling a performance logovaním
        /// </summary>
        public async Task<List<RowDisplayInfo>> ApplyZebraRowStylingAsync(List<Dictionary<string, object?>> data)
        {
            var operationId = StartOperation("ApplyZebraRowStylingAsync");
            _totalZebraOperations++;
            
            try
            {
                _logger.LogInformation("🦓 ApplyZebraStyling START - InputRows: {InputRows}, ZebraEnabled: {ZebraEnabled}, " +
                    "TotalZebraOps: {TotalOps}",
                    data?.Count ?? 0, _zebraRowsEnabled, _totalZebraOperations);

                if (data == null)
                {
                    _logger.LogWarning("🦓 Null data provided to ApplyZebraRowStylingAsync");
                    return new List<RowDisplayInfo>();
                }

                var result = await Task.Run(() =>
                {
                    var resultList = new List<RowDisplayInfo>();
                    var nonEmptyRowIndex = 0;
                    var zebraRowCount = 0;
                    var emptyRowCount = 0;
                    var totalRows = data.Count;

                    for (int i = 0; i < data.Count; i++)
                    {
                        var row = data[i];
                        var isEmpty = IsRowEmpty(row);
                        var isZebraRow = false;

                        if (isEmpty)
                        {
                            emptyRowCount++;
                        } 
                        else
                        {
                            if (_zebraRowsEnabled)
                            {
                                // Iba neprázdne riadky majú zebra effect
                                isZebraRow = nonEmptyRowIndex % 2 == 1; // Každý druhý neprázdny riadok
                                if (isZebraRow) zebraRowCount++;
                            }
                            nonEmptyRowIndex++;
                        }

                        var rowInfo = new RowDisplayInfo
                        {
                            RowIndex = i,
                            Data = row,
                            IsEmpty = isEmpty,
                            IsZebraRow = isZebraRow,
                            IsEvenNonEmptyRow = !isEmpty && (nonEmptyRowIndex - 1) % 2 == 0
                        };

                        resultList.Add(rowInfo);

                        // Sample logging for first few rows
                        if (i < 5 && _logger.IsEnabled(LogLevel.Debug))
                        {
                            _logger.LogDebug("🦓 Row styled - Index: {Index}, IsEmpty: {IsEmpty}, " +
                                "IsZebraRow: {IsZebraRow}, NonEmptyIndex: {NonEmptyIndex}, StyleClass: {StyleClass}",
                                i, isEmpty, isZebraRow, isEmpty ? -1 : nonEmptyRowIndex - 1, rowInfo.GetRowStyleClass());
                        }
                    }

                    // Performance and styling analysis
                    var duration = EndOperation(operationId);
                    var zebraEfficiency = nonEmptyRowIndex > 0 ? (double)zebraRowCount / nonEmptyRowIndex * 100 : 0;
                    var stylingRate = duration > 0 ? totalRows / duration : 0;

                    _logger.LogInformation("✅ ApplyZebraStyling COMPLETED - Duration: {Duration}ms, " +
                        "Input: {InputRows}, Output: {OutputRows}, " +
                        "NonEmptyRows: {NonEmptyRows}, EmptyRows: {EmptyRows}, " +
                        "ZebraRows: {ZebraRows}, ZebraEfficiency: {ZebraEfficiency:F1}%, " +
                        "StylingRate: {StylingRate:F0} rows/ms, ZebraEnabled: {ZebraEnabled}",
                        duration, totalRows, resultList.Count,
                        nonEmptyRowIndex, emptyRowCount, zebraRowCount, zebraEfficiency,
                        stylingRate, _zebraRowsEnabled);

                    // Log zebra pattern analysis
                    if (_zebraRowsEnabled && nonEmptyRowIndex > 0)
                    {
                        var expectedZebraRows = nonEmptyRowIndex / 2;
                        var patternAccuracy = expectedZebraRows > 0 ? (double)zebraRowCount / expectedZebraRows * 100 : 100;
                        
                        _logger.LogDebug("🦓 Zebra pattern analysis - NonEmptyRows: {NonEmpty}, " +
                            "ExpectedZebraRows: {Expected}, ActualZebraRows: {Actual}, " +
                            "PatternAccuracy: {Accuracy:F1}%",
                            nonEmptyRowIndex, expectedZebraRows, zebraRowCount, patternAccuracy);
                    }

                    // Log style distribution
                    var styleDistribution = resultList.GroupBy(r => r.GetRowStyleClass())
                        .ToDictionary(g => g.Key, g => g.Count());
                    
                    _logger.LogDebug("🦓 Style distribution - {StyleDetails}",
                        string.Join(", ", styleDistribution.Select(kvp => $"{kvp.Key}: {kvp.Value}")));

                    return resultList;
                });

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ CRITICAL ERROR in ApplyZebraRowStylingAsync - InputRows: {InputRows}, " +
                    "ZebraEnabled: {ZebraEnabled}", data?.Count ?? 0, _zebraRowsEnabled);
                throw;
            }
        }

        #endregion

        #region ✅ Combined Search + Sort + Zebra

        /// <summary>
        /// Aplikuje search, potom sort a nakoniec zebra styling na dáta s kompletným pipeline logovaním
        /// </summary>
        public async Task<List<RowDisplayInfo>> ApplyAllFiltersAndStylingAsync(List<Dictionary<string, object?>> data)
        {
            var operationId = StartOperation("ApplyAllFiltersAndStylingAsync");
            
            try
            {
                _logger.LogInformation("🎯 Complete Filter Pipeline START - InputRows: {InputRows}, " +
                    "HasSearchFilters: {HasSearchFilters}, HasSort: {HasSort}, ZebraEnabled: {ZebraEnabled}",
                    data?.Count ?? 0, HasActiveSearchFilters, HasActiveSort, _zebraRowsEnabled);

                if (data == null)
                {
                    _logger.LogWarning("🎯 Null data provided to ApplyAllFiltersAndStylingAsync");
                    return new List<RowDisplayInfo>();
                }

                var originalCount = data.Count;

                // Najprv aplikuj search
                _logger.LogDebug("🎯 Pipeline Step 1: Applying search filters");
                var searchedData = await ApplySearchFiltersAsync(data);

                // Potom aplikuj sort
                _logger.LogDebug("🎯 Pipeline Step 2: Applying sorting");
                var sortedData = await ApplySortingAsync(searchedData);

                // Nakoniec aplikuj zebra styling
                _logger.LogDebug("🎯 Pipeline Step 3: Applying zebra styling");
                var styledData = await ApplyZebraRowStylingAsync(sortedData);

                var duration = EndOperation(operationId);
                var finalCount = styledData.Count;
                var dataReduction = originalCount > 0 ? (double)(originalCount - finalCount) / originalCount * 100 : 0;
                var processingRate = duration > 0 ? originalCount / duration : 0;

                _logger.LogInformation("✅ Complete Filter Pipeline COMPLETED - Duration: {Duration}ms, " +
                    "InputRows: {InputRows}, OutputRows: {OutputRows}, DataReduction: {DataReduction:F1}%, " +
                    "ProcessingRate: {ProcessingRate:F0} rows/ms, " +
                    "Pipeline: Search({SearchFilters}) → Sort({SortColumn}) → Zebra({ZebraEnabled})",
                    duration, originalCount, finalCount, dataReduction, processingRate,
                    _columnSearchFilters.Count, _currentSortColumn ?? "None", _zebraRowsEnabled);

                // Pipeline efficiency analysis
                if (styledData.Any())
                {
                    var zebraRows = styledData.Count(r => r.IsZebraRow);
                    var emptyRows = styledData.Count(r => r.IsEmpty);
                    var dataRows = styledData.Count(r => !r.IsEmpty);
                    
                    _logger.LogDebug("🎯 Pipeline result analysis - DataRows: {DataRows}, EmptyRows: {EmptyRows}, " +
                        "ZebraRows: {ZebraRows}, EmptyRatio: {EmptyRatio:F1}%, ZebraRatio: {ZebraRatio:F1}%",
                        dataRows, emptyRows, zebraRows, 
                        finalCount > 0 ? (double)emptyRows / finalCount * 100 : 0,
                        dataRows > 0 ? (double)zebraRows / dataRows * 100 : 0);
                }

                return styledData;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ CRITICAL ERROR in ApplyAllFiltersAndStylingAsync - InputRows: {InputRows}, " +
                    "SearchFilters: {SearchFilters}, SortColumn: {SortColumn}",
                    data?.Count ?? 0, _columnSearchFilters.Count, _currentSortColumn);
                throw;
            }
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
            
            _logger.LogTrace("⏱️ Operation START - {OperationName} (ID: {OperationId}), TotalCalls: {TotalCalls}",
                operationName, operationId, _operationCounters[operationName]);
                
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
                
                _logger.LogTrace("⏱️ Operation END - ID: {OperationId}, Duration: {Duration:F2}ms", 
                    operationId, duration);
                    
                return duration;
            }
            
            _logger.LogWarning("⏱️ Operation END - Unknown operation ID: {OperationId}", operationId);
            return 0;
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
        /// Získa detailný status info pre debugging s kompletnými metrikami
        /// </summary>
        public string GetStatusInfo()
        {
            var operationId = StartOperation("GetStatusInfo");
            
            try
            {
                var searchCount = _columnSearchFilters.Count;
                var sortInfo = _currentSortColumn != null
                    ? $"{_currentSortColumn} ({_columnSortStates[_currentSortColumn]})"
                    : "None";

                var statusInfo = $"Search: {searchCount} filters, Sort: {sortInfo}, Zebra: {_zebraRowsEnabled}, " +
                    $"TotalOps: S:{_totalSearchOperations}/So:{_totalSortOperations}/Z:{_totalZebraOperations}";

                _logger.LogDebug("📊 Status requested - {StatusInfo}, ActiveOperations: {ActiveOps}",
                    statusInfo, _operationStartTimes.Count);

                return statusInfo;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ ERROR in GetStatusInfo");
                return "Error retrieving status";
            }
            finally
            {
                EndOperation(operationId);
            }
        }

        #endregion

        #region IDisposable

        public void Dispose()
        {
            if (_isDisposed) return;

            try
            {
                // Log final statistics before disposal
                _logger.LogInformation("🧹 SearchAndSortService DISPOSING - FinalStats: " +
                    "SearchOps: {SearchOps}, SortOps: {SortOps}, ZebraOps: {ZebraOps}, " +
                    "ActiveSearchFilters: {ActiveSearchFilters}, ActiveSorts: {ActiveSorts}, " +
                    "PendingOperations: {PendingOps}",
                    _totalSearchOperations, _totalSortOperations, _totalZebraOperations,
                    _columnSearchFilters.Count, _columnSortStates.Count, _operationStartTimes.Count);

                // Clean up search filters
                if (_columnSearchFilters.Any())
                {
                    var filters = _columnSearchFilters.Select(kvp => $"{kvp.Key}:'{kvp.Value}'").ToList();
                    _logger.LogDebug("🧹 Clearing {Count} search filters: [{Filters}]", 
                        _columnSearchFilters.Count, string.Join(", ", filters));
                }
                _columnSearchFilters.Clear();

                // Clean up sort states
                if (_columnSortStates.Any())
                {
                    var sorts = _columnSortStates.Select(kvp => $"{kvp.Key}:{kvp.Value}").ToList();
                    _logger.LogDebug("🧹 Clearing {Count} sort states: [{Sorts}]", 
                        _columnSortStates.Count, string.Join(", ", sorts));
                }
                _columnSortStates.Clear();
                _currentSortColumn = null;

                // Clean up performance tracking
                if (_operationStartTimes.Any())
                {
                    _logger.LogWarning("🧹 Disposing with {Count} pending operations: [{Operations}]",
                        _operationStartTimes.Count, string.Join(", ", _operationStartTimes.Keys));
                }
                _operationStartTimes.Clear();
                _operationCounters.Clear();

                _isDisposed = true;

                _logger.LogInformation("✅ SearchAndSortService DISPOSED successfully - ZebraEnabled: {ZebraEnabled}",
                    _zebraRowsEnabled);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ ERROR during SearchAndSortService disposal");
                
                // Force cleanup even if logging fails
                _columnSearchFilters.Clear();
                _columnSortStates.Clear();
                _operationStartTimes.Clear();
                _operationCounters.Clear();
                _currentSortColumn = null;
                _isDisposed = true;
            }
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