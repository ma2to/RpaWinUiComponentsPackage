// Services/AdvancedSearchService.cs - ✅ NOVÉ: Advanced Search Service s regex, history, highlighting
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Models.Grid;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Models.Search;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Services
{
    /// <summary>
    /// ✅ NOVÉ: Service pre advanced search s regex, history, highlighting
    /// </summary>
    public class AdvancedSearchService : IDisposable
    {
        private readonly ILogger _logger;
        private AdvancedSearchConfiguration _config;
        private readonly List<SearchCriteria> _searchHistory = new();
        private readonly object _lockObject = new();
        private bool _isDisposed = false;
        private Timer? _debounceTimer;
        private string _lastSearchTerm = string.Empty;

        public AdvancedSearchService(AdvancedSearchConfiguration? config = null, ILogger? logger = null)
        {
            _logger = logger ?? NullLogger.Instance;
            _config = config ?? AdvancedSearchConfiguration.Default;
            
            _logger.LogDebug("🚀 AdvancedSearchService initialized with config: {Config}", 
                $"Regex={_config.EnableRegexSearch}, History={_config.EnableSearchHistory}, Highlighting={_config.EnableSearchHighlighting}");
        }

        /// <summary>
        /// Event pre search results
        /// </summary>
        public event EventHandler<SearchResults>? SearchCompleted;

        /// <summary>
        /// Event pre search history changes
        /// </summary>
        public event EventHandler<List<SearchCriteria>>? SearchHistoryChanged;

        /// <summary>
        /// Aktualizuje konfiguráciu advanced search
        /// </summary>
        public void UpdateConfiguration(AdvancedSearchConfiguration config)
        {
            lock (_lockObject)
            {
                if (!config.IsValid())
                    throw new ArgumentException("Invalid AdvancedSearchConfiguration");
                
                _config = config;
                _logger.LogDebug("⚙️ Advanced search configuration updated");
            }
        }

        /// <summary>
        /// Spustí advanced search s debouncing
        /// </summary>
        public async Task SearchAsync(
            string searchTerm,
            List<Dictionary<string, object?>> data,
            List<Models.Grid.ColumnDefinition> columns,
            bool isCaseSensitive = false,
            bool isRegex = false,
            bool isWholeWord = false,
            List<string>? targetColumns = null,
            CancellationToken cancellationToken = default)
        {
            if (_isDisposed) return;

            try
            {
                _logger.LogDebug("🔍 SearchAsync START - Term: '{SearchTerm}', Regex: {IsRegex}, " +
                    "CaseSensitive: {IsCaseSensitive}, WholeWord: {IsWholeWord}",
                    searchTerm, isRegex, isCaseSensitive, isWholeWord);

                // Debouncing
                if (_config.SearchDebounceMs > 0 && searchTerm == _lastSearchTerm)
                {
                    _debounceTimer?.Dispose();
                    _debounceTimer = new Timer(async _ => 
                    {
                        await PerformSearchAsync(searchTerm, data, columns, isCaseSensitive, isRegex, isWholeWord, targetColumns, cancellationToken);
                    }, null, _config.SearchDebounceMs, Timeout.Infinite);
                    return;
                }

                _lastSearchTerm = searchTerm;
                await PerformSearchAsync(searchTerm, data, columns, isCaseSensitive, isRegex, isWholeWord, targetColumns, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ ERROR in SearchAsync - Term: '{SearchTerm}'", searchTerm);
                throw;
            }
        }

        /// <summary>
        /// Vykoná skutočný search
        /// </summary>
        private async Task PerformSearchAsync(
            string searchTerm,
            List<Dictionary<string, object?>> data,
            List<Models.Grid.ColumnDefinition> columns,
            bool isCaseSensitive,
            bool isRegex,
            bool isWholeWord,
            List<string>? targetColumns,
            CancellationToken cancellationToken)
        {
            var stopwatch = Stopwatch.StartNew();
            var criteria = new SearchCriteria
            {
                SearchTerm = searchTerm,
                IsCaseSensitive = isCaseSensitive,
                IsRegex = isRegex,
                IsWholeWord = isWholeWord,
                TargetColumns = targetColumns,
                Timestamp = DateTime.Now
            };

            var results = new SearchResults
            {
                Criteria = criteria
            };

            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                {
                    _logger.LogDebug("⏭️ Search skipped - empty search term");
                    SearchCompleted?.Invoke(this, results);
                    return;
                }

                // Určuj stĺpce na search
                var columnsToSearch = targetColumns?.Any() == true 
                    ? columns.Where(c => targetColumns.Contains(c.Name)).ToList()
                    : columns.Where(c => c.IsVisible || _config.SearchInHiddenColumns).ToList();

                _logger.LogDebug("🔍 Searching in {ColumnCount} columns: {Columns}", 
                    columnsToSearch.Count, string.Join(", ", columnsToSearch.Select(c => c.Name)));

                // Vykonaj search podľa typu
                if (isRegex && _config.EnableRegexSearch)
                {
                    await PerformRegexSearchAsync(searchTerm, data, columnsToSearch, results, cancellationToken);
                }
                else if (_config.EnableFuzzySearch && _config.Strategy == SearchStrategy.Fuzzy)
                {
                    await PerformFuzzySearchAsync(searchTerm, data, columnsToSearch, results, isCaseSensitive, cancellationToken);
                }
                else
                {
                    await PerformStandardSearchAsync(searchTerm, data, columnsToSearch, results, isCaseSensitive, isWholeWord, cancellationToken);
                }

                stopwatch.Stop();
                results.Duration = stopwatch.Elapsed;
                criteria.Duration = stopwatch.Elapsed;
                criteria.ResultCount = results.TotalCount;

                // Pridaj do historie
                if (_config.EnableSearchHistory)
                {
                    AddToHistory(criteria);
                }

                _logger.LogInformation("✅ Search completed - Term: '{SearchTerm}', Results: {ResultCount}, " +
                    "Duration: {Duration}ms", searchTerm, results.TotalCount, results.Duration.TotalMilliseconds);

                SearchCompleted?.Invoke(this, results);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ ERROR performing search - Term: '{SearchTerm}'", searchTerm);
                SearchCompleted?.Invoke(this, results);
            }
        }

        /// <summary>
        /// Vykonaj regex search
        /// </summary>
        private async Task PerformRegexSearchAsync(
            string pattern,
            List<Dictionary<string, object?>> data,
            List<Models.Grid.ColumnDefinition> columns,
            SearchResults results,
            CancellationToken cancellationToken)
        {
            await Task.Run(() =>
            {
                try
                {
                    var regex = new Regex(pattern, RegexOptions.Compiled, TimeSpan.FromMilliseconds(_config.RegexTimeoutMs));
                    
                    for (int rowIndex = 0; rowIndex < data.Count; rowIndex++)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        
                        var row = data[rowIndex];
                        
                        foreach (var column in columns)
                        {
                            if (!row.ContainsKey(column.Name)) continue;
                            
                            var cellValue = row[column.Name]?.ToString() ?? string.Empty;
                            var matches = regex.Matches(cellValue);
                            
                            foreach (Match match in matches)
                            {
                                results.Results.Add(new SearchResultItem
                                {
                                    RowIndex = rowIndex,
                                    ColumnName = column.Name,
                                    CellValue = cellValue,
                                    MatchStartIndex = match.Index,
                                    MatchLength = match.Length,
                                    MatchText = match.Value
                                });

                                if (results.Results.Count >= _config.MaxHighlightResults)
                                    return;
                            }
                        }
                    }
                }
                catch (RegexMatchTimeoutException ex)
                {
                    _logger.LogWarning(ex, "⚠️ Regex search timeout - Pattern: '{Pattern}'", pattern);
                }
                catch (ArgumentException ex)
                {
                    _logger.LogWarning(ex, "⚠️ Invalid regex pattern - Pattern: '{Pattern}'", pattern);
                }
            }, cancellationToken);
        }

        /// <summary>
        /// Vykonaj fuzzy search
        /// </summary>
        private async Task PerformFuzzySearchAsync(
            string searchTerm,
            List<Dictionary<string, object?>> data,
            List<Models.Grid.ColumnDefinition> columns,
            SearchResults results,
            bool isCaseSensitive,
            CancellationToken cancellationToken)
        {
            await Task.Run(() =>
            {
                var comparison = isCaseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;
                
                for (int rowIndex = 0; rowIndex < data.Count; rowIndex++)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    
                    var row = data[rowIndex];
                    
                    foreach (var column in columns)
                    {
                        if (!row.ContainsKey(column.Name)) continue;
                        
                        var cellValue = row[column.Name]?.ToString() ?? string.Empty;
                        var similarity = CalculateLevenshteinSimilarity(searchTerm, cellValue, comparison);
                        
                        if (similarity >= (1.0 - _config.FuzzyTolerance))
                        {
                            results.Results.Add(new SearchResultItem
                            {
                                RowIndex = rowIndex,
                                ColumnName = column.Name,
                                CellValue = cellValue,
                                MatchStartIndex = 0,
                                MatchLength = cellValue.Length,
                                MatchText = cellValue
                            });

                            if (results.Results.Count >= _config.MaxHighlightResults)
                                return;
                        }
                    }
                }
            }, cancellationToken);
        }

        /// <summary>
        /// Vykonaj štandardný search
        /// </summary>
        private async Task PerformStandardSearchAsync(
            string searchTerm,
            List<Dictionary<string, object?>> data,
            List<Models.Grid.ColumnDefinition> columns,
            SearchResults results,
            bool isCaseSensitive,
            bool isWholeWord,
            CancellationToken cancellationToken)
        {
            await Task.Run(() =>
            {
                var comparison = isCaseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;
                
                for (int rowIndex = 0; rowIndex < data.Count; rowIndex++)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    
                    var row = data[rowIndex];
                    
                    foreach (var column in columns)
                    {
                        if (!row.ContainsKey(column.Name)) continue;
                        
                        var cellValue = row[column.Name]?.ToString() ?? string.Empty;
                        var matches = FindMatches(cellValue, searchTerm, comparison, isWholeWord);
                        
                        foreach (var match in matches)
                        {
                            results.Results.Add(new SearchResultItem
                            {
                                RowIndex = rowIndex,
                                ColumnName = column.Name,
                                CellValue = cellValue,
                                MatchStartIndex = match.Index,
                                MatchLength = match.Length,
                                MatchText = match.Text
                            });

                            if (results.Results.Count >= _config.MaxHighlightResults)
                                return;
                        }
                    }
                }
            }, cancellationToken);
        }

        /// <summary>
        /// Nájde matches v texte
        /// </summary>
        private List<(int Index, int Length, string Text)> FindMatches(
            string text, 
            string searchTerm, 
            StringComparison comparison, 
            bool isWholeWord)
        {
            var matches = new List<(int Index, int Length, string Text)>();
            
            if (isWholeWord)
            {
                // Whole word search using regex
                var pattern = $@"\b{Regex.Escape(searchTerm)}\b";
                var options = comparison == StringComparison.OrdinalIgnoreCase ? RegexOptions.IgnoreCase : RegexOptions.None;
                var regex = new Regex(pattern, options);
                
                foreach (Match match in regex.Matches(text))
                {
                    matches.Add((match.Index, match.Length, match.Value));
                }
            }
            else
            {
                // Simple contains search
                int index = 0;
                while ((index = text.IndexOf(searchTerm, index, comparison)) != -1)
                {
                    matches.Add((index, searchTerm.Length, searchTerm));
                    index += searchTerm.Length;
                }
            }
            
            return matches;
        }

        /// <summary>
        /// Vypočíta Levenshtein similarity
        /// </summary>
        private double CalculateLevenshteinSimilarity(string s1, string s2, StringComparison comparison)
        {
            if (comparison == StringComparison.OrdinalIgnoreCase)
            {
                s1 = s1.ToLowerInvariant();
                s2 = s2.ToLowerInvariant();
            }

            var distance = CalculateLevenshteinDistance(s1, s2);
            var maxLength = Math.Max(s1.Length, s2.Length);
            
            return maxLength == 0 ? 1.0 : 1.0 - (double)distance / maxLength;
        }

        /// <summary>
        /// Vypočíta Levenshtein distance
        /// </summary>
        private int CalculateLevenshteinDistance(string s1, string s2)
        {
            var matrix = new int[s1.Length + 1, s2.Length + 1];

            for (int i = 0; i <= s1.Length; i++)
                matrix[i, 0] = i;

            for (int j = 0; j <= s2.Length; j++)
                matrix[0, j] = j;

            for (int i = 1; i <= s1.Length; i++)
            {
                for (int j = 1; j <= s2.Length; j++)
                {
                    var cost = s1[i - 1] == s2[j - 1] ? 0 : 1;
                    matrix[i, j] = Math.Min(
                        Math.Min(matrix[i - 1, j] + 1, matrix[i, j - 1] + 1),
                        matrix[i - 1, j - 1] + cost);
                }
            }

            return matrix[s1.Length, s2.Length];
        }

        /// <summary>
        /// Pridá search criteria do histórie
        /// </summary>
        private void AddToHistory(SearchCriteria criteria)
        {
            lock (_lockObject)
            {
                try
                {
                    // Odstráň duplicitné search terms
                    _searchHistory.RemoveAll(h => h.SearchTerm.Equals(criteria.SearchTerm, StringComparison.OrdinalIgnoreCase));
                    
                    // Pridaj na začiatok
                    _searchHistory.Insert(0, criteria.Clone());
                    
                    // Udržuj maximálny počet
                    while (_searchHistory.Count > _config.MaxSearchHistoryItems)
                    {
                        _searchHistory.RemoveAt(_searchHistory.Count - 1);
                    }

                    _logger.LogTrace("📝 Added to search history: '{SearchTerm}' (total: {HistoryCount})", 
                        criteria.SearchTerm, _searchHistory.Count);

                    SearchHistoryChanged?.Invoke(this, new List<SearchCriteria>(_searchHistory));
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "⚠️ Error adding to search history");
                }
            }
        }

        /// <summary>
        /// Získa search history
        /// </summary>
        public List<SearchCriteria> GetSearchHistory()
        {
            lock (_lockObject)
            {
                return new List<SearchCriteria>(_searchHistory);
            }
        }

        /// <summary>
        /// Vyčistí search history
        /// </summary>
        public void ClearSearchHistory()
        {
            lock (_lockObject)
            {
                _searchHistory.Clear();
                _logger.LogDebug("🗑️ Search history cleared");
                SearchHistoryChanged?.Invoke(this, new List<SearchCriteria>());
            }
        }

        /// <summary>
        /// Vráti konfiguráciu
        /// </summary>
        public AdvancedSearchConfiguration GetConfiguration()
        {
            lock (_lockObject)
            {
                return _config;
            }
        }

        /// <summary>
        /// Dispose resources
        /// </summary>
        public void Dispose()
        {
            if (_isDisposed) return;

            _debounceTimer?.Dispose();
            _searchHistory.Clear();
            _isDisposed = true;

            _logger.LogDebug("🗑️ AdvancedSearchService disposed");
        }
    }

    /// <summary>
    /// ✅ NOVÉ: Search criteria pre advanced search
    /// </summary>
    public class SearchCriteria
    {
        public string SearchTerm { get; set; } = string.Empty;
        public bool IsCaseSensitive { get; set; } = false;
        public bool IsRegex { get; set; } = false;
        public bool IsWholeWord { get; set; } = false;
        public List<string>? TargetColumns { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.Now;
        public int ResultCount { get; set; }
        public TimeSpan Duration { get; set; }

        public bool IsValid => !string.IsNullOrWhiteSpace(SearchTerm);

        public SearchCriteria Clone()
        {
            return new SearchCriteria
            {
                SearchTerm = SearchTerm,
                IsCaseSensitive = IsCaseSensitive,
                IsRegex = IsRegex,
                IsWholeWord = IsWholeWord,
                TargetColumns = TargetColumns?.ToList(),
                Timestamp = Timestamp,
                ResultCount = ResultCount,
                Duration = Duration
            };
        }

        public override string ToString()
        {
            var flags = new List<string>();
            if (IsCaseSensitive) flags.Add("Aa");
            if (IsRegex) flags.Add(".*");
            if (IsWholeWord) flags.Add("\\b");
            
            var flagsStr = flags.Count > 0 ? $" [{string.Join(",", flags)}]" : "";
            return $"{SearchTerm}{flagsStr} ({ResultCount} results)";
        }
    }

    /// <summary>
    /// ✅ NOVÉ: Search result item
    /// </summary>
    public class SearchResultItem
    {
        public int RowIndex { get; set; }
        public string ColumnName { get; set; } = string.Empty;
        public string? CellValue { get; set; }
        public int MatchStartIndex { get; set; }
        public int MatchLength { get; set; }
        public string MatchText { get; set; } = string.Empty;

        public override string ToString()
        {
            return $"Row {RowIndex}, Column {ColumnName}: '{MatchText}'";
        }
    }

    /// <summary>
    /// ✅ NOVÉ: Search results
    /// </summary>
    public class SearchResults
    {
        public SearchCriteria Criteria { get; set; } = new();
        public List<SearchResultItem> Results { get; set; } = new();
        public int TotalCount => Results.Count;
        public int RowCount => Results.Select(r => r.RowIndex).Distinct().Count();
        public TimeSpan Duration { get; set; }
        public bool HasResults => Results.Count > 0;

        public override string ToString()
        {
            return $"Search '{Criteria.SearchTerm}': {TotalCount} matches in {RowCount} rows ({Duration.TotalMilliseconds:F1}ms)";
        }
    }
}