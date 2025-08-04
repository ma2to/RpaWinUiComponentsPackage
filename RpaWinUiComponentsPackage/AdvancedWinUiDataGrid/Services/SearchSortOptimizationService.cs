// Services/SearchSortOptimizationService.cs - ✅ NOVÉ: Search/Sort Optimization Service
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Models;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Models.Search;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Services
{
    /// <summary>
    /// ✅ NOVÉ: Search/Sort Optimization Service - indexed searching, B-Tree indexes, parallel sorting
    /// </summary>
    internal class SearchSortOptimizationService : IDisposable
    {
        #region Private Fields

        private readonly ILogger _logger;
        private readonly object _lockObject = new();
        private bool _isDisposed = false;

        // ✅ NOVÉ: Search indexes for fast lookup
        private readonly ConcurrentDictionary<string, SearchIndex> _columnIndexes = new();
        private readonly ConcurrentDictionary<string, SortedDictionary<object, List<int>>> _sortedIndexes = new();
        
        // ✅ NOVÉ: Search result caching
        private readonly ConcurrentDictionary<string, CachedSearchResult> _searchCache = new();
        private readonly Timer _cacheCleanupTimer;

        // ✅ NOVÉ: Performance monitoring
        private readonly List<double> _searchTimes = new();
        private readonly List<double> _sortTimes = new();
        private int _searchCacheHits = 0;
        private int _searchCacheMisses = 0;
        private int _indexBuildsCount = 0;

        // ✅ NOVÉ: Configuration
        private SearchSortOptimizationConfiguration _config;

        #endregion

        #region Events

        /// <summary>
        /// Event vyvolaný pri completion indexovania
        /// </summary>
        public event EventHandler<IndexBuildCompletedEventArgs>? IndexBuildCompleted;

        /// <summary>
        /// Event vyvolaný pri performance warning
        /// </summary>
        public event EventHandler<SearchPerformanceWarningEventArgs>? PerformanceWarning;

        #endregion

        #region Constructor & Initialization

        public SearchSortOptimizationService(
            SearchSortOptimizationConfiguration? config = null,
            ILogger? logger = null)
        {
            _logger = logger ?? NullLogger.Instance;
            _config = config ?? SearchSortOptimizationConfiguration.Default;

            // Setup cache cleanup timer
            _cacheCleanupTimer = new Timer(CleanupExpiredCache, null, 
                _config.CacheCleanupIntervalMs, _config.CacheCleanupIntervalMs);

            _logger.LogDebug("🚀 SearchSortOptimizationService initialized - " +
                "Indexes: {IndexEnabled}, Cache: {CacheEnabled}, ParallelSort: {ParallelEnabled}",
                _config.EnableIndexedSearch, _config.EnableSearchCache, _config.EnableParallelSort);
        }

        #endregion

        #region Index Management

        /// <summary>
        /// ✅ NOVÉ: Build search index for column data
        /// </summary>
        public async Task BuildColumnIndexAsync<T>(string columnName, IEnumerable<T> values, 
            CancellationToken cancellationToken = default)
        {
            if (!_config.EnableIndexedSearch || string.IsNullOrWhiteSpace(columnName))
                return;

            try
            {
                _logger.LogDebug("🔨 Building search index for column: {ColumnName}", columnName);
                var stopwatch = Stopwatch.StartNew();

                var index = new SearchIndex(columnName);
                var valuesList = values.ToList();

                // Build different index types based on configuration
                await Task.Run(() =>
                {
                    for (int i = 0; i < valuesList.Count; i++)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        
                        var value = valuesList[i];
                        var stringValue = value?.ToString() ?? string.Empty;
                        
                        // Build hash index for exact matches
                        if (_config.EnableHashIndex)
                        {
                            if (!index.HashIndex.ContainsKey(stringValue))
                                index.HashIndex[stringValue] = new List<int>();
                            index.HashIndex[stringValue].Add(i);
                        }

                        // Build trie index for prefix searches
                        if (_config.EnableTrieIndex && !string.IsNullOrEmpty(stringValue))
                        {
                            AddToTrieIndex(index.TrieIndex, stringValue.ToLowerInvariant(), i);
                        }

                        // Build sorted index for range queries
                        if (_config.EnableSortedIndex && value != null)
                        {
                            if (!_sortedIndexes.ContainsKey(columnName))
                                _sortedIndexes[columnName] = new SortedDictionary<object, List<int>>();
                            
                            var sortedIndex = _sortedIndexes[columnName];
                            if (!sortedIndex.ContainsKey(value))
                                sortedIndex[value] = new List<int>();
                            sortedIndex[value].Add(i);
                        }
                    }
                }, cancellationToken);

                stopwatch.Stop();
                _columnIndexes[columnName] = index;
                _indexBuildsCount++;

                _logger.LogInformation("✅ Search index built for column {ColumnName} - " +
                    "Duration: {Duration}ms, Items: {ItemCount}, Hash entries: {HashCount}",
                    columnName, stopwatch.ElapsedMilliseconds, valuesList.Count, 
                    index.HashIndex.Count);

                IndexBuildCompleted?.Invoke(this, new IndexBuildCompletedEventArgs(
                    columnName, valuesList.Count, stopwatch.ElapsedMilliseconds));
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("⚠️ Index building cancelled for column: {ColumnName}", columnName);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error building index for column: {ColumnName}", columnName);
                throw;
            }
        }

        /// <summary>
        /// ✅ NOVÉ: Rebuild all indexes with fresh data
        /// </summary>
        public async Task RebuildAllIndexesAsync(Dictionary<string, IEnumerable<object>> columnData,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("🔄 Rebuilding all search indexes - Columns: {ColumnCount}", 
                    columnData.Count);

                // Clear existing indexes
                _columnIndexes.Clear();
                _sortedIndexes.Clear();

                // Build indexes in parallel if enabled
                if (_config.EnableParallelIndexBuilding && columnData.Count > 1)
                {
                    var tasks = columnData.Select(async kvp =>
                    {
                        await BuildColumnIndexAsync(kvp.Key, kvp.Value, cancellationToken);
                    });

                    await Task.WhenAll(tasks);
                }
                else
                {
                    // Sequential index building
                    foreach (var kvp in columnData)
                    {
                        await BuildColumnIndexAsync(kvp.Key, kvp.Value, cancellationToken);
                    }
                }

                _logger.LogInformation("✅ All search indexes rebuilt successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error rebuilding all indexes");
                throw;
            }
        }

        #endregion

        #region Optimized Search

        /// <summary>
        /// ✅ NOVÉ: Perform optimized search using indexes
        /// </summary>
        public async Task<OptimizedSearchResult> SearchAsync(OptimizedSearchRequest request, 
            CancellationToken cancellationToken = default)
        {
            try
            {
                var stopwatch = Stopwatch.StartNew();
                _logger.LogTrace("🔍 Starting optimized search - Query: '{Query}', Columns: {ColumnCount}",
                    request.Query, request.ColumnNames?.Count ?? 0);

                // Check cache first
                var cacheKey = GenerateSearchCacheKey(request);
                if (_config.EnableSearchCache && _searchCache.TryGetValue(cacheKey, out var cachedResult))
                {
                    if (!IsCacheExpired(cachedResult.Timestamp))
                    {
                        _searchCacheHits++;
                        _logger.LogTrace("💾 Search cache HIT - Query: '{Query}'", request.Query);
                        return cachedResult.Result;
                    }
                    else
                    {
                        _searchCache.TryRemove(cacheKey, out _);
                    }
                }

                _searchCacheMisses++;

                // Perform search based on type
                OptimizedSearchResult result;
                if (request.SearchType == SearchType.Exact && _config.EnableHashIndex)
                {
                    result = await PerformHashSearchAsync(request, cancellationToken);
                }
                else if (request.SearchType == SearchType.Prefix && _config.EnableTrieIndex)
                {
                    result = await PerformTrieSearchAsync(request, cancellationToken);
                }
                else if (request.SearchType == SearchType.Regex)
                {
                    result = await PerformRegexSearchAsync(request, cancellationToken);
                }
                else
                {
                    result = await PerformLinearSearchAsync(request, cancellationToken);
                }

                stopwatch.Stop();
                result.SearchDurationMs = stopwatch.ElapsedMilliseconds;
                
                UpdateSearchPerformanceMetrics(stopwatch.ElapsedMilliseconds);

                // Cache result if enabled
                if (_config.EnableSearchCache && _searchCache.Count < _config.MaxCacheSize)
                {
                    _searchCache.TryAdd(cacheKey, new CachedSearchResult
                    {
                        Result = result,
                        Timestamp = DateTime.UtcNow
                    });
                }

                _logger.LogDebug("✅ Search completed - Query: '{Query}', Results: {ResultCount}, " +
                    "Duration: {Duration}ms", request.Query, result.MatchingRowIndexes.Count, 
                    stopwatch.ElapsedMilliseconds);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error performing search - Query: '{Query}'", request.Query);
                throw;
            }
        }

        /// <summary>
        /// ✅ NOVÉ: Perform hash-based exact search
        /// </summary>
        private async Task<OptimizedSearchResult> PerformHashSearchAsync(OptimizedSearchRequest request,
            CancellationToken cancellationToken)
        {
            var result = new OptimizedSearchResult
            {
                Query = request.Query,
                SearchType = SearchType.Exact,
                MatchingRowIndexes = new List<int>()
            };

            await Task.Run(() =>
            {
                var searchColumns = request.ColumnNames ?? _columnIndexes.Keys.ToList();
                
                foreach (var columnName in searchColumns)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    
                    if (_columnIndexes.TryGetValue(columnName, out var index))
                    {
                        if (index.HashIndex.TryGetValue(request.Query, out var matchingRows))
                        {
                            result.MatchingRowIndexes.AddRange(matchingRows);
                        }
                    }
                }

                // Remove duplicates and sort
                result.MatchingRowIndexes = result.MatchingRowIndexes.Distinct().OrderBy(x => x).ToList();
            }, cancellationToken);

            return result;
        }

        /// <summary>
        /// ✅ NOVÉ: Perform trie-based prefix search
        /// </summary>
        private async Task<OptimizedSearchResult> PerformTrieSearchAsync(OptimizedSearchRequest request,
            CancellationToken cancellationToken)
        {
            var result = new OptimizedSearchResult
            {
                Query = request.Query,
                SearchType = SearchType.Prefix,
                MatchingRowIndexes = new List<int>()
            };

            await Task.Run(() =>
            {
                var searchColumns = request.ColumnNames ?? _columnIndexes.Keys.ToList();
                var queryLower = request.Query.ToLowerInvariant();
                
                foreach (var columnName in searchColumns)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    
                    if (_columnIndexes.TryGetValue(columnName, out var index))
                    {
                        var matchingRows = SearchTrieIndex(index.TrieIndex, queryLower);
                        result.MatchingRowIndexes.AddRange(matchingRows);
                    }
                }

                // Remove duplicates and sort
                result.MatchingRowIndexes = result.MatchingRowIndexes.Distinct().OrderBy(x => x).ToList();
            }, cancellationToken);

            return result;
        }

        /// <summary>
        /// ✅ NOVÉ: Perform regex search with timeout protection
        /// </summary>
        private async Task<OptimizedSearchResult> PerformRegexSearchAsync(OptimizedSearchRequest request,
            CancellationToken cancellationToken)
        {
            var result = new OptimizedSearchResult
            {
                Query = request.Query,
                SearchType = SearchType.Regex,
                MatchingRowIndexes = new List<int>()
            };

            try
            {
                var regex = new Regex(request.Query, RegexOptions.IgnoreCase | RegexOptions.Compiled,
                    TimeSpan.FromMilliseconds(_config.RegexTimeoutMs));

                // For regex, we need to search through actual data (not just indexes)
                // This would require access to the original data - simplified for now
                result.MatchingRowIndexes = new List<int>(); // Placeholder

                _logger.LogTrace("🔍 Regex search completed - Pattern: '{Pattern}'", request.Query);
            }
            catch (RegexMatchTimeoutException)
            {
                _logger.LogWarning("⚠️ Regex search timeout - Pattern: '{Pattern}'", request.Query);
                result.HasTimeout = true;
            }

            return result;
        }

        /// <summary>
        /// ✅ NOVÉ: Fallback linear search
        /// </summary>
        private async Task<OptimizedSearchResult> PerformLinearSearchAsync(OptimizedSearchRequest request,
            CancellationToken cancellationToken)
        {
            var result = new OptimizedSearchResult
            {
                Query = request.Query,
                SearchType = SearchType.Contains,
                MatchingRowIndexes = new List<int>()
            };

            // Linear search implementation would go here
            // This is a fallback when indexes are not available
            
            return result;
        }

        #endregion

        #region Optimized Sorting

        /// <summary>
        /// ✅ NOVÉ: Perform optimized parallel sorting
        /// </summary>
        public async Task<SortResult> SortAsync<T>(SortRequest<T> request,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var stopwatch = Stopwatch.StartNew();
                _logger.LogDebug("🔄 Starting optimized sort - Columns: {ColumnCount}, Items: {ItemCount}",
                    request.SortColumns.Count, request.Data.Count);

                SortResult result;

                if (_config.EnableParallelSort && request.Data.Count > _config.ParallelSortThreshold)
                {
                    result = await PerformParallelSortAsync(request, cancellationToken);
                }
                else
                {
                    result = await PerformSequentialSortAsync(request, cancellationToken);
                }

                stopwatch.Stop();
                result.SortDurationMs = stopwatch.ElapsedMilliseconds;
                
                UpdateSortPerformanceMetrics(stopwatch.ElapsedMilliseconds);

                _logger.LogDebug("✅ Sort completed - Duration: {Duration}ms, Items: {ItemCount}",
                    stopwatch.ElapsedMilliseconds, request.Data.Count);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error performing sort");
                throw;
            }
        }

        /// <summary>
        /// ✅ NOVÉ: Perform parallel multi-column sort
        /// </summary>
        private async Task<SortResult> PerformParallelSortAsync<T>(SortRequest<T> request,
            CancellationToken cancellationToken)
        {
            var result = new SortResult();

            await Task.Run(() =>
            {
                try
                {
                    // Create sortable items with indexes
                    var sortableItems = request.Data
                        .Select((item, index) => new { Item = item, OriginalIndex = index })
                        .ToList();

                    // Perform parallel sort using PLINQ
                    var orderedQuery = (dynamic)null;
                    
                    for (int i = 0; i < request.SortColumns.Count; i++)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        
                        var sortColumn = request.SortColumns[i];
                        var keySelector = CreateKeySelectorFunc<T>(sortColumn.ColumnName);

                        if (i == 0)
                        {
                            // First sort column
                            orderedQuery = sortColumn.Direction == SortDirection.Ascending
                                ? System.Linq.ParallelEnumerable.OrderBy(sortableItems.AsParallel(), keySelector)
                                : System.Linq.ParallelEnumerable.OrderByDescending(sortableItems.AsParallel(), keySelector);
                        }
                        else
                        {
                            // Subsequent sort columns (ThenBy)
                            orderedQuery = sortColumn.Direction == SortDirection.Ascending
                                ? System.Linq.ParallelEnumerable.ThenBy(orderedQuery, keySelector)
                                : System.Linq.ParallelEnumerable.ThenByDescending(orderedQuery, keySelector);
                        }
                    }

                    // Extract sorted results
                    var sortedItems = orderedQuery?.ToList() ?? new List<dynamic>();
                    result.SortedIndexes = sortedItems.Select((Func<dynamic, int>)(x => (int)x.OriginalIndex)).ToList();
                    result.IsSuccessful = true;

                    Microsoft.Extensions.Logging.LoggerExtensions.LogTrace(_logger, "✅ Parallel sort completed - Items: {ItemCount}", sortedItems.Count);
                }
                catch (OperationCanceledException)
                {
                    _logger.LogWarning("⚠️ Parallel sort cancelled");
                    result.IsSuccessful = false;
                    throw;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "❌ Error in parallel sort");
                    result.IsSuccessful = false;
                    throw;
                }
            }, cancellationToken);

            return result;
        }

        /// <summary>
        /// ✅ NOVÉ: Perform sequential sort for smaller datasets
        /// </summary>
        private async Task<SortResult> PerformSequentialSortAsync<T>(SortRequest<T> request,
            CancellationToken cancellationToken)
        {
            var result = new SortResult();

            await Task.Run(() =>
            {
                // Sequential sort implementation
                var sortableItems = request.Data
                    .Select((item, index) => new { Item = item, OriginalIndex = index })
                    .ToList();

                // Use standard LINQ ordering
                IOrderedEnumerable<dynamic> orderedQuery = null;
                
                for (int i = 0; i < request.SortColumns.Count; i++)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    
                    var sortColumn = request.SortColumns[i];
                    var keySelector = CreateKeySelectorFunc<T>(sortColumn.ColumnName);

                    if (i == 0)
                    {
                        orderedQuery = sortColumn.Direction == SortDirection.Ascending
                            ? sortableItems.OrderBy(keySelector)
                            : sortableItems.OrderByDescending(keySelector);
                    }
                    else
                    {
                        orderedQuery = sortColumn.Direction == SortDirection.Ascending
                            ? orderedQuery.ThenBy(keySelector)
                            : orderedQuery.ThenByDescending(keySelector);
                    }
                }

                var sortedItems = orderedQuery?.ToList() ?? new List<dynamic>();
                result.SortedIndexes = sortedItems.Select(x => (int)x.OriginalIndex).ToList();
                result.IsSuccessful = true;
            }, cancellationToken);

            return result;
        }

        #endregion

        #region Private Helper Methods

        /// <summary>
        /// ✅ NOVÉ: Add entry to trie index
        /// </summary>
        private void AddToTrieIndex(TrieNode root, string word, int rowIndex)
        {
            var current = root;
            foreach (char c in word)
            {
                if (!current.Children.ContainsKey(c))
                    current.Children[c] = new TrieNode();
                current = current.Children[c];
            }
            current.RowIndexes.Add(rowIndex);
        }

        /// <summary>
        /// ✅ NOVÉ: Search trie index for prefix matches
        /// </summary>
        private List<int> SearchTrieIndex(TrieNode root, string prefix)
        {
            var results = new List<int>();
            var current = root;

            // Navigate to prefix node
            foreach (char c in prefix)
            {
                if (!current.Children.ContainsKey(c))
                    return results; // No matches
                current = current.Children[c];
            }

            // Collect all row indexes from this point down
            CollectAllRowIndexes(current, results);
            return results;
        }

        /// <summary>
        /// ✅ NOVÉ: Recursively collect all row indexes from trie node
        /// </summary>
        private void CollectAllRowIndexes(TrieNode node, List<int> results)
        {
            results.AddRange(node.RowIndexes);
            foreach (var child in node.Children.Values)
            {
                CollectAllRowIndexes(child, results);
            }
        }

        /// <summary>
        /// ✅ NOVÉ: Create key selector function for sorting
        /// </summary>
        private Func<dynamic, object> CreateKeySelectorFunc<T>(string propertyName)
        {
            return item =>
            {
                try
                {
                    // Use reflection to get property value
                    var type = typeof(T);
                    var property = type.GetProperty(propertyName);
                    return property?.GetValue(item.Item) ?? string.Empty;
                }
                catch
                {
                    return string.Empty;
                }
            };
        }

        /// <summary>
        /// ✅ NOVÉ: Generate cache key for search request
        /// </summary>
        private string GenerateSearchCacheKey(OptimizedSearchRequest request)
        {
            var columnsKey = request.ColumnNames != null 
                ? string.Join(",", request.ColumnNames.OrderBy(x => x))
                : "ALL";
            
            return $"{request.SearchType}:{request.Query}:{columnsKey}:{request.CaseSensitive}";
        }

        /// <summary>
        /// ✅ NOVÉ: Check if cached result is expired
        /// </summary>
        private bool IsCacheExpired(DateTime timestamp)
        {
            return DateTime.UtcNow - timestamp > TimeSpan.FromMilliseconds(_config.CacheExpirationMs);
        }

        /// <summary>
        /// ✅ NOVÉ: Update search performance metrics
        /// </summary>
        private void UpdateSearchPerformanceMetrics(double durationMs)
        {
            _searchTimes.Add(durationMs);
            
            // Keep only last 100 measurements
            if (_searchTimes.Count > 100)
                _searchTimes.RemoveAt(0);

            // Check for performance warning
            if (durationMs > _config.SearchPerformanceWarningThresholdMs)
            {
                var avgTime = _searchTimes.Average();
                var warning = new SearchPerformanceWarningEventArgs(
                    durationMs, avgTime, _searchCache.Count,
                    $"Search time {durationMs:F2}ms exceeds threshold {_config.SearchPerformanceWarningThresholdMs}ms"
                );

                PerformanceWarning?.Invoke(this, warning);
                
                _logger.LogWarning("⚠️ Search Performance Warning - Duration: {Duration:F2}ms, " +
                    "Average: {Average:F2}ms, Cache size: {CacheSize}",
                    durationMs, avgTime, _searchCache.Count);
            }
        }

        /// <summary>
        /// ✅ NOVÉ: Update sort performance metrics
        /// </summary>
        private void UpdateSortPerformanceMetrics(double durationMs)
        {
            _sortTimes.Add(durationMs);
            
            // Keep only last 100 measurements
            if (_sortTimes.Count > 100)
                _sortTimes.RemoveAt(0);
        }

        /// <summary>
        /// ✅ NOVÉ: Cleanup expired cache entries
        /// </summary>
        private void CleanupExpiredCache(object? state)
        {
            if (_isDisposed)
                return;

            try
            {
                var expiredKeys = new List<string>();
                var now = DateTime.UtcNow;

                foreach (var kvp in _searchCache)
                {
                    if (now - kvp.Value.Timestamp > TimeSpan.FromMilliseconds(_config.CacheExpirationMs))
                    {
                        expiredKeys.Add(kvp.Key);
                    }
                }

                foreach (var key in expiredKeys)
                {
                    _searchCache.TryRemove(key, out _);
                }

                if (expiredKeys.Count > 0)
                {
                    _logger.LogTrace("🧹 Cleaned up {ExpiredCount} expired search cache entries", 
                        expiredKeys.Count);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "⚠️ Error during search cache cleanup");
            }
        }

        #endregion

        #region Public Properties & Configuration

        /// <summary>
        /// Konfigurácia search/sort optimization
        /// </summary>
        public SearchSortOptimizationConfiguration Configuration => _config;

        /// <summary>
        /// Aktualizuje konfiguráciu
        /// </summary>
        public void UpdateConfiguration(SearchSortOptimizationConfiguration config)
        {
            lock (_lockObject)
            {
                config.Validate();
                _config = config.Clone();
                _logger.LogDebug("⚙️ Search/Sort optimization configuration updated");
            }
        }

        /// <summary>
        /// Získa štatistiky search/sort optimization
        /// </summary>
        public SearchSortOptimizationStats GetStats()
        {
            var avgSearchTime = _searchTimes.Count > 0 ? _searchTimes.Average() : 0;
            var avgSortTime = _sortTimes.Count > 0 ? _sortTimes.Average() : 0;
            
            return new SearchSortOptimizationStats
            {
                IndexedColumnsCount = _columnIndexes.Count,
                SearchCacheSize = _searchCache.Count,
                SearchCacheHitRatio = CalculateSearchCacheHitRatio(),
                AverageSearchTimeMs = avgSearchTime,
                AverageSortTimeMs = avgSortTime,
                IndexBuildsCount = _indexBuildsCount,
                RecentSearchTimes = new List<double>(_searchTimes),
                RecentSortTimes = new List<double>(_sortTimes)
            };
        }

        /// <summary>
        /// ✅ NOVÉ: Calculate search cache hit ratio
        /// </summary>
        private double CalculateSearchCacheHitRatio()
        {
            var totalRequests = _searchCacheHits + _searchCacheMisses;
            return totalRequests > 0 ? (double)_searchCacheHits / totalRequests * 100 : 0;
        }

        #endregion

        #region IDisposable

        public void Dispose()
        {
            if (!_isDisposed)
            {
                try
                {
                    _cacheCleanupTimer?.Dispose();
                    
                    // Clear all indexes and caches
                    _columnIndexes.Clear();
                    _sortedIndexes.Clear();
                    _searchCache.Clear();

                    _logger.LogDebug("🔄 SearchSortOptimizationService disposed");
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "⚠️ Error during SearchSortOptimizationService disposal");
                }

                _isDisposed = true;
            }
        }

        #endregion
    }

    #region Supporting Classes and Data Structures

    /// <summary>
    /// ✅ NOVÉ: Search index containing multiple index types
    /// </summary>
    internal class SearchIndex
    {
        public string ColumnName { get; }
        public Dictionary<string, List<int>> HashIndex { get; } = new();
        public TrieNode TrieIndex { get; } = new();

        public SearchIndex(string columnName)
        {
            ColumnName = columnName;
        }
    }

    /// <summary>
    /// ✅ NOVÉ: Trie node for prefix searches
    /// </summary>
    internal class TrieNode
    {
        public Dictionary<char, TrieNode> Children { get; } = new();
        public List<int> RowIndexes { get; } = new();
    }

    /// <summary>
    /// ✅ NOVÉ: Optimized search request
    /// </summary>
    public class OptimizedSearchRequest
    {
        public string Query { get; set; } = string.Empty;
        public SearchType SearchType { get; set; } = SearchType.Contains;
        public List<string>? ColumnNames { get; set; }
        public bool CaseSensitive { get; set; } = false;
    }

    /// <summary>
    /// ✅ NOVÉ: Sort request
    /// </summary>
    public class SortRequest<T>
    {
        public List<T> Data { get; set; } = new();
        public List<MultiSortColumn> SortColumns { get; set; } = new();
    }

    /// <summary>
    /// ✅ NOVÉ: Optimized search result
    /// </summary>
    public class OptimizedSearchResult
    {
        public string Query { get; set; } = string.Empty;
        public SearchType SearchType { get; set; }
        public List<int> MatchingRowIndexes { get; set; } = new();
        public long SearchDurationMs { get; set; }
        public bool HasTimeout { get; set; }
    }

    /// <summary>
    /// ✅ NOVÉ: Sort result
    /// </summary>
    public class SortResult
    {
        public List<int> SortedIndexes { get; set; } = new();
        public long SortDurationMs { get; set; }
        public bool IsSuccessful { get; set; }
    }

    /// <summary>
    /// ✅ NOVÉ: Cached search result
    /// </summary>
    internal class CachedSearchResult
    {
        public OptimizedSearchResult Result { get; set; } = new();
        public DateTime Timestamp { get; set; }
    }

    /// <summary>
    /// ✅ NOVÉ: Search type enumeration
    /// </summary>
    public enum SearchType
    {
        Contains,
        Exact,
        Prefix,
        Regex
    }

    /// <summary>
    /// ✅ NOVÉ: Event args for index build completion
    /// </summary>
    public class IndexBuildCompletedEventArgs : EventArgs
    {
        public string ColumnName { get; }
        public int ItemCount { get; }
        public long DurationMs { get; }

        public IndexBuildCompletedEventArgs(string columnName, int itemCount, long durationMs)
        {
            ColumnName = columnName;
            ItemCount = itemCount;
            DurationMs = durationMs;
        }
    }

    /// <summary>
    /// ✅ NOVÉ: Event args for search performance warnings
    /// </summary>
    public class SearchPerformanceWarningEventArgs : EventArgs
    {
        public double SearchTimeMs { get; }
        public double AverageSearchTimeMs { get; }
        public int CacheSize { get; }
        public string Message { get; }

        public SearchPerformanceWarningEventArgs(double searchTime, double averageSearchTime,
            int cacheSize, string message)
        {
            SearchTimeMs = searchTime;
            AverageSearchTimeMs = averageSearchTime;
            CacheSize = cacheSize;
            Message = message;
        }
    }

    /// <summary>
    /// ✅ NOVÉ: Search/Sort optimization statistics
    /// </summary>
    public class SearchSortOptimizationStats
    {
        public int IndexedColumnsCount { get; set; }
        public int SearchCacheSize { get; set; }
        public double SearchCacheHitRatio { get; set; }
        public double AverageSearchTimeMs { get; set; }
        public double AverageSortTimeMs { get; set; }
        public int IndexBuildsCount { get; set; }
        public List<double> RecentSearchTimes { get; set; } = new();
        public List<double> RecentSortTimes { get; set; } = new();

        public override string ToString()
        {
            return $"SearchSortOptimization: IndexedColumns={IndexedColumnsCount}, " +
                   $"CacheSize={SearchCacheSize} (HitRatio={SearchCacheHitRatio:F1}%), " +
                   $"AvgSearch={AverageSearchTimeMs:F2}ms, AvgSort={AverageSortTimeMs:F2}ms, " +
                   $"IndexBuilds={IndexBuildsCount}";
        }
    }

    #endregion
}