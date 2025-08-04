// Services/CachingOptimizationService.cs - ✅ NOVÉ: Caching Optimization Service
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.Serialization.Formatters.Binary;

namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Services
{
    /// <summary>
    /// ✅ NOVÉ: Caching Optimization Service - multi-level caching with intelligent eviction
    /// </summary>
    internal class CachingOptimizationService : IDisposable
    {
        #region Private Fields

        private readonly ILogger _logger;
        private readonly object _lockObject = new();
        private bool _isDisposed = false;

        // ✅ NOVÉ: Multi-level cache system
        private readonly ConcurrentDictionary<string, L1CacheEntry> _l1MemoryCache = new();
        private readonly ConcurrentDictionary<string, L2CacheEntry> _l2WeakReferenceCache = new();
        private readonly ConcurrentDictionary<string, L3CacheEntry> _l3DiskCacheIndex = new();

        // ✅ NOVÉ: LRU tracking for eviction
        private readonly LinkedList<string> _l1LruList = new();
        private readonly ConcurrentDictionary<string, LinkedListNode<string>> _l1LruNodes = new();

        // ✅ NOVÉ: Cache maintenance timers
        private readonly Timer _cacheMaintenanceTimer;
        private readonly Timer _diskCacheCleanupTimer;

        // ✅ NOVÉ: Performance monitoring
        private int _l1Hits = 0;
        private int _l1Misses = 0;
        private int _l2Hits = 0;
        private int _l2Misses = 0;
        private int _l3Hits = 0;
        private int _l3Misses = 0;
        private int _evictedEntries = 0;
        private readonly List<double> _cacheOperationTimes = new();

        // ✅ NOVÉ: Configuration
        private CachingOptimizationConfiguration _config;
        private readonly string _diskCacheDirectory;

        #endregion

        #region Events

        /// <summary>
        /// Event vyvolaný pri cache hit
        /// </summary>
        public event EventHandler<CacheHitEventArgs>? CacheHit;

        /// <summary>
        /// Event vyvolaný pri cache miss
        /// </summary>
        public event EventHandler<CacheMissEventArgs>? CacheMiss;

        /// <summary>
        /// Event vyvolaný pri cache eviction
        /// </summary>
        public event EventHandler<CacheEvictionEventArgs>? CacheEviction;

        /// <summary>
        /// Event vyvolaný pri performance warning
        /// </summary>
        public event EventHandler<CachePerformanceWarningEventArgs>? PerformanceWarning;

        #endregion

        #region Constructor & Initialization

        public CachingOptimizationService(
            CachingOptimizationConfiguration? config = null,
            ILogger? logger = null)
        {
            _logger = logger ?? NullLogger.Instance;
            _config = config ?? CachingOptimizationConfiguration.Default;

            // Setup disk cache directory
            _diskCacheDirectory = Path.Combine(Path.GetTempPath(), "AdvancedDataGrid_Cache", 
                Environment.UserName, "L3Cache");

            if (_config.EnableL3DiskCache)
            {
                Directory.CreateDirectory(_diskCacheDirectory);
            }

            // Setup maintenance timers
            _cacheMaintenanceTimer = new Timer(PerformCacheMaintenance, null,
                _config.MaintenanceIntervalMs, _config.MaintenanceIntervalMs);

            _diskCacheCleanupTimer = new Timer(CleanupDiskCache, null,
                _config.DiskCacheCleanupIntervalMs, _config.DiskCacheCleanupIntervalMs);

            _logger.LogDebug("🚀 CachingOptimizationService initialized - " +
                "L1: {L1Enabled}, L2: {L2Enabled}, L3: {L3Enabled}, DiskPath: {DiskPath}",
                _config.EnableL1MemoryCache, _config.EnableL2WeakReferenceCache, 
                _config.EnableL3DiskCache, _diskCacheDirectory);
        }

        #endregion

        #region L1 Memory Cache

        /// <summary>
        /// ✅ NOVÉ: Store item in L1 memory cache
        /// </summary>
        public void StoreInL1Cache<T>(string key, T value, TimeSpan? expiration = null)
        {
            if (!_config.EnableL1MemoryCache || string.IsNullOrEmpty(key) || value == null)
                return;

            try
            {
                var entry = new L1CacheEntry
                {
                    Key = key,
                    Value = value,
                    CreatedAt = DateTime.UtcNow,
                    ExpiresAt = expiration.HasValue ? DateTime.UtcNow.Add(expiration.Value) : null,
                    AccessCount = 0,
                    LastAccessTime = DateTime.UtcNow,
                    EstimatedSizeBytes = EstimateObjectSize(value)
                };

                // Check size limits
                if (entry.EstimatedSizeBytes > _config.MaxL1ItemSizeBytes)
                {
                    _logger.LogTrace("📏 L1 cache item too large - Key: {Key}, Size: {Size} bytes",
                        key, entry.EstimatedSizeBytes);
                    return;
                }

                // Evict if necessary
                EnsureL1CacheCapacity();

                // Store in cache
                _l1MemoryCache[key] = entry;

                // Update LRU tracking
                UpdateL1LruTracking(key);

                _logger.LogTrace("💾 Stored in L1 cache - Key: {Key}, Size: {Size} bytes, " +
                    "Expires: {Expiration}", key, entry.EstimatedSizeBytes, entry.ExpiresAt);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error storing in L1 cache - Key: {Key}", key);
            }
        }

        /// <summary>
        /// ✅ NOVÉ: Retrieve item from L1 memory cache
        /// </summary>
        public T? RetrieveFromL1Cache<T>(string key)
        {
            if (!_config.EnableL1MemoryCache || string.IsNullOrEmpty(key))
                return default(T);

            try
            {
                if (!_l1MemoryCache.TryGetValue(key, out var entry))
                {
                    _l1Misses++;
                    CacheMiss?.Invoke(this, new CacheMissEventArgs(key, CacheLevel.L1));
                    return default(T);
                }

                // Check expiration
                if (entry.ExpiresAt.HasValue && DateTime.UtcNow > entry.ExpiresAt.Value)
                {
                    RemoveFromL1Cache(key);
                    _l1Misses++;
                    CacheMiss?.Invoke(this, new CacheMissEventArgs(key, CacheLevel.L1));
                    return default(T);
                }

                // Update access tracking
                entry.AccessCount++;
                entry.LastAccessTime = DateTime.UtcNow;
                UpdateL1LruTracking(key);

                _l1Hits++;
                CacheHit?.Invoke(this, new CacheHitEventArgs(key, CacheLevel.L1));

                _logger.LogTrace("🎯 L1 cache hit - Key: {Key}, AccessCount: {AccessCount}",
                    key, entry.AccessCount);

                return (T)entry.Value;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error retrieving from L1 cache - Key: {Key}", key);
                _l1Misses++;
                return default(T);
            }
        }

        /// <summary>
        /// ✅ NOVÉ: Update L1 LRU tracking
        /// </summary>
        private void UpdateL1LruTracking(string key)
        {
            lock (_lockObject)
            {
                // Remove existing node if present
                if (_l1LruNodes.TryGetValue(key, out var existingNode))
                {
                    _l1LruList.Remove(existingNode);
                }

                // Add to front (most recently used)
                var newNode = _l1LruList.AddFirst(key);
                _l1LruNodes[key] = newNode;
            }
        }

        /// <summary>
        /// ✅ NOVÉ: Ensure L1 cache capacity
        /// </summary>
        private void EnsureL1CacheCapacity()
        {
            if (_l1MemoryCache.Count < _config.MaxL1CacheItems)
                return;

            lock (_lockObject)
            {
                // Evict LRU items until we have space
                while (_l1MemoryCache.Count >= _config.MaxL1CacheItems && _l1LruList.Count > 0)
                {
                    var lruKey = _l1LruList.Last?.Value;
                    if (lruKey != null)
                    {
                        RemoveFromL1Cache(lruKey);
                        _evictedEntries++;

                        _logger.LogTrace("🗑️ Evicted from L1 cache - Key: {Key}", lruKey);
                        CacheEviction?.Invoke(this, new CacheEvictionEventArgs(lruKey, CacheLevel.L1, "LRU"));
                    }
                }
            }
        }

        /// <summary>
        /// ✅ NOVÉ: Remove item from L1 cache
        /// </summary>
        public void RemoveFromL1Cache(string key)
        {
            if (_l1MemoryCache.TryRemove(key, out _))
            {
                lock (_lockObject)
                {
                    if (_l1LruNodes.TryRemove(key, out var node))
                    {
                        _l1LruList.Remove(node);
                    }
                }
            }
        }

        #endregion

        #region L2 Weak Reference Cache

        /// <summary>
        /// ✅ NOVÉ: Store item in L2 weak reference cache
        /// </summary>
        public void StoreInL2Cache<T>(string key, T value, TimeSpan? expiration = null) where T : class
        {
            if (!_config.EnableL2WeakReferenceCache || string.IsNullOrEmpty(key) || value == null)
                return;

            try
            {
                var entry = new L2CacheEntry
                {
                    Key = key,
                    ValueReference = new WeakReference(value),
                    CreatedAt = DateTime.UtcNow,
                    ExpiresAt = expiration.HasValue ? DateTime.UtcNow.Add(expiration.Value) : null,
                    AccessCount = 0,
                    LastAccessTime = DateTime.UtcNow
                };

                _l2WeakReferenceCache[key] = entry;

                _logger.LogTrace("🔗 Stored in L2 cache - Key: {Key}, Expires: {Expiration}",
                    key, entry.ExpiresAt);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error storing in L2 cache - Key: {Key}", key);
            }
        }

        /// <summary>
        /// ✅ NOVÉ: Retrieve item from L2 weak reference cache
        /// </summary>
        public T? RetrieveFromL2Cache<T>(string key) where T : class
        {
            if (!_config.EnableL2WeakReferenceCache || string.IsNullOrEmpty(key))
                return default(T);

            try
            {
                if (!_l2WeakReferenceCache.TryGetValue(key, out var entry))
                {
                    _l2Misses++;
                    CacheMiss?.Invoke(this, new CacheMissEventArgs(key, CacheLevel.L2));
                    return default(T);
                }

                // Check expiration
                if (entry.ExpiresAt.HasValue && DateTime.UtcNow > entry.ExpiresAt.Value)
                {
                    RemoveFromL2Cache(key);
                    _l2Misses++;
                    CacheMiss?.Invoke(this, new CacheMissEventArgs(key, CacheLevel.L2));
                    return default(T);
                }

                // Check if target is still alive
                var target = entry.ValueReference.Target as T;
                if (target == null)
                {
                    // Object was garbage collected
                    RemoveFromL2Cache(key);
                    _l2Misses++;
                    CacheMiss?.Invoke(this, new CacheMissEventArgs(key, CacheLevel.L2));
                    return default(T);
                }

                // Update access tracking
                entry.AccessCount++;
                entry.LastAccessTime = DateTime.UtcNow;

                _l2Hits++;
                CacheHit?.Invoke(this, new CacheHitEventArgs(key, CacheLevel.L2));

                _logger.LogTrace("🎯 L2 cache hit - Key: {Key}, AccessCount: {AccessCount}",
                    key, entry.AccessCount);

                return target;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error retrieving from L2 cache - Key: {Key}", key);
                _l2Misses++;
                return default(T);
            }
        }

        /// <summary>
        /// ✅ NOVÉ: Remove item from L2 cache
        /// </summary>
        public void RemoveFromL2Cache(string key)
        {
            _l2WeakReferenceCache.TryRemove(key, out _);
        }

        #endregion

        #region L3 Disk Cache

        /// <summary>
        /// ✅ NOVÉ: Store item in L3 disk cache
        /// </summary>
        public async Task StoreInL3CacheAsync<T>(string key, T value, TimeSpan? expiration = null)
        {
            if (!_config.EnableL3DiskCache || string.IsNullOrEmpty(key) || value == null)
                return;

            try
            {
                var entry = new L3CacheEntry
                {
                    Key = key,
                    CreatedAt = DateTime.UtcNow,
                    ExpiresAt = expiration.HasValue ? DateTime.UtcNow.Add(expiration.Value) : null,
                    AccessCount = 0,
                    LastAccessTime = DateTime.UtcNow,
                    FileSize = 0
                };

                var fileName = GenerateSafeCacheFileName(key);
                var filePath = Path.Combine(_diskCacheDirectory, fileName);

                // Serialize and save to disk
                var jsonData = JsonSerializer.Serialize(value, new JsonSerializerOptions
                {
                    WriteIndented = false
                });

                await File.WriteAllTextAsync(filePath, jsonData);
                entry.FileSize = new FileInfo(filePath).Length;

                // Check size limits
                if (entry.FileSize > _config.MaxL3ItemSizeBytes)
                {
                    File.Delete(filePath);
                    _logger.LogTrace("📏 L3 cache item too large - Key: {Key}, Size: {Size} bytes",
                        key, entry.FileSize);
                    return;
                }

                _l3DiskCacheIndex[key] = entry;

                _logger.LogTrace("💽 Stored in L3 cache - Key: {Key}, Size: {Size} bytes, " +
                    "File: {FileName}", key, entry.FileSize, fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error storing in L3 cache - Key: {Key}", key);
            }
        }

        /// <summary>
        /// ✅ NOVÉ: Retrieve item from L3 disk cache
        /// </summary>
        public async Task<T?> RetrieveFromL3CacheAsync<T>(string key)
        {
            if (!_config.EnableL3DiskCache || string.IsNullOrEmpty(key))
                return default(T);

            try
            {
                if (!_l3DiskCacheIndex.TryGetValue(key, out var entry))
                {
                    _l3Misses++;
                    CacheMiss?.Invoke(this, new CacheMissEventArgs(key, CacheLevel.L3));
                    return default(T);
                }

                // Check expiration
                if (entry.ExpiresAt.HasValue && DateTime.UtcNow > entry.ExpiresAt.Value)
                {
                    await RemoveFromL3CacheAsync(key);
                    _l3Misses++;
                    CacheMiss?.Invoke(this, new CacheMissEventArgs(key, CacheLevel.L3));
                    return default(T);
                }

                var fileName = GenerateSafeCacheFileName(key);
                var filePath = Path.Combine(_diskCacheDirectory, fileName);

                if (!File.Exists(filePath))
                {
                    // File was deleted externally
                    _l3DiskCacheIndex.TryRemove(key, out _);
                    _l3Misses++;
                    CacheMiss?.Invoke(this, new CacheMissEventArgs(key, CacheLevel.L3));
                    return default(T);
                }

                // Read and deserialize
                var jsonData = await File.ReadAllTextAsync(filePath);
                var value = JsonSerializer.Deserialize<T>(jsonData);

                // Update access tracking
                entry.AccessCount++;
                entry.LastAccessTime = DateTime.UtcNow;

                _l3Hits++;
                CacheHit?.Invoke(this, new CacheHitEventArgs(key, CacheLevel.L3));

                _logger.LogTrace("🎯 L3 cache hit - Key: {Key}, AccessCount: {AccessCount}",
                    key, entry.AccessCount);

                return value;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error retrieving from L3 cache - Key: {Key}", key);
                _l3Misses++;
                return default(T);
            }
        }

        /// <summary>
        /// ✅ NOVÉ: Remove item from L3 cache
        /// </summary>
        public async Task RemoveFromL3CacheAsync(string key)
        {
            if (_l3DiskCacheIndex.TryRemove(key, out _))
            {
                try
                {
                    var fileName = GenerateSafeCacheFileName(key);
                    var filePath = Path.Combine(_diskCacheDirectory, fileName);

                    if (File.Exists(filePath))
                    {
                        File.Delete(filePath);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "❌ Error deleting L3 cache file - Key: {Key}", key);
                }
            }
        }

        /// <summary>
        /// ✅ NOVÉ: Generate safe cache file name
        /// </summary>
        private string GenerateSafeCacheFileName(string key)
        {
            using var sha256 = SHA256.Create();
            var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(key));
            var hashString = Convert.ToHexString(hashBytes);
            return $"cache_{hashString}.json";
        }

        #endregion

        #region Multi-Level Cache Operations

        /// <summary>
        /// ✅ NOVÉ: Store item across all cache levels
        /// </summary>
        public async Task StoreAsync<T>(string key, T value, TimeSpan? expiration = null)
        {
            var stopwatch = Stopwatch.StartNew();

            try
            {
                var tasks = new List<Task>();

                // Store in L1 (memory)
                if (_config.EnableL1MemoryCache)
                {
                    StoreInL1Cache(key, value, expiration);
                }

                // Store in L2 (weak references) - only for reference types
                if (_config.EnableL2WeakReferenceCache && value != null && value.GetType().IsClass)
                {
                    StoreInL2Cache(key, value as object, expiration);
                }

                // Store in L3 (disk) - async
                if (_config.EnableL3DiskCache)
                {
                    tasks.Add(StoreInL3CacheAsync(key, value, expiration));
                }

                await Task.WhenAll(tasks);

                _logger.LogTrace("📦 Stored across cache levels - Key: {Key}, " +
                    "L1: {L1}, L2: {L2}, L3: {L3}",
                    key, _config.EnableL1MemoryCache, _config.EnableL2WeakReferenceCache, 
                    _config.EnableL3DiskCache);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error storing across cache levels - Key: {Key}", key);
            }
            finally
            {
                stopwatch.Stop();
                UpdateCacheOperationMetrics(stopwatch.ElapsedMilliseconds);
            }
        }

        /// <summary>
        /// ✅ NOVÉ: Retrieve item from cache levels (L1 → L2 → L3)
        /// </summary>
        public async Task<T?> RetrieveAsync<T>(string key) where T : class
        {
            var stopwatch = Stopwatch.StartNew();

            try
            {
                // Try L1 first (fastest)
                if (_config.EnableL1MemoryCache)
                {
                    var l1Result = RetrieveFromL1Cache<T>(key);
                    if (l1Result != null)
                    {
                        _logger.LogTrace("🎯 Cache hit L1 - Key: {Key}", key);
                        return l1Result;
                    }
                }

                // Try L2 (medium speed)
                if (_config.EnableL2WeakReferenceCache && typeof(T).IsClass)
                {
                    var l2Result = RetrieveFromL2Cache<T>(key);
                    if (l2Result != null)
                    {
                        // Promote to L1 if enabled
                        if (_config.EnableL1MemoryCache)
                        {
                            StoreInL1Cache(key, l2Result);
                        }

                        _logger.LogTrace("🎯 Cache hit L2 - Key: {Key} (promoted to L1)", key);
                        return l2Result;
                    }
                }

                // Try L3 (slowest but persistent)
                if (_config.EnableL3DiskCache)
                {
                    var l3Result = await RetrieveFromL3CacheAsync<T>(key);
                    if (l3Result != null)
                    {
                        // Promote to L1 and L2 if enabled
                        if (_config.EnableL1MemoryCache)
                        {
                            StoreInL1Cache(key, l3Result);
                        }

                        if (_config.EnableL2WeakReferenceCache && l3Result != null && l3Result.GetType().IsClass)
                        {
                            StoreInL2Cache(key, l3Result as object);
                        }

                        _logger.LogTrace("🎯 Cache hit L3 - Key: {Key} (promoted to L1+L2)", key);
                        return l3Result;
                    }
                }

                _logger.LogTrace("❌ Cache miss all levels - Key: {Key}", key);
                return default(T);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error retrieving from cache levels - Key: {Key}", key);
                return default(T);
            }
            finally
            {
                stopwatch.Stop();
                UpdateCacheOperationMetrics(stopwatch.ElapsedMilliseconds);
            }
        }

        /// <summary>
        /// ✅ NOVÉ: Remove item from all cache levels
        /// </summary>
        public async Task RemoveAsync(string key)
        {
            try
            {
                var tasks = new List<Task>();

                // Remove from all levels
                RemoveFromL1Cache(key);
                RemoveFromL2Cache(key);
                tasks.Add(RemoveFromL3CacheAsync(key));

                await Task.WhenAll(tasks);

                _logger.LogTrace("🗑️ Removed from all cache levels - Key: {Key}", key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error removing from cache levels - Key: {Key}", key);
            }
        }

        /// <summary>
        /// ✅ NOVÉ: Clear all cache levels
        /// </summary>
        public async Task ClearAllAsync()
        {
            try
            {
                // Clear L1
                _l1MemoryCache.Clear();
                lock (_lockObject)
                {
                    _l1LruList.Clear();
                    _l1LruNodes.Clear();
                }

                // Clear L2
                _l2WeakReferenceCache.Clear();

                // Clear L3
                _l3DiskCacheIndex.Clear();
                if (Directory.Exists(_diskCacheDirectory))
                {
                    var files = Directory.GetFiles(_diskCacheDirectory, "cache_*.json");
                    await Task.Run(() =>
                    {
                        Parallel.ForEach(files, file =>
                        {
                            try
                            {
                                File.Delete(file);
                            }
                            catch (Exception ex)
                            {
                                _logger.LogWarning(ex, "⚠️ Error deleting cache file: {File}", file);
                            }
                        });
                    });
                }

                _logger.LogInformation("🧹 Cleared all cache levels");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error clearing all cache levels");
            }
        }

        #endregion

        #region Cache Maintenance

        /// <summary>
        /// ✅ NOVÉ: Perform cache maintenance
        /// </summary>
        private void PerformCacheMaintenance(object? state)
        {
            if (_isDisposed)
                return;

            try
            {
                var maintenanceCount = 0;

                // Clean expired L1 entries
                var expiredL1Keys = new List<string>();
                foreach (var kvp in _l1MemoryCache)
                {
                    if (kvp.Value.ExpiresAt.HasValue && DateTime.UtcNow > kvp.Value.ExpiresAt.Value)
                    {
                        expiredL1Keys.Add(kvp.Key);
                    }
                }

                foreach (var key in expiredL1Keys)
                {
                    RemoveFromL1Cache(key);
                    maintenanceCount++;
                }

                // Clean expired and dead L2 entries
                var expiredL2Keys = new List<string>();
                foreach (var kvp in _l2WeakReferenceCache)
                {
                    var entry = kvp.Value;
                    if ((entry.ExpiresAt.HasValue && DateTime.UtcNow > entry.ExpiresAt.Value) ||
                        !entry.ValueReference.IsAlive)
                    {
                        expiredL2Keys.Add(kvp.Key);
                    }
                }

                foreach (var key in expiredL2Keys)
                {
                    RemoveFromL2Cache(key);
                    maintenanceCount++;
                }

                // Clean expired L3 entries
                var expiredL3Keys = new List<string>();
                foreach (var kvp in _l3DiskCacheIndex)
                {
                    if (kvp.Value.ExpiresAt.HasValue && DateTime.UtcNow > kvp.Value.ExpiresAt.Value)
                    {
                        expiredL3Keys.Add(kvp.Key);
                    }
                }

                foreach (var key in expiredL3Keys)
                {
                    _ = Task.Run(async () => await RemoveFromL3CacheAsync(key));
                    maintenanceCount++;
                }

                if (maintenanceCount > 0)
                {
                    _logger.LogTrace("🧹 Cache maintenance completed - Cleaned: {Count} entries",
                        maintenanceCount);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error during cache maintenance");
            }
        }

        /// <summary>
        /// ✅ NOVÉ: Cleanup disk cache
        /// </summary>
        private void CleanupDiskCache(object? state)
        {
            if (_isDisposed || !_config.EnableL3DiskCache)
                return;

            try
            {
                if (!Directory.Exists(_diskCacheDirectory))
                    return;

                var files = Directory.GetFiles(_diskCacheDirectory, "cache_*.json");
                var cleanedFiles = 0;

                // Check for orphaned files (not in index)
                var indexedFileNames = _l3DiskCacheIndex.Values
                    .Select(entry => GenerateSafeCacheFileName(entry.Key))
                    .ToHashSet();

                foreach (var file in files)
                {
                    var fileName = Path.GetFileName(file);
                    if (!indexedFileNames.Contains(fileName))
                    {
                        try
                        {
                            File.Delete(file);
                            cleanedFiles++;
                        }
                        catch (Exception ex)
                        {
                            _logger.LogTrace("⚠️ Error deleting orphaned cache file {File}: {Error}",
                                file, ex.Message);
                        }
                    }
                }

                // Check disk space usage
                var totalSize = files.Sum(f => new FileInfo(f).Length);
                if (totalSize > _config.MaxL3TotalSizeBytes)
                {
                    // ✅ IMPLEMENTED: LRU eviction for disk cache
                    _logger.LogWarning("⚠️ L3 disk cache size exceeded: {Size} bytes, implementing LRU eviction", totalSize);
                    await EvictOldestDiskCacheFiles(files, totalSize);
                }

                if (cleanedFiles > 0)
                {
                    _logger.LogTrace("🧹 Disk cache cleanup completed - Cleaned: {Count} files",
                        cleanedFiles);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error during disk cache cleanup");
            }
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// ✅ NOVÉ: Estimate object size in bytes
        /// </summary>
        private long EstimateObjectSize(object obj)
        {
            if (obj == null)
                return 0;

            try
            {
                // Simple heuristic-based size estimation
                return obj switch
                {
                    string str => str.Length * 2, // UTF-16
                    int => 4,
                    long => 8,
                    double => 8,
                    bool => 1,
                    DateTime => 8,
                    _ => 1024 // Default estimate for complex objects
                };
            }
            catch
            {
                return 1024; // Fallback estimate
            }
        }

        /// <summary>
        /// ✅ IMPLEMENTED: LRU eviction for disk cache files
        /// </summary>
        private async Task EvictOldestDiskCacheFiles(string[] files, long currentTotalSize)
        {
            try
            {
                var targetSize = (long)(_config.MaxL3TotalSizeBytes * 0.8); // Target 80% of max size
                var bytesToFree = currentTotalSize - targetSize;
                
                _logger.LogInformation("🗑️ Starting LRU eviction - Current: {Current}MB, Target: {Target}MB, ToFree: {ToFree}MB",
                    currentTotalSize / 1024 / 1024, targetSize / 1024 / 1024, bytesToFree / 1024 / 1024);

                // Sort files by last access time (LRU first)
                var fileInfos = files
                    .Select(f => new FileInfo(f))
                    .Where(fi => fi.Exists)
                    .OrderBy(fi => fi.LastAccessTime)
                    .ToList();

                long freedBytes = 0;
                int deletedCount = 0;

                foreach (var fileInfo in fileInfos)
                {
                    if (freedBytes >= bytesToFree) break;

                    try
                    {
                        var fileSize = fileInfo.Length;
                        var fileName = fileInfo.Name;
                        
                        // Remove from L3 cache tracking first
                        var keyToRemove = _l3DiskCache.FirstOrDefault(kvp => 
                            kvp.Value.Equals(fileInfo.FullName, StringComparison.OrdinalIgnoreCase)).Key;
                        
                        if (!string.IsNullOrEmpty(keyToRemove))
                        {
                            _l3DiskCache.TryRemove(keyToRemove, out _);
                        }

                        // Delete the file
                        fileInfo.Delete();
                        freedBytes += fileSize;
                        deletedCount++;

                        _logger.LogTrace("🗑️ Evicted cache file: {FileName}, Size: {Size}KB", 
                            fileName, fileSize / 1024);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "⚠️ Failed to delete cache file: {File}", fileInfo.FullName);
                    }
                }

                _logger.LogInformation("✅ LRU eviction completed - Deleted: {Count} files, Freed: {FreedMB}MB", 
                    deletedCount, freedBytes / 1024 / 1024);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error during LRU eviction");
            }
        }

        /// <summary>
        /// ✅ NOVÉ: Update cache operation metrics
        /// </summary>
        private void UpdateCacheOperationMetrics(double durationMs)
        {
            _cacheOperationTimes.Add(durationMs);

            // Keep only last 100 measurements
            if (_cacheOperationTimes.Count > 100)
                _cacheOperationTimes.RemoveAt(0);

            // Check for performance warning
            if (durationMs > _config.PerformanceWarningThresholdMs)
            {
                var avgTime = _cacheOperationTimes.Average();
                var stats = GetStats();

                var warning = new CachePerformanceWarningEventArgs(
                    durationMs, avgTime, stats.TotalCacheSize, stats.L1HitRatio,
                    $"Cache operation time {durationMs:F2}ms exceeds threshold {_config.PerformanceWarningThresholdMs}ms"
                );

                PerformanceWarning?.Invoke(this, warning);

                _logger.LogWarning("⚠️ Cache Performance Warning - " +
                    "Operation: {OperationTime:F2}ms, Average: {AvgTime:F2}ms, " +
                    "CacheSize: {CacheSize}, L1HitRatio: {L1HitRatio:F1}%",
                    durationMs, avgTime, stats.TotalCacheSize, stats.L1HitRatio);
            }
        }

        #endregion

        #region Public Properties & Configuration

        /// <summary>
        /// Konfigurácia caching optimization
        /// </summary>
        public CachingOptimizationConfiguration Configuration => _config;

        /// <summary>
        /// Aktualizuje konfiguráciu
        /// </summary>
        public void UpdateConfiguration(CachingOptimizationConfiguration config)
        {
            lock (_lockObject)
            {
                config.Validate();
                _config = config.Clone();
                _logger.LogDebug("⚙️ Caching optimization configuration updated");
            }
        }

        /// <summary>
        /// Získa štatistiky caching optimization
        /// </summary>
        public CachingOptimizationStats GetStats()
        {
            var totalRequests = _l1Hits + _l1Misses + _l2Hits + _l2Misses + _l3Hits + _l3Misses;
            var totalHits = _l1Hits + _l2Hits + _l3Hits;

            var l1HitRatio = (_l1Hits + _l1Misses) > 0 ? (double)_l1Hits / (_l1Hits + _l1Misses) * 100 : 0;
            var l2HitRatio = (_l2Hits + _l2Misses) > 0 ? (double)_l2Hits / (_l2Hits + _l2Misses) * 100 : 0;
            var l3HitRatio = (_l3Hits + _l3Misses) > 0 ? (double)_l3Hits / (_l3Hits + _l3Misses) * 100 : 0;
            var overallHitRatio = totalRequests > 0 ? (double)totalHits / totalRequests * 100 : 0;

            var avgOperationTime = _cacheOperationTimes.Count > 0 ? _cacheOperationTimes.Average() : 0;

            var totalMemorySize = _l1MemoryCache.Values.Sum(e => e.EstimatedSizeBytes);
            var totalDiskSize = _l3DiskCacheIndex.Values.Sum(e => e.FileSize);

            return new CachingOptimizationStats
            {
                L1CacheSize = _l1MemoryCache.Count,
                L2CacheSize = _l2WeakReferenceCache.Count,
                L3CacheSize = _l3DiskCacheIndex.Count,
                TotalCacheSize = _l1MemoryCache.Count + _l2WeakReferenceCache.Count + _l3DiskCacheIndex.Count,
                L1Hits = _l1Hits,
                L1Misses = _l1Misses,
                L2Hits = _l2Hits,
                L2Misses = _l2Misses,
                L3Hits = _l3Hits,
                L3Misses = _l3Misses,
                L1HitRatio = l1HitRatio,
                L2HitRatio = l2HitRatio,
                L3HitRatio = l3HitRatio,
                OverallHitRatio = overallHitRatio,
                EvictedEntries = _evictedEntries,
                TotalMemorySizeBytes = totalMemorySize,
                TotalDiskSizeBytes = totalDiskSize,
                AverageOperationTimeMs = avgOperationTime,
                RecentOperationTimes = new List<double>(_cacheOperationTimes)
            };
        }

        #endregion

        #region IDisposable

        public void Dispose()
        {
            if (!_isDisposed)
            {
                try
                {
                    _cacheMaintenanceTimer?.Dispose();
                    _diskCacheCleanupTimer?.Dispose();

                    // Clear all caches
                    _ = Task.Run(async () => await ClearAllAsync());

                    _logger.LogDebug("🔄 CachingOptimizationService disposed");
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "⚠️ Error during CachingOptimizationService disposal");
                }

                _isDisposed = true;
            }
        }

        #endregion
    }

    #region Supporting Classes and Enums

    /// <summary>
    /// ✅ NOVÉ: L1 cache entry
    /// </summary>
    internal class L1CacheEntry
    {
        public string Key { get; set; } = string.Empty;
        public object Value { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public int AccessCount { get; set; }
        public DateTime LastAccessTime { get; set; }
        public long EstimatedSizeBytes { get; set; }
    }

    /// <summary>
    /// ✅ NOVÉ: L2 cache entry
    /// </summary>
    internal class L2CacheEntry
    {
        public string Key { get; set; } = string.Empty;
        public WeakReference ValueReference { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public int AccessCount { get; set; }
        public DateTime LastAccessTime { get; set; }
    }

    /// <summary>
    /// ✅ NOVÉ: L3 cache entry
    /// </summary>
    internal class L3CacheEntry
    {
        public string Key { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public int AccessCount { get; set; }
        public DateTime LastAccessTime { get; set; }
        public long FileSize { get; set; }
    }

    /// <summary>
    /// ✅ NOVÉ: Cache levels
    /// </summary>
    public enum CacheLevel
    {
        L1 = 1,
        L2 = 2,
        L3 = 3
    }

    /// <summary>
    /// ✅ NOVÉ: Cache hit event args
    /// </summary>
    public class CacheHitEventArgs : EventArgs
    {
        public string Key { get; }
        public CacheLevel Level { get; }
        public DateTime Timestamp { get; }

        public CacheHitEventArgs(string key, CacheLevel level)
        {
            Key = key;
            Level = level;
            Timestamp = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// ✅ NOVÉ: Cache miss event args
    /// </summary>
    public class CacheMissEventArgs : EventArgs
    {
        public string Key { get; }
        public CacheLevel Level { get; }
        public DateTime Timestamp { get; }

        public CacheMissEventArgs(string key, CacheLevel level)
        {
            Key = key;
            Level = level;
            Timestamp = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// ✅ NOVÉ: Cache eviction event args
    /// </summary>
    public class CacheEvictionEventArgs : EventArgs
    {
        public string Key { get; }
        public CacheLevel Level { get; }
        public string Reason { get; }
        public DateTime Timestamp { get; }

        public CacheEvictionEventArgs(string key, CacheLevel level, string reason)
        {
            Key = key;
            Level = level;
            Reason = reason;
            Timestamp = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// ✅ NOVÉ: Cache performance warning event args
    /// </summary>
    public class CachePerformanceWarningEventArgs : EventArgs
    {
        public double OperationTimeMs { get; }
        public double AverageOperationTimeMs { get; }
        public int TotalCacheSize { get; }
        public double L1HitRatio { get; }
        public string Message { get; }

        public CachePerformanceWarningEventArgs(double operationTime, double averageOperationTime,
            int totalCacheSize, double l1HitRatio, string message)
        {
            OperationTimeMs = operationTime;
            AverageOperationTimeMs = averageOperationTime;
            TotalCacheSize = totalCacheSize;
            L1HitRatio = l1HitRatio;
            Message = message;
        }
    }

    /// <summary>
    /// ✅ NOVÉ: Caching optimization statistics
    /// </summary>
    public class CachingOptimizationStats
    {
        public int L1CacheSize { get; set; }
        public int L2CacheSize { get; set; }
        public int L3CacheSize { get; set; }
        public int TotalCacheSize { get; set; }
        public int L1Hits { get; set; }
        public int L1Misses { get; set; }
        public int L2Hits { get; set; }
        public int L2Misses { get; set; }
        public int L3Hits { get; set; }
        public int L3Misses { get; set; }
        public double L1HitRatio { get; set; }
        public double L2HitRatio { get; set; }
        public double L3HitRatio { get; set; }
        public double OverallHitRatio { get; set; }
        public int EvictedEntries { get; set; }
        public long TotalMemorySizeBytes { get; set; }
        public long TotalDiskSizeBytes { get; set; }
        public double AverageOperationTimeMs { get; set; }
        public List<double> RecentOperationTimes { get; set; } = new();

        public override string ToString()
        {
            return $"CachingOptimization: L1={L1CacheSize} (HR:{L1HitRatio:F1}%), " +
                   $"L2={L2CacheSize} (HR:{L2HitRatio:F1}%), L3={L3CacheSize} (HR:{L3HitRatio:F1}%), " +
                   $"Overall HR={OverallHitRatio:F1}%, MemSize={TotalMemorySizeBytes} bytes, " +
                   $"DiskSize={TotalDiskSizeBytes} bytes, AvgTime={AverageOperationTimeMs:F2}ms";
        }
    }

    #endregion
}