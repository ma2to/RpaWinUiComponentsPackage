// Services/NetworkIOOptimizationService.cs - ✅ NOVÉ: Network/IO Optimization Service
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Services
{
    /// <summary>
    /// ✅ NOVÉ: Network/IO Optimization Service - streaming, compression, progressive loading
    /// </summary>
    internal class NetworkIOOptimizationService : IDisposable
    {
        #region Private Fields

        private readonly ILogger _logger;
        private readonly object _lockObject = new();
        private bool _isDisposed = false;

        // ✅ NOVÉ: Configuration
        private NetworkIOOptimizationConfiguration _config;
        private readonly string _serviceInstanceId = Guid.NewGuid().ToString("N")[..8];

        // ✅ NOVÉ: Network operations tracking
        private readonly ConcurrentDictionary<string, NetworkOperationInfo> _activeOperations = new();
        private readonly SemaphoreSlim _concurrentStreamsSemaphore;
        private readonly HttpClient _httpClient;

        // ✅ NOVÉ: Performance monitoring
        private long _totalOperations = 0;
        private long _totalBytesTransferred = 0;
        private long _totalCompressionSavings = 0;
        private readonly Stopwatch _performanceStopwatch = new();

        // ✅ NOVÉ: Metadata cache
        private readonly ConcurrentDictionary<string, FileMetadata> _metadataCache = new();
        private readonly Timer _cacheCleanupTimer;

        // ✅ NOVÉ: Progressive loading
        private readonly ConcurrentDictionary<string, ProgressiveLoadingState> _progressiveStates = new();

        #endregion

        #region Constructor

        public NetworkIOOptimizationService(ILogger? logger = null, NetworkIOOptimizationConfiguration? config = null)
        {
            _logger = logger ?? NullLogger.Instance;
            _config = config ?? NetworkIOOptimizationConfiguration.Default;
            _config.Validate();

            _concurrentStreamsSemaphore = new SemaphoreSlim(_config.MaxConcurrentStreams, _config.MaxConcurrentStreams);
            
            _httpClient = new HttpClient();
            _httpClient.Timeout = TimeSpan.FromMilliseconds(_config.NetworkTimeoutMs);

            _cacheCleanupTimer = new Timer(CleanupMetadataCache, null, TimeSpan.FromMinutes(10), TimeSpan.FromMinutes(10));

            _logger.LogInformation("🌐 NetworkIOOptimizationService initialized - InstanceId: {InstanceId}, Config: {Config}",
                _serviceInstanceId, _config.ToString());
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Service je inicializovaný
        /// </summary>
        public bool IsInitialized => !_isDisposed;

        /// <summary>
        /// Počet aktívnych operácií
        /// </summary>
        public int ActiveOperationsCount => _activeOperations.Count;

        /// <summary>
        /// Dostupné stream slots
        /// </summary>
        public int AvailableStreamSlots => _concurrentStreamsSemaphore.CurrentCount;

        #endregion

        #region Streaming Operations

        /// <summary>
        /// ✅ NOVÉ: Stream file with optimization
        /// </summary>
        public async Task<Stream> StreamFileAsync(string filePath, CancellationToken cancellationToken = default)
        {
            if (!_config.IsEnabled || !_config.EnableStreaming)
            {
                return File.OpenRead(filePath);
            }

            var operationId = Guid.NewGuid().ToString("N")[..8];
            var stopwatch = Stopwatch.StartNew();

            try
            {
                await _concurrentStreamsSemaphore.WaitAsync(cancellationToken);

                var operation = new NetworkOperationInfo
                {
                    OperationId = operationId,
                    Type = NetworkOperationType.Stream,
                    FilePath = filePath,
                    Status = NetworkOperationStatus.InProgress,
                    StartTime = DateTime.UtcNow
                };

                _activeOperations[operationId] = operation;

                _logger.LogDebug("🌊 Starting file stream - Operation: {OperationId}, File: {FilePath}",
                    operationId, filePath);

                var fileInfo = new FileInfo(filePath);
                if (!fileInfo.Exists)
                {
                    throw new FileNotFoundException($"File not found: {filePath}");
                }

                // Create optimized stream
                var baseStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, _config.StreamingBufferSize);
                Stream resultStream = baseStream;

                // Apply compression if beneficial
                if (_config.EnableCompression && fileInfo.Length >= _config.MinCompressionFileSize)
                {
                    _logger.LogTrace("🗜️ Applying compression to stream - File: {FilePath}, Size: {Size}",
                        filePath, fileInfo.Length);
                    
                    var compressionStream = new MemoryStream();
                    using (var gzipStream = new GZipStream(compressionStream, _config.CompressionLevel, true))
                    {
                        await baseStream.CopyToAsync(gzipStream, _config.StreamingBufferSize, cancellationToken);
                    }
                    
                    baseStream.Dispose();
                    compressionStream.Position = 0;
                    resultStream = compressionStream;

                    var savings = fileInfo.Length - compressionStream.Length;
                    Interlocked.Add(ref _totalCompressionSavings, savings);
                }

                // Update operation
                operation.Status = NetworkOperationStatus.Completed;
                operation.BytesTransferred = fileInfo.Length;
                Interlocked.Increment(ref _totalOperations);
                Interlocked.Add(ref _totalBytesTransferred, fileInfo.Length);

                _logger.LogDebug("✅ File stream completed - Operation: {OperationId}, Duration: {Duration}ms",
                    operationId, stopwatch.ElapsedMilliseconds);

                return new OptimizedStream(resultStream, () =>
                {
                    _activeOperations.TryRemove(operationId, out _);
                    _concurrentStreamsSemaphore.Release();
                });
            }
            catch (Exception ex)
            {
                _activeOperations.TryRemove(operationId, out _);
                _concurrentStreamsSemaphore.Release();
                
                _logger.LogError(ex, "❌ Error streaming file - Operation: {OperationId}, File: {FilePath}",
                    operationId, filePath);
                throw;
            }
        }

        /// <summary>
        /// ✅ NOVÉ: Stream from URL with retry mechanism
        /// </summary>
        public async Task<Stream> StreamFromUrlAsync(string url, CancellationToken cancellationToken = default)
        {
            if (!_config.IsEnabled || !_config.EnableStreaming)
            {
                return await _httpClient.GetStreamAsync(url);
            }

            var operationId = Guid.NewGuid().ToString("N")[..8];
            var attempts = 0;

            while (attempts <= _config.MaxRetryAttempts)
            {
                try
                {
                    await _concurrentStreamsSemaphore.WaitAsync(cancellationToken);

                    var operation = new NetworkOperationInfo
                    {
                        OperationId = operationId,
                        Type = NetworkOperationType.Download,
                        Url = url,
                        Status = attempts > 0 ? NetworkOperationStatus.Retrying : NetworkOperationStatus.InProgress,
                        StartTime = DateTime.UtcNow,
                        RetryAttempt = attempts
                    };

                    _activeOperations[operationId] = operation;

                    _logger.LogDebug("🌐 Starting URL stream - Operation: {OperationId}, URL: {Url}, Attempt: {Attempt}",
                        operationId, url, attempts + 1);

                    var response = await _httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
                    response.EnsureSuccessStatusCode();

                    var stream = await response.Content.ReadAsStreamAsync();
                    var contentLength = response.Content.Headers.ContentLength ?? 0;

                    // Update operation
                    operation.Status = NetworkOperationStatus.Completed;
                    operation.BytesTransferred = contentLength;
                    Interlocked.Increment(ref _totalOperations);
                    Interlocked.Add(ref _totalBytesTransferred, contentLength);

                    _logger.LogDebug("✅ URL stream completed - Operation: {OperationId}",
                        operationId);

                    return new OptimizedStream(stream, () =>
                    {
                        _activeOperations.TryRemove(operationId, out _);
                        _concurrentStreamsSemaphore.Release();
                        response.Dispose();
                    });
                }
                catch (Exception ex) when (attempts < _config.MaxRetryAttempts && _config.EnableRetry)
                {
                    attempts++;
                    _concurrentStreamsSemaphore.Release();
                    
                    _logger.LogWarning("⚠️ URL stream attempt {Attempt} failed, retrying - Operation: {OperationId}, Error: {Error}",
                        attempts, operationId, ex.Message);

                    if (attempts <= _config.MaxRetryAttempts)
                    {
                        await Task.Delay(_config.RetryDelayMs * attempts, cancellationToken);
                    }
                }
                catch (Exception ex)
                {
                    _activeOperations.TryRemove(operationId, out _);
                    _concurrentStreamsSemaphore.Release();
                    
                    _logger.LogError(ex, "❌ Error streaming from URL - Operation: {OperationId}, URL: {Url}",
                        operationId, url);
                    throw;
                }
            }

            throw new InvalidOperationException($"Failed to stream from URL after {_config.MaxRetryAttempts + 1} attempts: {url}");
        }

        #endregion

        #region Progressive Loading

        /// <summary>
        /// ✅ NOVÉ: Start progressive file loading
        /// </summary>
        public async Task<ProgressiveLoadingResult> StartProgressiveLoadingAsync(string filePath, 
            IProgress<ProgressiveLoadingProgress>? progress = null, CancellationToken cancellationToken = default)
        {
            if (!_config.IsEnabled || !_config.EnableProgressiveLoading)
            {
                // Fallback to regular file reading
                var data = await File.ReadAllBytesAsync(filePath, cancellationToken);
                return new ProgressiveLoadingResult { Data = data, IsComplete = true };
            }

            var operationId = Guid.NewGuid().ToString("N")[..8];
            var fileInfo = new FileInfo(filePath);

            if (!fileInfo.Exists)
            {
                throw new FileNotFoundException($"File not found: {filePath}");
            }

            // Check if file is suitable for progressive loading
            if (fileInfo.Length <= _config.ProgressiveChunkSize)
            {
                var data = await File.ReadAllBytesAsync(filePath, cancellationToken);
                return new ProgressiveLoadingResult { Data = data, IsComplete = true };
            }

            var state = new ProgressiveLoadingState
            {
                OperationId = operationId,
                FilePath = filePath,
                TotalSize = fileInfo.Length,
                ChunkSize = _config.ProgressiveChunkSize,
                LoadedBytes = 0,
                StartTime = DateTime.UtcNow
            };

            _progressiveStates[operationId] = state;

            _logger.LogDebug("📁 Starting progressive loading - Operation: {OperationId}, File: {FilePath}, Size: {Size}",
                operationId, filePath, fileInfo.Length);

            try
            {
                using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, _config.StreamingBufferSize);
                var buffer = new byte[_config.ProgressiveChunkSize];
                var totalData = new List<byte>();

                while (state.LoadedBytes < state.TotalSize && !cancellationToken.IsCancellationRequested)
                {
                    var remainingBytes = (int)Math.Min(_config.ProgressiveChunkSize, state.TotalSize - state.LoadedBytes);
                    var bytesRead = await fileStream.ReadAsync(buffer, 0, remainingBytes, cancellationToken);

                    if (bytesRead == 0) break;

                    totalData.AddRange(buffer.Take(bytesRead));
                    state.LoadedBytes += bytesRead;

                    // Report progress
                    var progressPercent = (double)state.LoadedBytes / state.TotalSize * 100;
                    progress?.Report(new ProgressiveLoadingProgress
                    {
                        OperationId = operationId,
                        LoadedBytes = state.LoadedBytes,
                        TotalBytes = state.TotalSize,
                        ProgressPercent = progressPercent,
                        IsComplete = state.LoadedBytes >= state.TotalSize
                    });

                    _logger.LogTrace("📊 Progressive loading progress - Operation: {OperationId}, Progress: {Progress:F1}%",
                        operationId, progressPercent);

                    // Small delay to allow UI updates
                    if (state.LoadedBytes < state.TotalSize)
                    {
                        await Task.Delay(10, cancellationToken);
                    }
                }

                var result = new ProgressiveLoadingResult
                {
                    Data = totalData.ToArray(),
                    IsComplete = state.LoadedBytes >= state.TotalSize,
                    LoadedBytes = state.LoadedBytes,
                    TotalBytes = state.TotalSize
                };

                Interlocked.Increment(ref _totalOperations);
                Interlocked.Add(ref _totalBytesTransferred, state.LoadedBytes);

                _logger.LogDebug("✅ Progressive loading completed - Operation: {OperationId}, Loaded: {LoadedBytes}/{TotalBytes}",
                    operationId, state.LoadedBytes, state.TotalSize);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error in progressive loading - Operation: {OperationId}, File: {FilePath}",
                    operationId, filePath);
                throw;
            }
            finally
            {
                _progressiveStates.TryRemove(operationId, out _);
            }
        }

        #endregion

        #region File Operations

        /// <summary>
        /// ✅ NOVÉ: Async file write with compression
        /// </summary>
        public async Task WriteFileAsync(string filePath, byte[] data, bool compress = false, CancellationToken cancellationToken = default)
        {
            if (!_config.IsEnabled)
            {
                await File.WriteAllBytesAsync(filePath, data, cancellationToken);
                return;
            }

            var operationId = Guid.NewGuid().ToString("N")[..8];
            var stopwatch = Stopwatch.StartNew();

            try
            {
                _logger.LogDebug("💾 Starting file write - Operation: {OperationId}, File: {FilePath}, Size: {Size}, Compress: {Compress}",
                    operationId, filePath, data.Length, compress);

                byte[] dataToWrite = data;

                // Apply compression if requested and beneficial
                if (compress && _config.EnableCompression && data.Length >= _config.MinCompressionFileSize)
                {
                    using var memoryStream = new MemoryStream();
                    using (var gzipStream = new GZipStream(memoryStream, _config.CompressionLevel))
                    {
                        await gzipStream.WriteAsync(data, 0, data.Length, cancellationToken);
                    }
                    
                    dataToWrite = memoryStream.ToArray();
                    var savings = data.Length - dataToWrite.Length;
                    Interlocked.Add(ref _totalCompressionSavings, savings);

                    _logger.LogTrace("🗜️ File compressed - Original: {OriginalSize}, Compressed: {CompressedSize}, Savings: {Savings}",
                        data.Length, dataToWrite.Length, savings);
                }

                // Write file with timeout
                using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                cts.CancelAfter(_config.FileTimeoutMs);

                await File.WriteAllBytesAsync(filePath, dataToWrite, cts.Token);

                Interlocked.Increment(ref _totalOperations);
                Interlocked.Add(ref _totalBytesTransferred, data.Length);

                _logger.LogDebug("✅ File write completed - Operation: {OperationId}, Duration: {Duration}ms",
                    operationId, stopwatch.ElapsedMilliseconds);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error writing file - Operation: {OperationId}, File: {FilePath}",
                    operationId, filePath);
                throw;
            }
        }

        /// <summary>
        /// ✅ NOVÉ: Get file metadata with caching
        /// </summary>
        public async Task<FileMetadata> GetFileMetadataAsync(string filePath, CancellationToken cancellationToken = default)
        {
            // Check cache first
            if (_metadataCache.TryGetValue(filePath, out var cachedMetadata) && 
                DateTime.UtcNow - cachedMetadata.CacheTime < TimeSpan.FromMinutes(5))
            {
                return cachedMetadata;
            }

            try
            {
                var fileInfo = new FileInfo(filePath);
                if (!fileInfo.Exists)
                {
                    throw new FileNotFoundException($"File not found: {filePath}");
                }

                var metadata = new FileMetadata
                {
                    FilePath = filePath,
                    Size = fileInfo.Length,
                    CreatedTime = fileInfo.CreationTimeUtc,
                    LastModifiedTime = fileInfo.LastWriteTimeUtc,
                    Extension = fileInfo.Extension,
                    CacheTime = DateTime.UtcNow
                };

                // Add to cache if space available
                if (_metadataCache.Count < _config.MetadataCacheSize)
                {
                    _metadataCache[filePath] = metadata;
                }

                return metadata;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error getting file metadata - File: {FilePath}", filePath);
                throw;
            }
        }

        #endregion

        #region Configuration & Management

        /// <summary>
        /// ✅ NOVÉ: Update configuration
        /// </summary>
        public void UpdateConfiguration(NetworkIOOptimizationConfiguration config)
        {
            lock (_lockObject)
            {
                config.Validate();
                _config = config.Clone();
                
                // Update HTTP client timeout
                _httpClient.Timeout = TimeSpan.FromMilliseconds(_config.NetworkTimeoutMs);

                _logger.LogInformation("⚙️ NetworkIOOptimizationService configuration updated - Config: {Config}",
                    _config.ToString());
            }
        }

        /// <summary>
        /// ✅ NOVÉ: Get performance metrics
        /// </summary>
        public NetworkIOPerformanceMetrics GetPerformanceMetrics()
        {
            return new NetworkIOPerformanceMetrics
            {
                TotalOperations = _totalOperations,
                TotalBytesTransferred = _totalBytesTransferred,
                TotalCompressionSavings = _totalCompressionSavings,
                ActiveOperationsCount = _activeOperations.Count,
                AvailableStreamSlots = _concurrentStreamsSemaphore.CurrentCount,
                CachedMetadataCount = _metadataCache.Count,
                ProgressiveOperationsCount = _progressiveStates.Count,
                AverageCompressionRatio = _totalBytesTransferred > 0 ? (double)_totalCompressionSavings / _totalBytesTransferred : 0
            };
        }

        /// <summary>
        /// ✅ NOVÉ: Cancel operation
        /// </summary>
        public bool CancelOperation(string operationId)
        {
            if (_activeOperations.TryGetValue(operationId, out var operation))
            {
                operation.Status = NetworkOperationStatus.Cancelled;
                _logger.LogInformation("🛑 Operation cancelled - Operation: {OperationId}", operationId);
                return true;
            }
            return false;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Cleanup expired metadata cache entries
        /// </summary>
        private void CleanupMetadataCache(object? state)
        {
            if (_isDisposed) return;

            try
            {
                var expiredKeys = _metadataCache
                    .Where(kvp => DateTime.UtcNow - kvp.Value.CacheTime > TimeSpan.FromMinutes(10))
                    .Select(kvp => kvp.Key)
                    .ToList();

                foreach (var key in expiredKeys)
                {
                    _metadataCache.TryRemove(key, out _);
                }

                if (expiredKeys.Count > 0)
                {
                    _logger.LogTrace("🧹 Cleaned up {Count} expired metadata cache entries", expiredKeys.Count);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error cleaning up metadata cache");
            }
        }

        #endregion

        #region IDisposable

        public void Dispose()
        {
            if (_isDisposed) return;

            _isDisposed = true;

            try
            {
                _cacheCleanupTimer?.Dispose();
                _concurrentStreamsSemaphore?.Dispose();
                _httpClient?.Dispose();

                _activeOperations.Clear();
                _metadataCache.Clear();
                _progressiveStates.Clear();

                _logger.LogInformation("🧹 NetworkIOOptimizationService disposed - InstanceId: {InstanceId}", 
                    _serviceInstanceId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error disposing NetworkIOOptimizationService");
            }
        }

        #endregion
    }

    #region Supporting Classes

    /// <summary>
    /// Network operation info
    /// </summary>
    internal class NetworkOperationInfo
    {
        public string OperationId { get; set; } = string.Empty;
        public NetworkOperationType Type { get; set; }
        public string? FilePath { get; set; }
        public string? Url { get; set; }
        public NetworkOperationStatus Status { get; set; }
        public DateTime StartTime { get; set; }
        public long BytesTransferred { get; set; }
        public int RetryAttempt { get; set; }
    }

    /// <summary>
    /// File metadata
    /// </summary>
    public class FileMetadata
    {
        public string FilePath { get; set; } = string.Empty;
        public long Size { get; set; }
        public DateTime CreatedTime { get; set; }
        public DateTime LastModifiedTime { get; set; }
        public string Extension { get; set; } = string.Empty;
        public DateTime CacheTime { get; set; }
    }

    /// <summary>
    /// Progressive loading state
    /// </summary>
    internal class ProgressiveLoadingState
    {
        public string OperationId { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public long TotalSize { get; set; }
        public int ChunkSize { get; set; }
        public long LoadedBytes { get; set; }
        public DateTime StartTime { get; set; }
    }

    /// <summary>
    /// Progressive loading result
    /// </summary>
    public class ProgressiveLoadingResult
    {
        public byte[] Data { get; set; } = Array.Empty<byte>();
        public bool IsComplete { get; set; }
        public long LoadedBytes { get; set; }
        public long TotalBytes { get; set; }
    }

    /// <summary>
    /// Progressive loading progress
    /// </summary>
    public class ProgressiveLoadingProgress
    {
        public string OperationId { get; set; } = string.Empty;
        public long LoadedBytes { get; set; }
        public long TotalBytes { get; set; }
        public double ProgressPercent { get; set; }
        public bool IsComplete { get; set; }
    }

    /// <summary>
    /// Performance metrics
    /// </summary>
    public class NetworkIOPerformanceMetrics
    {
        public long TotalOperations { get; set; }
        public long TotalBytesTransferred { get; set; }
        public long TotalCompressionSavings { get; set; }
        public int ActiveOperationsCount { get; set; }
        public int AvailableStreamSlots { get; set; }
        public int CachedMetadataCount { get; set; }
        public int ProgressiveOperationsCount { get; set; }
        public double AverageCompressionRatio { get; set; }
    }

    /// <summary>
    /// Optimized stream wrapper
    /// </summary>
    internal class OptimizedStream : Stream
    {
        private readonly Stream _baseStream;
        private readonly Action _onDispose;
        private bool _isDisposed = false;

        public OptimizedStream(Stream baseStream, Action onDispose)
        {
            _baseStream = baseStream;
            _onDispose = onDispose;
        }

        public override bool CanRead => _baseStream.CanRead;
        public override bool CanSeek => _baseStream.CanSeek;
        public override bool CanWrite => _baseStream.CanWrite;
        public override long Length => _baseStream.Length;
        public override long Position { get => _baseStream.Position; set => _baseStream.Position = value; }

        public override void Flush() => _baseStream.Flush();
        public override int Read(byte[] buffer, int offset, int count) => _baseStream.Read(buffer, offset, count);
        public override long Seek(long offset, SeekOrigin origin) => _baseStream.Seek(offset, origin);
        public override void SetLength(long value) => _baseStream.SetLength(value);
        public override void Write(byte[] buffer, int offset, int count) => _baseStream.Write(buffer, offset, count);

        protected override void Dispose(bool disposing)
        {
            if (!_isDisposed && disposing)
            {
                _baseStream?.Dispose();
                _onDispose?.Invoke();
                _isDisposed = true;
            }
            base.Dispose(disposing);
        }
    }

    #endregion
}