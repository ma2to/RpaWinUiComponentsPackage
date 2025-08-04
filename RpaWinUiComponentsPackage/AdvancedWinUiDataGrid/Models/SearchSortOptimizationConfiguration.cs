// Models/SearchSortOptimizationConfiguration.cs - ✅ NOVÉ: Search/Sort Optimization Configuration
using System;

namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Models
{
    /// <summary>
    /// ✅ NOVÉ: Konfigurácia pre search/sort optimization service
    /// </summary>
    public class SearchSortOptimizationConfiguration
    {
        #region Properties

        /// <summary>
        /// Povoliť diagnostické informácie
        /// </summary>
        public bool EnableDiagnostics { get; set; } = false;

        /// <summary>
        /// Povoliť indexed search
        /// </summary>
        public bool EnableIndexedSearch { get; set; } = true;

        /// <summary>
        /// Povoliť search cache
        /// </summary>
        public bool EnableSearchCache { get; set; } = true;

        /// <summary>
        /// Povoliť parallel sorting
        /// </summary>
        public bool EnableParallelSort { get; set; } = true;

        /// <summary>
        /// Povoliť parallel index building
        /// </summary>
        public bool EnableParallelIndexBuilding { get; set; } = true;

        /// <summary>
        /// Povoliť hash index pre exact matches
        /// </summary>
        public bool EnableHashIndex { get; set; } = true;

        /// <summary>
        /// Povoliť trie index pre prefix searches
        /// </summary>
        public bool EnableTrieIndex { get; set; } = true;

        /// <summary>
        /// Povoliť sorted index pre range queries
        /// </summary>
        public bool EnableSortedIndex { get; set; } = true;

        /// <summary>
        /// Maximálna veľkosť search cache
        /// </summary>
        public int MaxCacheSize { get; set; } = 1000;

        /// <summary>
        /// Cache expiration v ms
        /// </summary>
        public int CacheExpirationMs { get; set; } = 300000; // 5 minutes

        /// <summary>
        /// Interval čistenia cache v ms
        /// </summary>
        public int CacheCleanupIntervalMs { get; set; } = 60000; // 1 minute

        /// <summary>
        /// Threshold pre parallel sorting (počet items)
        /// </summary>
        public int ParallelSortThreshold { get; set; } = 1000;

        /// <summary>
        /// Regex timeout v ms
        /// </summary>
        public int RegexTimeoutMs { get; set; } = 5000; // 5 seconds

        /// <summary>
        /// Search performance warning threshold v ms
        /// </summary>
        public double SearchPerformanceWarningThresholdMs { get; set; } = 100.0;

        /// <summary>
        /// Sort performance warning threshold v ms
        /// </summary>
        public double SortPerformanceWarningThresholdMs { get; set; } = 500.0;

        /// <summary>
        /// Minimum dataset size pre index building
        /// </summary>
        public int MinDatasetSizeForIndexing { get; set; } = 100;

        /// <summary>
        /// Maximum memory usage pre indexes v MB
        /// </summary>
        public double MaxIndexMemoryUsageMB { get; set; } = 100.0;

        #endregion

        #region Static Configurations

        /// <summary>
        /// Základná konfigurácia pre malé datasety
        /// </summary>
        public static SearchSortOptimizationConfiguration Basic => new()
        {
            EnableDiagnostics = false,
            EnableIndexedSearch = false, // Linear search only
            EnableSearchCache = true,
            EnableParallelSort = false, // Sequential sort
            EnableParallelIndexBuilding = false,
            EnableHashIndex = true,
            EnableTrieIndex = false,
            EnableSortedIndex = false,
            MaxCacheSize = 100,
            CacheExpirationMs = 600000, // 10 minutes
            CacheCleanupIntervalMs = 120000, // 2 minutes
            ParallelSortThreshold = 5000, // Higher threshold
            RegexTimeoutMs = 10000, // 10 seconds
            SearchPerformanceWarningThresholdMs = 200.0,
            SortPerformanceWarningThresholdMs = 1000.0,
            MinDatasetSizeForIndexing = 500,
            MaxIndexMemoryUsageMB = 50.0
        };

        /// <summary>
        /// Optimalizovaná konfigurácia pre stredné datasety
        /// </summary>
        public static SearchSortOptimizationConfiguration Optimized => new()
        {
            EnableDiagnostics = false,
            EnableIndexedSearch = true,
            EnableSearchCache = true,
            EnableParallelSort = true,
            EnableParallelIndexBuilding = true,
            EnableHashIndex = true,
            EnableTrieIndex = true,
            EnableSortedIndex = true,
            MaxCacheSize = 1000,
            CacheExpirationMs = 300000, // 5 minutes
            CacheCleanupIntervalMs = 60000, // 1 minute
            ParallelSortThreshold = 1000,
            RegexTimeoutMs = 5000, // 5 seconds
            SearchPerformanceWarningThresholdMs = 100.0,
            SortPerformanceWarningThresholdMs = 500.0,
            MinDatasetSizeForIndexing = 100,
            MaxIndexMemoryUsageMB = 100.0
        };

        /// <summary>
        /// Pokročilá konfigurácia pre veľké datasety
        /// </summary>
        public static SearchSortOptimizationConfiguration Advanced => new()
        {
            EnableDiagnostics = true,
            EnableIndexedSearch = true,
            EnableSearchCache = true,
            EnableParallelSort = true,
            EnableParallelIndexBuilding = true,
            EnableHashIndex = true,
            EnableTrieIndex = true,
            EnableSortedIndex = true,
            MaxCacheSize = 2000,
            CacheExpirationMs = 180000, // 3 minutes
            CacheCleanupIntervalMs = 30000, // 30 seconds
            ParallelSortThreshold = 500,
            RegexTimeoutMs = 3000, // 3 seconds
            SearchPerformanceWarningThresholdMs = 50.0,
            SortPerformanceWarningThresholdMs = 250.0,
            MinDatasetSizeForIndexing = 50,
            MaxIndexMemoryUsageMB = 200.0
        };

        /// <summary>
        /// High-performance konfigurácia pre enterprise datasety
        /// </summary>
        public static SearchSortOptimizationConfiguration HighPerformance => new()
        {
            EnableDiagnostics = true,
            EnableIndexedSearch = true,
            EnableSearchCache = true,
            EnableParallelSort = true,
            EnableParallelIndexBuilding = true,
            EnableHashIndex = true,
            EnableTrieIndex = true,
            EnableSortedIndex = true,
            MaxCacheSize = 5000,
            CacheExpirationMs = 120000, // 2 minutes
            CacheCleanupIntervalMs = 15000, // 15 seconds
            ParallelSortThreshold = 200,
            RegexTimeoutMs = 2000, // 2 seconds
            SearchPerformanceWarningThresholdMs = 25.0,
            SortPerformanceWarningThresholdMs = 100.0,
            MinDatasetSizeForIndexing = 25,
            MaxIndexMemoryUsageMB = 500.0
        };

        /// <summary>
        /// Default konfigurácia
        /// </summary>
        public static SearchSortOptimizationConfiguration Default => Optimized;

        #endregion

        #region Methods

        /// <summary>
        /// Validuje konfiguráciu a nastaví rozumné defaults
        /// </summary>
        public void Validate()
        {
            if (MaxCacheSize < 10) MaxCacheSize = 10;
            if (MaxCacheSize > 10000) MaxCacheSize = 10000;
            
            if (CacheExpirationMs < 30000) CacheExpirationMs = 30000; // 30 seconds min
            if (CacheExpirationMs > 3600000) CacheExpirationMs = 3600000; // 1 hour max
            
            if (CacheCleanupIntervalMs < 10000) CacheCleanupIntervalMs = 10000; // 10 seconds min
            if (CacheCleanupIntervalMs > 300000) CacheCleanupIntervalMs = 300000; // 5 minutes max
            
            if (ParallelSortThreshold < 50) ParallelSortThreshold = 50;
            if (ParallelSortThreshold > 10000) ParallelSortThreshold = 10000;
            
            if (RegexTimeoutMs < 1000) RegexTimeoutMs = 1000; // 1 second min
            if (RegexTimeoutMs > 30000) RegexTimeoutMs = 30000; // 30 seconds max
            
            if (SearchPerformanceWarningThresholdMs < 10.0) SearchPerformanceWarningThresholdMs = 10.0;
            if (SearchPerformanceWarningThresholdMs > 5000.0) SearchPerformanceWarningThresholdMs = 5000.0;
            
            if (SortPerformanceWarningThresholdMs < 50.0) SortPerformanceWarningThresholdMs = 50.0;
            if (SortPerformanceWarningThresholdMs > 10000.0) SortPerformanceWarningThresholdMs = 10000.0;
            
            if (MinDatasetSizeForIndexing < 10) MinDatasetSizeForIndexing = 10;
            if (MinDatasetSizeForIndexing > 1000) MinDatasetSizeForIndexing = 1000;
            
            if (MaxIndexMemoryUsageMB < 10.0) MaxIndexMemoryUsageMB = 10.0;
            if (MaxIndexMemoryUsageMB > 1000.0) MaxIndexMemoryUsageMB = 1000.0;
        }

        /// <summary>
        /// Vytvorí kópiu konfigurácie
        /// </summary>
        public SearchSortOptimizationConfiguration Clone()
        {
            return new SearchSortOptimizationConfiguration
            {
                EnableDiagnostics = EnableDiagnostics,
                EnableIndexedSearch = EnableIndexedSearch,
                EnableSearchCache = EnableSearchCache,
                EnableParallelSort = EnableParallelSort,
                EnableParallelIndexBuilding = EnableParallelIndexBuilding,
                EnableHashIndex = EnableHashIndex,
                EnableTrieIndex = EnableTrieIndex,
                EnableSortedIndex = EnableSortedIndex,
                MaxCacheSize = MaxCacheSize,
                CacheExpirationMs = CacheExpirationMs,
                CacheCleanupIntervalMs = CacheCleanupIntervalMs,
                ParallelSortThreshold = ParallelSortThreshold,
                RegexTimeoutMs = RegexTimeoutMs,
                SearchPerformanceWarningThresholdMs = SearchPerformanceWarningThresholdMs,
                SortPerformanceWarningThresholdMs = SortPerformanceWarningThresholdMs,
                MinDatasetSizeForIndexing = MinDatasetSizeForIndexing,
                MaxIndexMemoryUsageMB = MaxIndexMemoryUsageMB
            };
        }

        /// <summary>
        /// ✅ NOVÉ: Optimalizuje konfiguráciu na základe dataset size
        /// </summary>
        public SearchSortOptimizationConfiguration OptimizeForDatasetSize(int rowCount, int columnCount)
        {
            var optimized = Clone();
            var totalCells = rowCount * columnCount;

            if (totalCells < 10000) // Small dataset
            {
                // Conservative settings for small datasets
                optimized.EnableIndexedSearch = false; // Linear search is faster
                optimized.EnableParallelSort = false;
                optimized.EnableParallelIndexBuilding = false;
                optimized.ParallelSortThreshold = int.MaxValue; // Never parallel
                optimized.MaxCacheSize = Math.Min(100, optimized.MaxCacheSize);
                optimized.EnableTrieIndex = false; // Only hash index
                optimized.EnableSortedIndex = false;
            }
            else if (totalCells < 100000) // Medium dataset
            {
                // Balanced settings
                optimized.EnableIndexedSearch = true;
                optimized.EnableParallelSort = rowCount > 1000;
                optimized.ParallelSortThreshold = Math.Max(500, rowCount / 10);
                optimized.MaxCacheSize = Math.Min(1000, optimized.MaxCacheSize);
            }
            else // Large dataset
            {
                // Aggressive optimization for large datasets
                optimized.EnableIndexedSearch = true;
                optimized.EnableParallelSort = true;
                optimized.EnableParallelIndexBuilding = true;
                optimized.ParallelSortThreshold = Math.Min(200, rowCount / 20);
                optimized.MaxCacheSize = Math.Min(5000, totalCells / 100);
                optimized.SearchPerformanceWarningThresholdMs *= 0.5; // Stricter warnings
                optimized.SortPerformanceWarningThresholdMs *= 0.5;
            }

            optimized.Validate();
            return optimized;
        }

        /// <summary>
        /// ✅ NOVÉ: Optimalizuje konfiguráciu na základe search patterns
        /// </summary>
        public SearchSortOptimizationConfiguration OptimizeForSearchPattern(SearchUsagePattern pattern)
        {
            var optimized = Clone();

            switch (pattern)
            {
                case SearchUsagePattern.ExactMatchHeavy:
                    // Optimize for exact matches
                    optimized.EnableHashIndex = true;
                    optimized.EnableTrieIndex = false;
                    optimized.EnableSortedIndex = false;
                    optimized.EnableSearchCache = true;
                    optimized.MaxCacheSize *= 2; // Larger cache for exact matches
                    break;

                case SearchUsagePattern.PrefixSearchHeavy:
                    // Optimize for prefix searches
                    optimized.EnableHashIndex = true;
                    optimized.EnableTrieIndex = true;
                    optimized.EnableSortedIndex = false;
                    optimized.EnableSearchCache = true;
                    break;

                case SearchUsagePattern.RangeQueryHeavy:
                    // Optimize for range queries
                    optimized.EnableHashIndex = false;
                    optimized.EnableTrieIndex = false;
                    optimized.EnableSortedIndex = true;
                    optimized.EnableSearchCache = false; // Range queries vary too much
                    break;

                case SearchUsagePattern.RegexHeavy:
                    // Optimize for regex searches
                    optimized.EnableIndexedSearch = false; // Regex can't use indexes
                    optimized.EnableSearchCache = true; // Cache is crucial for regex
                    optimized.MaxCacheSize *= 3; // Much larger cache
                    optimized.CacheExpirationMs *= 2; // Longer expiration
                    optimized.RegexTimeoutMs = Math.Min(3000, optimized.RegexTimeoutMs); // Strict timeout
                    break;

                case SearchUsagePattern.SortHeavy:
                    // Optimize for sorting
                    optimized.EnableParallelSort = true;
                    optimized.ParallelSortThreshold /= 2; // More aggressive parallel sorting
                    optimized.EnableSortedIndex = true; // Pre-sorted indexes help
                    optimized.EnableSearchCache = false; // Focus resources on sorting
                    break;
            }

            optimized.Validate();
            return optimized;
        }

        /// <summary>
        /// ✅ NOVÉ: Získa diagnostické informácie o konfigurácii
        /// </summary>
        public string GetDiagnosticInfo()
        {
            var indexTypes = new List<string>();
            if (EnableHashIndex) indexTypes.Add("Hash");
            if (EnableTrieIndex) indexTypes.Add("Trie");
            if (EnableSortedIndex) indexTypes.Add("Sorted");
            
            return $"SearchSortOptimization: " +
                   $"IndexedSearch={EnableIndexedSearch}({string.Join(",", indexTypes)}), " +
                   $"Cache={EnableSearchCache}(Size={MaxCacheSize},Exp={CacheExpirationMs}ms), " +
                   $"ParallelSort={EnableParallelSort}(Threshold={ParallelSortThreshold}), " +
                   $"Warnings: Search={SearchPerformanceWarningThresholdMs}ms/Sort={SortPerformanceWarningThresholdMs}ms, " +
                   $"Features: ParallelIndexing={EnableParallelIndexBuilding}, " +
                   $"RegexTimeout={RegexTimeoutMs}ms, MaxMemory={MaxIndexMemoryUsageMB}MB";
        }

        /// <summary>
        /// ✅ NOVÉ: Vypočíta očakávané performance improvements
        /// </summary>
        public SearchSortPerformanceEstimate EstimatePerformanceImprovement()
        {
            var searchSpeedup = 1.0;
            var sortSpeedup = 1.0;
            var memoryOverhead = 0.0;

            // Search speedup estimation
            if (EnableIndexedSearch)
            {
                if (EnableHashIndex) searchSpeedup *= 10.0; // Hash lookup is O(1) vs O(n)
                if (EnableTrieIndex) searchSpeedup *= 5.0; // Trie is faster for prefix searches
                if (EnableSortedIndex) searchSpeedup *= 3.0; // Binary search is O(log n)
            }
            
            if (EnableSearchCache) searchSpeedup *= 1.5; // Cache gives 50% improvement

            // Sort speedup estimation
            if (EnableParallelSort)
            {
                var coreCount = Environment.ProcessorCount;
                sortSpeedup *= Math.Min(coreCount * 0.7, 4.0); // Up to 4x with diminishing returns
            }

            // Memory overhead estimation
            if (EnableIndexedSearch)
            {
                memoryOverhead += 20.0; // Base index overhead
                if (EnableHashIndex) memoryOverhead += 15.0;
                if (EnableTrieIndex) memoryOverhead += 25.0;
                if (EnableSortedIndex) memoryOverhead += 20.0;
            }
            
            if (EnableSearchCache) memoryOverhead += MaxCacheSize * 0.001; // ~1KB per cache entry

            return new SearchSortPerformanceEstimate
            {
                SearchSpeedupFactor = searchSpeedup,
                SortSpeedupFactor = sortSpeedup,
                MemoryOverheadPercent = memoryOverhead,
                EstimatedSearchImprovementPercent = (searchSpeedup - 1) * 100,
                EstimatedSortImprovementPercent = (sortSpeedup - 1) * 100,
                RecommendedForDatasetSize = GetRecommendedDatasetSize()
            };
        }

        /// <summary>
        /// ✅ NOVÉ: Získa odporúčanú veľkosť datasetu pre túto konfiguráciu
        /// </summary>
        private DatasetSize GetRecommendedDatasetSize()
        {
            if (!EnableIndexedSearch && !EnableParallelSort)
                return DatasetSize.Small; // <1k rows

            if (EnableIndexedSearch && !EnableParallelSort)
                return DatasetSize.Medium; // 1k-10k rows

            if (EnableIndexedSearch && EnableParallelSort && ParallelSortThreshold <= 500)
                return DatasetSize.VeryLarge; // 100k+ rows

            return DatasetSize.Large; // 10k-100k rows
        }

        #endregion
    }

    /// <summary>
    /// ✅ NOVÉ: Search usage patterns
    /// </summary>
    public enum SearchUsagePattern
    {
        ExactMatchHeavy,    // Mostly exact string matches
        PrefixSearchHeavy,  // Mostly prefix/autocomplete searches
        RangeQueryHeavy,    // Mostly numeric/date range queries
        RegexHeavy,         // Mostly regex pattern searches
        SortHeavy           // Mostly sorting operations
    }

    /// <summary>
    /// ✅ NOVÉ: Odhad search/sort performance improvement
    /// </summary>
    public class SearchSortPerformanceEstimate
    {
        public double SearchSpeedupFactor { get; set; }
        public double SortSpeedupFactor { get; set; }
        public double MemoryOverheadPercent { get; set; }
        public double EstimatedSearchImprovementPercent { get; set; }
        public double EstimatedSortImprovementPercent { get; set; }
        public DatasetSize RecommendedForDatasetSize { get; set; }

        public override string ToString()
        {
            return $"Search/Sort Performance Estimate: " +
                   $"SearchSpeedup={SearchSpeedupFactor:F1}x ({EstimatedSearchImprovementPercent:F0}%), " +
                   $"SortSpeedup={SortSpeedupFactor:F1}x ({EstimatedSortImprovementPercent:F0}%), " +
                   $"MemoryOverhead={MemoryOverheadPercent:F1}%, " +
                   $"RecommendedFor={RecommendedForDatasetSize}";
        }
    }
}