// Services/SearchAndSortService.cs - ✅ ROZŠÍRENÉ o Multi-Sort funkcionalitu
using Microsoft.Extensions.Logging.Abstractions;
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

        // ✅ NOVÉ: Multi-Sort functionality
        private readonly List<MultiSortColumn> _multiSortColumns = new();
        private MultiSortConfiguration _multiSortConfig = MultiSortConfiguration.Default;
        private bool _isMultiSortMode = false;

        // ✅ NOVÉ: Zebra rows (alternating row colors)
        private bool _zebraRowsEnabled = true;

        // ✅ ROZŠÍRENÉ: Performance a state tracking
        private readonly Dictionary<string, DateTime> _operationStartTimes = new();
        private readonly Dictionary<string, int> _operationCounters = new();
        private int _totalSearchOperations = 0;
        private int _totalSortOperations = 0;
        private int _totalZebraOperations = 0;

        // ✅ NOVÉ: Advanced Search functionality
        private AdvancedSearchConfiguration _advancedSearchConfig = AdvancedSearchConfiguration.Default;
        private readonly List<string> _searchHistory = new();
        private readonly Dictionary<string, List<SearchResult>> _searchResultsCache = new();
        private int _totalAdvancedSearchOperations = 0;

        // ✅ NOVÉ: Advanced Filtering functionality
        private readonly Dictionary<string, MultiColumnFilterSet> _filterSets = new();
        private MultiColumnFilterSet? _activeFilterSet;
        private int _totalAdvancedFilteringOperations = 0;

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

        #region ✅ NOVÉ: Advanced Search Functionality

        /// <summary>
        /// Nastaví Advanced Search konfiguráciu
        /// </summary>
        public void SetAdvancedSearchConfiguration(AdvancedSearchConfiguration config)
        {
            var operationId = StartOperation("SetAdvancedSearchConfiguration");

            try
            {
                if (config == null)
                {
                    _logger.LogWarning("🔍 SetAdvancedSearchConfiguration - Null config provided, using default");
                    config = AdvancedSearchConfiguration.Default;
                }

                if (!config.IsValid())
                {
                    _logger.LogWarning("🔍 SetAdvancedSearchConfiguration - Invalid config provided, using default. Config: {Config}",
                        config.GetDescription());
                    config = AdvancedSearchConfiguration.Default;
                }

                var previousConfig = _advancedSearchConfig.GetDescription();
                _advancedSearchConfig = config;

                _logger.LogInformation("🔍 Advanced Search configuration SET - Previous: '{PreviousConfig}', " +
                    "New: '{NewConfig}', Features: FuzzySearch:{FuzzyEnabled}, RegexSearch:{RegexEnabled}, " +
                    "Highlighting:{HighlightEnabled}, History:{HistoryEnabled}",
                    previousConfig, config.GetDescription(), config.EnableFuzzySearch, config.EnableRegexSearch,
                    config.EnableSearchHighlighting, config.EnableSearchHistory);

                // Vyčisti search cache pri zmene konfigurácie
                ClearSearchCache();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ ERROR in SetAdvancedSearchConfiguration");
                throw;
            }
            finally
            {
                EndOperation(operationId);
            }
        }

        /// <summary>
        /// Vykonáva pokročilé vyhľadávanie s fuzzy, regex a highlighting podporou
        /// </summary>
        public async Task<List<SearchResult>> PerformAdvancedSearchAsync(string searchTerm, List<Dictionary<string, object?>> data, List<string>? targetColumns = null)
        {
            var operationId = StartOperation("PerformAdvancedSearchAsync");
            _totalAdvancedSearchOperations++;

            try
            {
                _logger.LogInformation("🔍 AdvancedSearch START - SearchTerm: '{SearchTerm}', InputRows: {InputRows}, " +
                    "TargetColumns: [{TargetColumns}], TotalAdvancedSearchOps: {TotalOps}",
                    searchTerm, data?.Count ?? 0, 
                    targetColumns != null ? string.Join(", ", targetColumns) : "ALL",
                    _totalAdvancedSearchOperations);

                if (string.IsNullOrWhiteSpace(searchTerm))
                {
                    _logger.LogDebug("🔍 Empty search term - returning empty results");
                    return new List<SearchResult>();
                }

                if (data == null || !data.Any())
                {
                    _logger.LogWarning("🔍 Null or empty data provided to PerformAdvancedSearchAsync");
                    return new List<SearchResult>();
                }

                // Check cache first
                var cacheKey = $"{searchTerm}_{string.Join(",", targetColumns ?? new List<string>())}";
                if (_searchResultsCache.TryGetValue(cacheKey, out var cachedResults))
                {
                    _logger.LogDebug("🔍 Returning cached results - CacheKey: {CacheKey}, ResultCount: {ResultCount}",
                        cacheKey, cachedResults.Count);
                    return cachedResults;
                }

                var results = await Task.Run(() =>
                {
                    var searchResults = new List<SearchResult>();
                    var nonEmptyRows = data.Where(row => !IsRowEmpty(row)).ToList();
                    var processedRows = 0;
                    var totalMatches = 0;

                    _logger.LogDebug("🔍 Processing {NonEmptyRows} non-empty rows for advanced search", nonEmptyRows.Count);

                    for (int rowIndex = 0; rowIndex < nonEmptyRows.Count; rowIndex++)
                    {
                        var row = nonEmptyRows[rowIndex];
                        var searchResult = new SearchResult(rowIndex, row);
                        var rowHasMatches = false;

                        // Determine which columns to search
                        var columnsToSearch = targetColumns ?? row.Keys.Where(k => 
                            k != "DeleteRows" && k != "ValidAlerts" && 
                            (_advancedSearchConfig.SearchInHiddenColumns || !k.StartsWith("_"))).ToList();

                        foreach (var columnName in columnsToSearch)
                        {
                            if (row.TryGetValue(columnName, out var cellValue))
                            {
                                var cellText = cellValue?.ToString() ?? string.Empty;
                                if (string.IsNullOrEmpty(cellText)) continue;

                                var matches = FindMatchesInText(columnName, cellText, searchTerm);
                                foreach (var match in matches)
                                {
                                    searchResult.AddMatch(match);
                                    rowHasMatches = true;
                                    totalMatches++;
                                }
                            }
                        }

                        if (rowHasMatches)
                        {
                            searchResults.Add(searchResult);
                        }
                        processedRows++;
                    }

                    // Sort by relevance score (descending)
                    searchResults = searchResults.OrderByDescending(r => r.RelevanceScore)
                        .ThenByDescending(r => r.MatchCount)
                        .ToList();

                    var duration = EndOperation(operationId);
                    var searchEfficiency = processedRows > 0 ? duration / processedRows : 0;

                    _logger.LogInformation("✅ AdvancedSearch COMPLETED - Duration: {Duration}ms, " +
                        "SearchTerm: '{SearchTerm}', ProcessedRows: {ProcessedRows}, " +
                        "ResultRows: {ResultRows}, TotalMatches: {TotalMatches}, " +
                        "SearchEfficiency: {Efficiency:F3}ms/row, PerformanceRate: {Rate:F0} rows/ms",
                        duration, searchTerm, processedRows, searchResults.Count, totalMatches,
                        searchEfficiency, duration > 0 ? processedRows / duration : 0);

                    // Cache results if enabled
                    if (_searchResultsCache.Count < 50) // Limit cache size
                    {
                        _searchResultsCache[cacheKey] = searchResults;
                    }

                    return searchResults;
                });

                // Add to search history
                AddToSearchHistory(searchTerm);

                return results;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ CRITICAL ERROR in PerformAdvancedSearchAsync - SearchTerm: '{SearchTerm}', " +
                    "InputRows: {InputRows}", searchTerm, data?.Count ?? 0);
                throw;
            }
        }

        /// <summary>
        /// Nájde všetky matches v texte pre daný search term
        /// </summary>
        private List<SearchMatch> FindMatchesInText(string columnName, string text, string searchTerm)
        {
            var matches = new List<SearchMatch>();

            try
            {
                // 1. Exact match (case-insensitive unless specified otherwise)
                var exactMatches = FindExactMatches(columnName, text, searchTerm);
                matches.AddRange(exactMatches);

                // 2. Regex match (if enabled and no exact matches found)
                if (_advancedSearchConfig.EnableRegexSearch && !matches.Any())
                {
                    var regexMatches = FindRegexMatches(columnName, text, searchTerm);
                    matches.AddRange(regexMatches);
                }

                // 3. Fuzzy match (if enabled and no other matches found)
                if (_advancedSearchConfig.EnableFuzzySearch && !matches.Any())
                {
                    var fuzzyMatches = FindFuzzyMatches(columnName, text, searchTerm);
                    matches.AddRange(fuzzyMatches);
                }

                _logger.LogTrace("🔍 FindMatchesInText - Column: {ColumnName}, Text: '{TextSample}', " +
                    "SearchTerm: '{SearchTerm}', FoundMatches: {MatchCount}",
                    columnName, text.Length > 50 ? text.Substring(0, 50) + "..." : text, 
                    searchTerm, matches.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ ERROR in FindMatchesInText - Column: {ColumnName}, SearchTerm: '{SearchTerm}'",
                    columnName, searchTerm);
            }

            return matches;
        }

        /// <summary>
        /// Nájde exact matches v texte
        /// </summary>
        private List<SearchMatch> FindExactMatches(string columnName, string text, string searchTerm)
        {
            var matches = new List<SearchMatch>();
            var comparison = _advancedSearchConfig.EnableCaseSensitiveSearch 
                ? StringComparison.Ordinal 
                : StringComparison.OrdinalIgnoreCase;

            int startIndex = 0;
            while (startIndex < text.Length)
            {
                int foundIndex = text.IndexOf(searchTerm, startIndex, comparison);
                if (foundIndex == -1) break;

                // Check whole word matching if enabled
                if (_advancedSearchConfig.EnableWholeWordSearch)
                {
                    bool isWholeWord = (foundIndex == 0 || !char.IsLetterOrDigit(text[foundIndex - 1])) &&
                                       (foundIndex + searchTerm.Length >= text.Length || 
                                        !char.IsLetterOrDigit(text[foundIndex + searchTerm.Length]));
                    
                    if (!isWholeWord)
                    {
                        startIndex = foundIndex + 1;
                        continue;
                    }
                }

                var matchedText = text.Substring(foundIndex, searchTerm.Length);
                var match = new SearchMatch(columnName, searchTerm, matchedText, foundIndex, searchTerm.Length, SearchMatchType.Exact)
                {
                    IsCaseSensitive = _advancedSearchConfig.EnableCaseSensitiveSearch,
                    IsWholeWord = _advancedSearchConfig.EnableWholeWordSearch
                };

                matches.Add(match);
                startIndex = foundIndex + searchTerm.Length;
            }

            return matches;
        }

        /// <summary>
        /// Nájde regex matches v texte
        /// </summary>
        private List<SearchMatch> FindRegexMatches(string columnName, string text, string searchTerm)
        {
            var matches = new List<SearchMatch>();

            try
            {
                // Pokus sa interpretovať search term ako regex pattern
                var regexOptions = _advancedSearchConfig.EnableCaseSensitiveSearch
                    ? System.Text.RegularExpressions.RegexOptions.None
                    : System.Text.RegularExpressions.RegexOptions.IgnoreCase;

                var regex = new System.Text.RegularExpressions.Regex(searchTerm, regexOptions | System.Text.RegularExpressions.RegexOptions.Compiled);
                var regexMatches = regex.Matches(text);

                foreach (System.Text.RegularExpressions.Match regexMatch in regexMatches)
                {
                    if (regexMatch.Success)
                    {
                        var match = new SearchMatch(columnName, searchTerm, regexMatch.Value, 
                            regexMatch.Index, regexMatch.Length, SearchMatchType.Regex)
                        {
                            MatchScore = 0.9 // Slightly lower than exact match
                        };

                        matches.Add(match);
                    }
                }

                _logger.LogTrace("🔍 Regex search - Pattern: '{Pattern}', Text: '{TextSample}', Matches: {MatchCount}",
                    searchTerm, text.Length > 30 ? text.Substring(0, 30) + "..." : text, matches.Count);
            }
            catch (System.Text.RegularExpressions.RegexException regexEx)
            {
                _logger.LogTrace("🔍 Invalid regex pattern - Pattern: '{Pattern}', Error: {Error}",
                    searchTerm, regexEx.Message);
                // Invalid regex pattern - treat as literal text
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ ERROR in FindRegexMatches - SearchTerm: '{SearchTerm}'", searchTerm);
            }

            return matches;
        }

        /// <summary>
        /// Nájde fuzzy matches v texte (simplified Levenshtein-based)
        /// </summary>
        private List<SearchMatch> FindFuzzyMatches(string columnName, string text, string searchTerm)
        {
            var matches = new List<SearchMatch>();

            try
            {
                var words = text.Split(new[] { ' ', '\t', '\n', '\r', ',', ';', '.', '!', '?' }, 
                    StringSplitOptions.RemoveEmptyEntries);

                foreach (var word in words)
                {
                    var similarity = CalculateStringSimilarity(searchTerm.ToLowerInvariant(), word.ToLowerInvariant());
                    
                    if (similarity >= _advancedSearchConfig.FuzzyTolerance)
                    {
                        var wordIndex = text.IndexOf(word, StringComparison.OrdinalIgnoreCase);
                        if (wordIndex >= 0)
                        {
                            var match = new SearchMatch(columnName, searchTerm, word, wordIndex, word.Length, SearchMatchType.Fuzzy)
                            {
                                MatchScore = similarity
                            };

                            matches.Add(match);
                        }
                    }
                }

                _logger.LogTrace("🔍 Fuzzy search - SearchTerm: '{SearchTerm}', Words: {WordCount}, " +
                    "Tolerance: {Tolerance}, Matches: {MatchCount}",
                    searchTerm, words.Length, _advancedSearchConfig.FuzzyTolerance, matches.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ ERROR in FindFuzzyMatches - SearchTerm: '{SearchTerm}'", searchTerm);
            }

            return matches;
        }

        /// <summary>
        /// Vypočíta podobnosť medzi dvoma stringami (0.0 - 1.0)
        /// </summary>
        private double CalculateStringSimilarity(string s1, string s2)
        {
            if (string.IsNullOrEmpty(s1) || string.IsNullOrEmpty(s2))
                return 0.0;

            if (s1 == s2)
                return 1.0;

            // Simplified Levenshtein distance calculation
            var distance = CalculateLevenshteinDistance(s1, s2);
            var maxLength = Math.Max(s1.Length, s2.Length);
            
            return maxLength > 0 ? 1.0 - (double)distance / maxLength : 0.0;
        }

        /// <summary>
        /// Vypočíta Levenshtein distance medzi dvoma stringami
        /// </summary>
        private int CalculateLevenshteinDistance(string s1, string s2)
        {
            if (s1 == s2) return 0;
            if (s1.Length == 0) return s2.Length;
            if (s2.Length == 0) return s1.Length;

            var matrix = new int[s1.Length + 1, s2.Length + 1];

            for (int i = 0; i <= s1.Length; i++)
                matrix[i, 0] = i;

            for (int j = 0; j <= s2.Length; j++)
                matrix[0, j] = j;

            for (int i = 1; i <= s1.Length; i++)
            {
                for (int j = 1; j <= s2.Length; j++)
                {
                    int cost = s1[i - 1] == s2[j - 1] ? 0 : 1;
                    matrix[i, j] = Math.Min(Math.Min(
                        matrix[i - 1, j] + 1,      // deletion
                        matrix[i, j - 1] + 1),     // insertion
                        matrix[i - 1, j - 1] + cost); // substitution
                }
            }

            return matrix[s1.Length, s2.Length];
        }

        /// <summary>
        /// Pridá search term do histórie
        /// </summary>
        private void AddToSearchHistory(string searchTerm)
        {
            if (!_advancedSearchConfig.EnableSearchHistory || string.IsNullOrWhiteSpace(searchTerm))
                return;

            try
            {
                // Remove if already exists
                _searchHistory.RemoveAll(term => term.Equals(searchTerm, StringComparison.OrdinalIgnoreCase));
                
                // Add to beginning
                _searchHistory.Insert(0, searchTerm);
                
                // Trim to max size
                while (_searchHistory.Count > _advancedSearchConfig.MaxSearchHistoryItems)
                {
                    _searchHistory.RemoveAt(_searchHistory.Count - 1);
                }

                _logger.LogTrace("🔍 Added to search history - Term: '{SearchTerm}', HistorySize: {HistorySize}",
                    searchTerm, _searchHistory.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ ERROR in AddToSearchHistory - SearchTerm: '{SearchTerm}'", searchTerm);
            }
        }

        /// <summary>
        /// Získa search históriu
        /// </summary>
        public List<string> GetSearchHistory()
        {
            return new List<string>(_searchHistory);
        }

        /// <summary>
        /// Vyčistí search cache
        /// </summary>
        public void ClearSearchCache()
        {
            var operationId = StartOperation("ClearSearchCache");

            try
            {
                var cacheSize = _searchResultsCache.Count;
                _searchResultsCache.Clear();

                _logger.LogInformation("🔍 Search cache CLEARED - ClearedEntries: {ClearedEntries}", cacheSize);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ ERROR in ClearSearchCache");
                throw;
            }
            finally
            {
                EndOperation(operationId);
            }
        }

        /// <summary>
        /// Vyčistí search históriu
        /// </summary>
        public void ClearSearchHistory()
        {
            var operationId = StartOperation("ClearSearchHistory");

            try
            {
                var historySize = _searchHistory.Count;
                _searchHistory.Clear();

                _logger.LogInformation("🔍 Search history CLEARED - ClearedEntries: {ClearedEntries}", historySize);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ ERROR in ClearSearchHistory");
                throw;
            }
            finally
            {
                EndOperation(operationId);
            }
        }

        #endregion

        #region ✅ ROZŠÍRENÉ: Advanced Filtering Functionality

        /// <summary>
        /// Vytvorí nový filter set
        /// </summary>
        public string CreateFilterSet(string name)
        {
            var operationId = StartOperation("CreateFilterSet");

            try
            {
                if (string.IsNullOrWhiteSpace(name))
                {
                    name = $"FilterSet_{Guid.NewGuid():N}"[..16];
                }

                var filterSet = new MultiColumnFilterSet { Name = name };
                _filterSets[name] = filterSet;

                _logger.LogInformation("🔍 Filter set CREATED - Name: {FilterSetName}, TotalFilterSets: {TotalSets}",
                    name, _filterSets.Count);

                return name;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ ERROR in CreateFilterSet - Name: {FilterSetName}", name);
                throw;
            }
            finally
            {
                EndOperation(operationId);
            }
        }

        /// <summary>
        /// Nastaví aktívny filter set
        /// </summary>
        public void SetActiveFilterSet(string name)
        {
            var operationId = StartOperation("SetActiveFilterSet");

            try
            {
                if (_filterSets.TryGetValue(name, out var filterSet))
                {
                    var previousActiveSet = _activeFilterSet?.Name ?? "None";
                    _activeFilterSet = filterSet;

                    _logger.LogInformation("🔍 Active filter set CHANGED - PreviousSet: {PreviousSet}, " +
                        "NewSet: {NewSet}, FilterCount: {FilterCount}",
                        previousActiveSet, name, filterSet.Filters.Count);
                }
                else
                {
                    _logger.LogWarning("🔍 Filter set not found - Name: {FilterSetName}, AvailableSets: [{AvailableSets}]",
                        name, string.Join(", ", _filterSets.Keys));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ ERROR in SetActiveFilterSet - Name: {FilterSetName}", name);
                throw;
            }
            finally
            {
                EndOperation(operationId);
            }
        }

        /// <summary>
        /// Pridá filter do aktívneho filter set
        /// </summary>
        public void AddAdvancedFilter(string columnName, FilterOperator filterOperator, object? value, object? secondValue = null, bool caseSensitive = false)
        {
            var operationId = StartOperation("AddAdvancedFilter");

            try
            {
                if (_activeFilterSet == null)
                {
                    _logger.LogWarning("🔍 No active filter set - creating default set");
                    var defaultSetName = CreateFilterSet("DefaultFilters");
                    SetActiveFilterSet(defaultSetName);
                }

                var filter = new AdvancedFilter
                {
                    Name = $"{columnName}_{filterOperator}_{Guid.NewGuid():N}"[..16],
                    ColumnName = columnName,
                    Operator = filterOperator,
                    Value = value,
                    SecondValue = secondValue,
                    CaseSensitive = caseSensitive,
                    IsEnabled = true
                };

                _activeFilterSet!.AddFilter(filter);

                _logger.LogInformation("🔍 Advanced filter ADDED - FilterSet: {FilterSetName}, " +
                    "Column: {ColumnName}, Operator: {Operator}, Value: '{Value}', " +
                    "SecondValue: '{SecondValue}', CaseSensitive: {CaseSensitive}, TotalFilters: {TotalFilters}",
                    _activeFilterSet.Name, columnName, filterOperator, value, secondValue, caseSensitive,
                    _activeFilterSet.Filters.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ ERROR in AddAdvancedFilter - Column: {ColumnName}, Operator: {Operator}",
                    columnName, filterOperator);
                throw;
            }
            finally
            {
                EndOperation(operationId);
            }
        }

        /// <summary>
        /// Aplikuje pokročilé filtre na dáta
        /// </summary>
        public async Task<List<Dictionary<string, object?>>> ApplyAdvancedFiltersAsync(List<Dictionary<string, object?>> data)
        {
            var operationId = StartOperation("ApplyAdvancedFiltersAsync");
            _totalAdvancedFilteringOperations++;

            try
            {
                _logger.LogInformation("🔍 AdvancedFiltering START - InputRows: {InputRows}, " +
                    "ActiveFilterSet: {ActiveFilterSet}, TotalAdvancedFilterOps: {TotalOps}",
                    data?.Count ?? 0, _activeFilterSet?.Name ?? "None", _totalAdvancedFilteringOperations);

                if (_activeFilterSet == null || !_activeFilterSet.Filters.Any(f => f.IsEnabled))
                {
                    _logger.LogDebug("🔍 No active advanced filters - returning original data");
                    return data ?? new List<Dictionary<string, object?>>();
                }

                if (data == null)
                {
                    _logger.LogWarning("🔍 Null data provided to ApplyAdvancedFiltersAsync");
                    return new List<Dictionary<string, object?>>();
                }

                var result = await Task.Run(() =>
                {
                    var totalRows = data.Count;
                    var filteredRows = new List<Dictionary<string, object?>>();
                    var nonEmptyRows = data.Where(row => !IsRowEmpty(row)).ToList();
                    var emptyRows = data.Where(IsRowEmpty).ToList();
                    var matchedRows = 0;
                    var filterMisses = new Dictionary<string, int>();

                    _logger.LogDebug("🔍 Processing {NonEmptyRows} non-empty rows for advanced filtering", nonEmptyRows.Count);

                    // Apply advanced filters to non-empty rows only
                    foreach (var row in nonEmptyRows)
                    {
                        bool passesAllFilters = _activeFilterSet.ApplyFilters(row);

                        if (passesAllFilters)
                        {
                            filteredRows.Add(row);
                            matchedRows++;
                        }
                        else
                        {
                            // Track which filters caused rejections
                            foreach (var filter in _activeFilterSet.Filters.Where(f => f.IsEnabled))
                            {
                                if (!filter.ApplyFilter(row))
                                {
                                    var filterKey = $"{filter.ColumnName}:{filter.Operator}";
                                    filterMisses[filterKey] = filterMisses.GetValueOrDefault(filterKey, 0) + 1;
                                }
                            }
                        }
                    }

                    // ✅ KĽÚČOVÉ: Prázdne riadky vždy na koniec
                    filteredRows.AddRange(emptyRows);

                    var duration = EndOperation(operationId);
                    var filterEfficiency = nonEmptyRows.Count > 0 ? (double)matchedRows / nonEmptyRows.Count * 100 : 0;
                    var filteringRate = duration > 0 ? totalRows / duration : 0;

                    _logger.LogInformation("✅ AdvancedFiltering COMPLETED - Duration: {Duration}ms, " +
                        "Input: {InputRows} ({NonEmptyRows} data + {EmptyRows} empty), " +
                        "Output: {OutputRows} ({MatchedRows} matched + {EmptyRows} empty), " +
                        "FilterEfficiency: {Efficiency:F1}%, FilteringRate: {Rate:F0} rows/ms, " +
                        "ActiveFilters: {ActiveFilters}",
                        duration, totalRows, nonEmptyRows.Count, emptyRows.Count,
                        filteredRows.Count, matchedRows, emptyRows.Count,
                        filterEfficiency, filteringRate, 
                        _activeFilterSet.Filters.Count(f => f.IsEnabled));

                    // Log filter effectiveness
                    if (filterMisses.Any())
                    {
                        _logger.LogDebug("🔍 Filter miss analysis - {MissDetails}",
                            string.Join(", ", filterMisses.Select(kvp => $"{kvp.Key}: {kvp.Value} misses")));
                    }

                    // Log filter set diagnostics
                    _logger.LogDebug("🔍 Filter set diagnostics - {FilterSetInfo}",
                        _activeFilterSet.GetDiagnosticInfo());

                    return filteredRows;
                });

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ CRITICAL ERROR in ApplyAdvancedFiltersAsync - InputRows: {InputRows}, " +
                    "ActiveFilterSet: {ActiveFilterSet}", data?.Count ?? 0, _activeFilterSet?.Name);
                throw;
            }
        }

        /// <summary>
        /// Vyčistí všetky filtre z aktívneho filter set
        /// </summary>
        public void ClearAdvancedFilters()
        {
            var operationId = StartOperation("ClearAdvancedFilters");

            try
            {
                if (_activeFilterSet != null)
                {
                    var clearedCount = _activeFilterSet.Filters.Count;
                    var clearedFilters = _activeFilterSet.Filters.Select(f => f.GetDiagnosticInfo()).ToList();

                    _activeFilterSet.ClearFilters();

                    _logger.LogInformation("🔍 Advanced filters CLEARED - FilterSet: {FilterSetName}, " +
                        "ClearedCount: {ClearedCount}",
                        _activeFilterSet.Name, clearedCount);

                    if (clearedCount > 0)
                    {
                        _logger.LogDebug("🔍 Cleared advanced filters - {ClearedFilters}",
                            string.Join("; ", clearedFilters));
                    }
                }
                else
                {
                    _logger.LogDebug("🔍 No active filter set to clear");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ ERROR in ClearAdvancedFilters");
                throw;
            }
            finally
            {
                EndOperation(operationId);
            }
        }

        /// <summary>
        /// Odstráni konkrétny filter
        /// </summary>
        public void RemoveAdvancedFilter(string filterId)
        {
            var operationId = StartOperation("RemoveAdvancedFilter");

            try
            {
                if (_activeFilterSet != null)
                {
                    var filtersBefore = _activeFilterSet.Filters.Count;
                    _activeFilterSet.RemoveFilter(filterId);
                    var filtersAfter = _activeFilterSet.Filters.Count;

                    if (filtersBefore != filtersAfter)
                    {
                        _logger.LogInformation("🔍 Advanced filter REMOVED - FilterSet: {FilterSetName}, " +
                            "FilterId: {FilterId}, FilterCount: {FiltersBefore} → {FiltersAfter}",
                            _activeFilterSet.Name, filterId, filtersBefore, filtersAfter);
                    }
                    else
                    {
                        _logger.LogWarning("🔍 Filter not found for removal - FilterId: {FilterId}, " +
                            "AvailableFilters: [{AvailableFilters}]",
                            filterId, string.Join(", ", _activeFilterSet.Filters.Select(f => f.Id)));
                    }
                }
                else
                {
                    _logger.LogWarning("🔍 No active filter set to remove filter from");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ ERROR in RemoveAdvancedFilter - FilterId: {FilterId}", filterId);
                throw;
            }
            finally
            {
                EndOperation(operationId);
            }
        }

        /// <summary>
        /// Získa všetky dostupné filter sets
        /// </summary>
        public Dictionary<string, string> GetAvailableFilterSets()
        {
            return _filterSets.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.GetDiagnosticInfo());
        }

        /// <summary>
        /// Získa aktuálny aktívny filter set
        /// </summary>
        public MultiColumnFilterSet? GetActiveFilterSet()
        {
            return _activeFilterSet;
        }

        /// <summary>
        /// Vytvára quick filter (helper metódy)
        /// </summary>
        public void AddQuickTextFilter(string columnName, string value, FilterOperator op = FilterOperator.Contains, bool caseSensitive = false)
        {
            AddAdvancedFilter(columnName, op, value, caseSensitive: caseSensitive);
        }

        public void AddQuickNumberRangeFilter(string columnName, decimal minValue, decimal maxValue)
        {
            AddAdvancedFilter(columnName, FilterOperator.Between, minValue, maxValue);
        }

        public void AddQuickEmptyFilter(string columnName, bool isEmpty = true)
        {
            AddAdvancedFilter(columnName, isEmpty ? FilterOperator.IsEmpty : FilterOperator.IsNotEmpty, null);
        }

        public void AddQuickRegexFilter(string columnName, string pattern, bool caseSensitive = false)
        {
            AddAdvancedFilter(columnName, FilterOperator.Regex, pattern, caseSensitive: caseSensitive);
        }

        public void AddQuickInListFilter(string columnName, string commaSeparatedValues, bool caseSensitive = false)
        {
            AddAdvancedFilter(columnName, FilterOperator.In, commaSeparatedValues, caseSensitive: caseSensitive);
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

        #region ✅ NOVÉ: Multi-Sort Functionality

        /// <summary>
        /// Nastaví Multi-Sort konfiguráciu
        /// </summary>
        public void SetMultiSortConfiguration(MultiSortConfiguration config)
        {
            var operationId = StartOperation("SetMultiSortConfiguration");

            try
            {
                if (config == null)
                {
                    _logger.LogWarning("🔢 SetMultiSortConfiguration - Null config provided, using default");
                    config = MultiSortConfiguration.Default;
                }

                if (!config.IsValid())
                {
                    _logger.LogWarning("🔢 SetMultiSortConfiguration - Invalid config provided, using default. Config: {Config}",
                        config.GetDescription());
                    config = MultiSortConfiguration.Default;
                }

                var previousConfig = _multiSortConfig.GetDescription();
                _multiSortConfig = config;

                _logger.LogInformation("🔢 Multi-Sort configuration SET - Previous: '{PreviousConfig}', " +
                    "New: '{NewConfig}', IsEnabled: {IsEnabled}, MaxColumns: {MaxColumns}",
                    previousConfig, config.GetDescription(), config.IsEnabled, config.MaxSortColumns);

                // Ak je Multi-Sort zakázané, vyčisti všetky multi-sort stavy
                if (!config.IsEnabled)
                {
                    ClearMultiSort();
                    _isMultiSortMode = false;
                    _logger.LogInformation("🔢 Multi-Sort DISABLED - cleared all multi-sort states");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ ERROR in SetMultiSortConfiguration");
                throw;
            }
            finally
            {
                EndOperation(operationId);
            }
        }

        /// <summary>
        /// Povolí/zakáže Multi-Sort režim
        /// </summary>
        public void SetMultiSortMode(bool enabled)
        {
            var operationId = StartOperation("SetMultiSortMode");

            try
            {
                if (!_multiSortConfig.IsEnabled && enabled)
                {
                    _logger.LogWarning("🔢 Cannot enable Multi-Sort mode - Multi-Sort is disabled in configuration");
                    return;
                }

                var previousMode = _isMultiSortMode;
                _isMultiSortMode = enabled;

                _logger.LogInformation("🔢 Multi-Sort mode CHANGED - PreviousMode: {PreviousMode}, " +
                    "NewMode: {NewMode}, ModeTransition: {Transition}",
                    previousMode, enabled, $"{previousMode}→{enabled}");

                // Ak sa Multi-Sort režim vypol, prepni na single-sort
                if (!enabled && _multiSortColumns.Any())
                {
                    ConvertMultiSortToSingleSort();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ ERROR in SetMultiSortMode - Enabled: {Enabled}", enabled);
                throw;
            }
            finally
            {
                EndOperation(operationId);
            }
        }

        /// <summary>
        /// Pridá alebo aktualizuje stĺpec v Multi-Sort (Ctrl+klik na header)
        /// </summary>
        public MultiSortColumn? AddOrUpdateMultiSort(string columnName, bool isCtrlPressed = false)
        {
            var operationId = StartOperation("AddOrUpdateMultiSort");

            try
            {
                if (string.IsNullOrWhiteSpace(columnName))
                {
                    _logger.LogWarning("🔢 AddOrUpdateMultiSort - Invalid columnName provided (null/empty)");
                    return null;
                }

                if (!_multiSortConfig.IsEnabled)
                {
                    _logger.LogDebug("🔢 Multi-Sort is disabled - using single sort for column: {ColumnName}", columnName);
                    var singleDirection = ToggleColumnSort(columnName);
                    return new MultiSortColumn(columnName, singleDirection, 1);
                }

                // Ak nie je Ctrl stlačené a nie sme v Multi-Sort režime, použij single sort
                if (!isCtrlPressed && !_isMultiSortMode)
                {
                    ClearMultiSort();
                    var singleDirection = ToggleColumnSort(columnName);
                    return new MultiSortColumn(columnName, singleDirection, 1);
                }

                // Multi-Sort logika
                var existingColumn = _multiSortColumns.FirstOrDefault(c => c.ColumnName.Equals(columnName, StringComparison.OrdinalIgnoreCase));
                var activeColumnsBefore = _multiSortColumns.Count;

                if (existingColumn != null)
                {
                    // Toggle existing column
                    var newDirection = existingColumn.Direction switch
                    {
                        SortDirection.None => SortDirection.Ascending,
                        SortDirection.Ascending => SortDirection.Descending,
                        SortDirection.Descending => SortDirection.None,
                        _ => SortDirection.None
                    };

                    if (newDirection == SortDirection.None)
                    {
                        // Odober stĺpec z multi-sort
                        _multiSortColumns.Remove(existingColumn);
                        ReassignPriorities();

                        _logger.LogInformation("🔢 Multi-Sort column REMOVED - Column: {ColumnName}, " +
                            "PreviousDirection: {PreviousDirection}, ActiveColumns: {ColumnsBefore} → {ColumnsAfter}",
                            columnName, existingColumn.Direction, activeColumnsBefore, _multiSortColumns.Count);

                        return null;
                    }
                    else
                    {
                        // Aktualizuj direction
                        existingColumn.Direction = newDirection;
                        existingColumn.AddedAt = DateTime.UtcNow;

                        _logger.LogInformation("🔢 Multi-Sort column UPDATED - Column: {ColumnName}, " +
                            "Direction: {PreviousDirection} → {NewDirection}, Priority: {Priority}",
                            columnName, existingColumn.Direction, newDirection, existingColumn.Priority);

                        return existingColumn.Clone();
                    }
                }
                else
                {
                    // Pridaj nový stĺpec
                    if (_multiSortColumns.Count >= _multiSortConfig.MaxSortColumns)
                    {
                        if (_multiSortConfig.AutoClearOldSorts)
                        {
                            var oldestColumn = _multiSortColumns.OrderBy(c => c.AddedAt).First();
                            _multiSortColumns.Remove(oldestColumn);

                            _logger.LogInformation("🔢 Multi-Sort AUTO-CLEAR - Removed oldest column: {OldestColumn} " +
                                "to make space for: {NewColumn}", oldestColumn.ColumnName, columnName);
                        }
                        else
                        {
                            _logger.LogWarning("🔢 Multi-Sort limit reached - MaxColumns: {MaxColumns}, " +
                                "Cannot add column: {ColumnName}", _multiSortConfig.MaxSortColumns, columnName);
                            return null;
                        }
                    }

                    var priority = _multiSortConfig.PriorityMode == SortPriorityMode.Sequential
                        ? _multiSortColumns.Count + 1
                        : (int)(DateTime.UtcNow.Ticks % 1000000); // Simplified timestamp priority

                    var newColumn = new MultiSortColumn(columnName, SortDirection.Ascending, priority);
                    _multiSortColumns.Add(newColumn);

                    if (_multiSortConfig.PriorityMode == SortPriorityMode.Sequential)
                    {
                        ReassignPriorities();
                    }

                    _logger.LogInformation("🔢 Multi-Sort column ADDED - Column: {ColumnName}, " +
                        "Direction: {Direction}, Priority: {Priority}, ActiveColumns: {ColumnsBefore} → {ColumnsAfter}",
                        columnName, newColumn.Direction, newColumn.Priority, activeColumnsBefore, _multiSortColumns.Count);

                    return newColumn.Clone();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ ERROR in AddOrUpdateMultiSort - Column: {ColumnName}, CtrlPressed: {CtrlPressed}",
                    columnName, isCtrlPressed);
                throw;
            }
            finally
            {
                EndOperation(operationId);
            }
        }

        /// <summary>
        /// Získa všetky Multi-Sort stĺpce usporiadané podľa priority
        /// </summary>
        public List<MultiSortColumn> GetMultiSortColumns()
        {
            return _multiSortColumns
                .OrderBy(c => c.Priority)
                .ThenBy(c => c.AddedAt)
                .Select(c => c.Clone())
                .ToList();
        }

        /// <summary>
        /// Vyčistí všetky Multi-Sort stavy
        /// </summary>
        public void ClearMultiSort()
        {
            var operationId = StartOperation("ClearMultiSort");

            try
            {
                var clearedCount = _multiSortColumns.Count;
                var clearedColumns = _multiSortColumns.Select(c => c.GetDisplayText()).ToList();

                _multiSortColumns.Clear();

                _logger.LogInformation("🔢 Multi-Sort CLEARED - ClearedCount: {ClearedCount}, " +
                    "ClearedColumns: [{ClearedColumns}]",
                    clearedCount, string.Join(", ", clearedColumns));

                if (clearedCount > 0)
                {
                    _logger.LogDebug("🔢 Multi-Sort state reset - Previous columns: {ColumnDetails}",
                        string.Join("; ", clearedColumns));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ ERROR in ClearMultiSort");
                throw;
            }
            finally
            {
                EndOperation(operationId);
            }
        }

        /// <summary>
        /// Aplikuje Multi-Sort na dáta
        /// </summary>
        public async Task<List<Dictionary<string, object?>>> ApplyMultiSortAsync(List<Dictionary<string, object?>> data)
        {
            var operationId = StartOperation("ApplyMultiSortAsync");

            try
            {
                _logger.LogInformation("🔢 ApplyMultiSort START - InputRows: {InputRows}, " +
                    "ActiveSortColumns: {ActiveColumns}, MultiSortColumns: [{SortColumns}]",
                    data?.Count ?? 0, _multiSortColumns.Count,
                    string.Join(", ", _multiSortColumns.Select(c => c.GetDisplayText())));

                if (!_multiSortColumns.Any())
                {
                    _logger.LogDebug("🔢 No Multi-Sort columns active - returning original data");
                    return data ?? new List<Dictionary<string, object?>>();
                }

                if (data == null)
                {
                    _logger.LogWarning("🔢 Null data provided to ApplyMultiSortAsync");
                    return new List<Dictionary<string, object?>>();
                }

                var result = await Task.Run(() =>
                {
                    var totalRows = data.Count;

                    // Rozdel dáta na neprázdne a prázdne riadky
                    var nonEmptyRows = data.Where(row => !IsRowEmpty(row)).ToList();
                    var emptyRows = data.Where(IsRowEmpty).ToList();

                    _logger.LogDebug("🔢 Multi-Sort data segmentation - Total: {TotalRows}, " +
                        "NonEmpty: {NonEmptyRows}, Empty: {EmptyRows}",
                        totalRows, nonEmptyRows.Count, emptyRows.Count);

                    // Aplikuj Multi-Sort iba na neprázdne riadky
                    var sortedColumns = _multiSortColumns.OrderBy(c => c.Priority).ToList();
                    
                    try
                    {
                        IOrderedEnumerable<Dictionary<string, object?>>? orderedQuery = null;

                        for (int i = 0; i < sortedColumns.Count; i++)
                        {
                            var sortColumn = sortedColumns[i];
                            var isFirstSort = i == 0;

                            _logger.LogTrace("🔢 Applying sort step {Step}/{Total} - Column: {ColumnName}, " +
                                "Direction: {Direction}, Priority: {Priority}",
                                i + 1, sortedColumns.Count, sortColumn.ColumnName, sortColumn.Direction, sortColumn.Priority);

                            if (isFirstSort)
                            {
                                // Prvi sort - OrderBy alebo OrderByDescending
                                orderedQuery = sortColumn.Direction == SortDirection.Ascending
                                    ? nonEmptyRows.OrderBy(row => GetSortValue(row, sortColumn.ColumnName))
                                    : nonEmptyRows.OrderByDescending(row => GetSortValue(row, sortColumn.ColumnName));
                            }
                            else
                            {
                                // Dodatočné sorts - ThenBy alebo ThenByDescending
                                orderedQuery = sortColumn.Direction == SortDirection.Ascending
                                    ? orderedQuery!.ThenBy(row => GetSortValue(row, sortColumn.ColumnName))
                                    : orderedQuery!.ThenByDescending(row => GetSortValue(row, sortColumn.ColumnName));
                            }
                        }

                        var sortedNonEmptyRows = orderedQuery?.ToList() ?? nonEmptyRows;

                        _logger.LogDebug("🔢 Multi-Sort steps completed - SortSteps: {SortSteps}, SortedRows: {SortedCount}",
                            sortedColumns.Count, sortedNonEmptyRows.Count);

                        // ✅ KĽÚČOVÉ: Prázdne riadky vždy na koniec
                        var finalResult = new List<Dictionary<string, object?>>();
                        finalResult.AddRange(sortedNonEmptyRows);
                        finalResult.AddRange(emptyRows);

                        // Performance a result analysis
                        var duration = EndOperation(operationId);
                        var sortEfficiency = nonEmptyRows.Count > 0 ? duration / nonEmptyRows.Count : 0;

                        _logger.LogInformation("✅ ApplyMultiSort COMPLETED - Duration: {Duration}ms, " +
                            "Input: {InputRows} ({NonEmptyRows} sortable + {EmptyRows} empty), " +
                            "Output: {OutputRows}, SortSteps: {SortSteps}, " +
                            "SortEfficiency: {Efficiency:F3}ms/row, PerformanceRate: {Rate:F0} rows/ms",
                            duration, totalRows, nonEmptyRows.Count, emptyRows.Count,
                            finalResult.Count, sortedColumns.Count,
                            sortEfficiency, duration > 0 ? nonEmptyRows.Count / duration : 0);

                        // Log Multi-Sort result verification (sample first few rows)
                        if (sortedNonEmptyRows.Count > 0 && _logger.IsEnabled(LogLevel.Debug))
                        {
                            var sampleSize = Math.Min(3, sortedNonEmptyRows.Count);
                            var sampleRows = sortedNonEmptyRows.Take(sampleSize).ToList();
                            var sampleDetails = sampleRows.Select((row, idx) =>
                            {
                                var values = sortedColumns.Select(sc => $"{sc.ColumnName}={GetSortValue(row, sc.ColumnName)}");
                                return $"Row{idx + 1}[{string.Join(", ", values)}]";
                            });

                            _logger.LogDebug("🔢 Multi-Sort verification sample - {SampleDetails}",
                                string.Join("; ", sampleDetails));
                        }

                        return finalResult;
                    }
                    catch (Exception sortEx)
                    {
                        _logger.LogError(sortEx, "❌ Multi-Sort operation failed - Columns: {SortColumns}, " +
                            "DataRows: {DataRows}", 
                            string.Join(", ", sortedColumns.Select(c => c.ColumnName)), nonEmptyRows.Count);
                        throw;
                    }
                });

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ CRITICAL ERROR in ApplyMultiSortAsync - InputRows: {InputRows}, " +
                    "SortColumns: {SortColumns}", 
                    data?.Count ?? 0, string.Join(", ", _multiSortColumns.Select(c => c.ColumnName)));
                throw;
            }
        }

        /// <summary>
        /// Preradí priority Multi-Sort stĺpcov na sekvenčné čísla (1, 2, 3...)
        /// </summary>
        private void ReassignPriorities()
        {
            var sortedByAddedTime = _multiSortColumns.OrderBy(c => c.AddedAt).ToList();
            
            for (int i = 0; i < sortedByAddedTime.Count; i++)
            {
                sortedByAddedTime[i].Priority = i + 1;
            }

            _logger.LogTrace("🔢 Priorities reassigned - {PriorityDetails}",
                string.Join(", ", sortedByAddedTime.Select(c => $"{c.ColumnName}:{c.Priority}")));
        }

        /// <summary>
        /// Konvertuje Multi-Sort na single sort (ponechá iba najvyššiu prioritu)
        /// </summary>
        private void ConvertMultiSortToSingleSort()
        {
            if (!_multiSortColumns.Any()) return;

            var primarySort = _multiSortColumns.OrderBy(c => c.Priority).First();
            var removedColumns = _multiSortColumns.Where(c => c != primarySort).ToList();

            _multiSortColumns.Clear();
            ClearAllSorts();

            // Nastav single sort
            _columnSortStates[primarySort.ColumnName] = primarySort.Direction;
            _currentSortColumn = primarySort.ColumnName;

            _logger.LogInformation("🔢 Multi-Sort converted to Single-Sort - KeptColumn: {KeptColumn}, " +
                "Direction: {Direction}, RemovedColumns: [{RemovedColumns}]",
                primarySort.ColumnName, primarySort.Direction,
                string.Join(", ", removedColumns.Select(c => c.ColumnName)));
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
                    "HasSearchFilters: {HasSearchFilters}, HasAdvancedFilters: {HasAdvancedFilters}, " +
                    "HasSort: {HasSort}, ZebraEnabled: {ZebraEnabled}",
                    data?.Count ?? 0, HasActiveSearchFilters, HasActiveAdvancedFilters, HasActiveSort, _zebraRowsEnabled);

                if (data == null)
                {
                    _logger.LogWarning("🎯 Null data provided to ApplyAllFiltersAndStylingAsync");
                    return new List<RowDisplayInfo>();
                }

                var originalCount = data.Count;

                // Najprv aplikuj basic search
                _logger.LogDebug("🎯 Pipeline Step 1: Applying basic search filters");
                var searchedData = await ApplySearchFiltersAsync(data);

                // Potom aplikuj advanced filters
                _logger.LogDebug("🎯 Pipeline Step 2: Applying advanced filters");
                var advancedFilteredData = await ApplyAdvancedFiltersAsync(searchedData);

                // Potom aplikuj sort (Multi-Sort ak je aktívne, inak single sort)
                _logger.LogDebug("🎯 Pipeline Step 3: Applying sorting (Multi-Sort: {IsMultiSort})", _multiSortColumns.Any());
                var sortedData = _multiSortColumns.Any() 
                    ? await ApplyMultiSortAsync(advancedFilteredData)
                    : await ApplySortingAsync(advancedFilteredData);

                // Nakoniec aplikuj zebra styling
                _logger.LogDebug("🎯 Pipeline Step 4: Applying zebra styling");
                var styledData = await ApplyZebraRowStylingAsync(sortedData);

                var duration = EndOperation(operationId);
                var finalCount = styledData.Count;
                var dataReduction = originalCount > 0 ? (double)(originalCount - finalCount) / originalCount * 100 : 0;
                var processingRate = duration > 0 ? originalCount / duration : 0;

                _logger.LogInformation("✅ Complete Filter Pipeline COMPLETED - Duration: {Duration}ms, " +
                    "InputRows: {InputRows}, OutputRows: {OutputRows}, DataReduction: {DataReduction:F1}%, " +
                    "ProcessingRate: {ProcessingRate:F0} rows/ms, " +
                    "Pipeline: Search({SearchFilters}) → AdvFilters({AdvFilters}) → Sort({SortColumn}) → Zebra({ZebraEnabled})",
                    duration, originalCount, finalCount, dataReduction, processingRate,
                    _columnSearchFilters.Count, _activeFilterSet?.Filters.Count(f => f.IsEnabled) ?? 0, 
                    _currentSortColumn ?? "None", _zebraRowsEnabled);

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
        /// Má aktívny sort (single alebo multi)
        /// </summary>
        public bool HasActiveSort => _currentSortColumn != null || _multiSortColumns.Any();

        /// <summary>
        /// Má aktívny Multi-Sort
        /// </summary>
        public bool HasActiveMultiSort => _multiSortColumns.Any();

        /// <summary>
        /// Je v Multi-Sort režime
        /// </summary>
        public bool IsMultiSortMode => _isMultiSortMode;

        /// <summary>
        /// Má aktívnu Advanced Search konfiguráciu
        /// </summary>
        public bool HasAdvancedSearchEnabled => _advancedSearchConfig.EnableFuzzySearch || _advancedSearchConfig.EnableRegexSearch || _advancedSearchConfig.EnableSearchHighlighting;

        /// <summary>
        /// Má search históriu
        /// </summary>
        public bool HasSearchHistory => _searchHistory.Any();

        /// <summary>
        /// Počet cached search results
        /// </summary>
        public int SearchCacheSize => _searchResultsCache.Count;

        /// <summary>
        /// Má aktívne advanced filtre
        /// </summary>
        public bool HasActiveAdvancedFilters => _activeFilterSet?.Filters.Any(f => f.IsEnabled) ?? false;

        /// <summary>
        /// Počet aktívnych filter sets
        /// </summary>
        public int FilterSetsCount => _filterSets.Count;

        /// <summary>
        /// Názov aktívneho filter set
        /// </summary>
        public string? ActiveFilterSetName => _activeFilterSet?.Name;

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
                var sortInfo = _multiSortColumns.Any()
                    ? $"Multi[{string.Join(",", _multiSortColumns.Select(c => $"{c.ColumnName}({c.GetSortSymbol()}){c.Priority}"))}]"
                    : _currentSortColumn != null
                        ? $"{_currentSortColumn} ({_columnSortStates[_currentSortColumn]})"
                        : "None";

                var statusInfo = $"Search: {searchCount} filters, Sort: {sortInfo}, " +
                    $"MultiSort: {(_isMultiSortMode ? "ON" : "OFF")}, Zebra: {_zebraRowsEnabled}, " +
                    $"AdvSearch: {(HasAdvancedSearchEnabled ? "ON" : "OFF")}, History: {_searchHistory.Count}, " +
                    $"Cache: {_searchResultsCache.Count}, " +
                    $"AdvFilters: {(HasActiveAdvancedFilters ? $"{_activeFilterSet!.Filters.Count(f => f.IsEnabled)}" : "0")}, " +
                    $"FilterSets: {_filterSets.Count}, " +
                    $"TotalOps: S:{_totalSearchOperations}/So:{_totalSortOperations}/Z:{_totalZebraOperations}/AS:{_totalAdvancedSearchOperations}/AF:{_totalAdvancedFilteringOperations}";

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
                    "SearchOps: {SearchOps}, SortOps: {SortOps}, ZebraOps: {ZebraOps}, AdvSearchOps: {AdvSearchOps}, " +
                    "ActiveSearchFilters: {ActiveSearchFilters}, ActiveSorts: {ActiveSorts}, " +
                    "ActiveMultiSorts: {ActiveMultiSorts}, MultiSortMode: {MultiSortMode}, " +
                    "SearchHistory: {SearchHistory}, SearchCache: {SearchCache}, " +
                    "PendingOperations: {PendingOps}",
                    _totalSearchOperations, _totalSortOperations, _totalZebraOperations, _totalAdvancedSearchOperations,
                    _columnSearchFilters.Count, _columnSortStates.Count, _multiSortColumns.Count, 
                    _isMultiSortMode, _searchHistory.Count, _searchResultsCache.Count, _operationStartTimes.Count);

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

                // Clean up Multi-Sort states
                if (_multiSortColumns.Any())
                {
                    var multiSorts = _multiSortColumns.Select(c => c.GetDisplayText()).ToList();
                    _logger.LogDebug("🧹 Clearing {Count} Multi-Sort states: [{MultiSorts}]", 
                        _multiSortColumns.Count, string.Join(", ", multiSorts));
                }
                _multiSortColumns.Clear();
                _isMultiSortMode = false;

                // Clean up Advanced Search states
                if (_searchHistory.Any())
                {
                    _logger.LogDebug("🧹 Clearing {Count} search history items", _searchHistory.Count);
                }
                _searchHistory.Clear();

                if (_searchResultsCache.Any())
                {
                    _logger.LogDebug("🧹 Clearing {Count} search cache entries", _searchResultsCache.Count);
                }
                _searchResultsCache.Clear();

                // Clean up Advanced Filtering states
                if (_filterSets.Any())
                {
                    var filterSetsInfo = _filterSets.Select(kvp => $"{kvp.Key}({kvp.Value.Filters.Count})").ToList();
                    _logger.LogDebug("🧹 Clearing {Count} filter sets: [{FilterSets}]", 
                        _filterSets.Count, string.Join(", ", filterSetsInfo));
                }
                _filterSets.Clear();
                _activeFilterSet = null;

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