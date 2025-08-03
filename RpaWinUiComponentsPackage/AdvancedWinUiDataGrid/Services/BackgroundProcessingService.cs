// Services/BackgroundProcessingService.cs - ✅ NOVÝ: Background Processing Service Implementation
using Microsoft.Extensions.Logging.Abstractions;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Models;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Services
{
    /// <summary>
    /// Implementácia background processing služby pre async data operations - INTERNAL
    /// </summary>
    internal class BackgroundProcessingService : IBackgroundProcessingService, IAsyncDisposable
    {
        #region Private Fields

        private readonly ILogger _logger;
        private readonly string _componentInstanceId = Guid.NewGuid().ToString("N")[..8];
        
        private BackgroundProcessingConfiguration? _configuration;
        private CancellationTokenSource? _currentOperationCts;
        private AsyncLoadingProgress? _currentProgress;
        private Task? _currentTask = null;
        private readonly SemaphoreSlim _operationSemaphore = new(1, 1);
        private bool _isDisposed = false;

        #endregion

        #region Events

        /// <summary>
        /// Event pre progress reporting
        /// </summary>
        public event EventHandler<AsyncLoadingProgress>? ProgressChanged;

        /// <summary>
        /// Event pre completion notification
        /// </summary>
        public event EventHandler<AsyncLoadingResult<object>>? OperationCompleted;

        #endregion

        #region Properties

        /// <summary>
        /// Či je momentálne spustená async operácia
        /// </summary>
        public bool IsOperationRunning => 
            _currentTask != null && !_currentTask.IsCompleted;

        /// <summary>
        /// Aktuálny progress poslednej operácie
        /// </summary>
        public AsyncLoadingProgress? CurrentProgress => _currentProgress;

        #endregion

        #region Constructor

        public BackgroundProcessingService(ILogger? logger = null)
        {
            _logger = logger ?? NullLogger.Instance;
            _logger.LogDebug("⚡ BackgroundProcessingService created - InstanceId: {InstanceId}", 
                _componentInstanceId);
        }

        #endregion

        #region Initialization

        /// <summary>
        /// Inicializuje service s konfiguráciou
        /// </summary>
        public async Task InitializeAsync(BackgroundProcessingConfiguration configuration)
        {
            await _operationSemaphore.WaitAsync().ConfigureAwait(false);
            try
            {
                _logger.LogInformation("🚀 BackgroundProcessingService.InitializeAsync START - InstanceId: {InstanceId}", 
                    _componentInstanceId);

                _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
                _configuration.Validate();

                _logger.LogDebug("✅ BackgroundProcessingService initialized - AsyncDataLoading: {AsyncEnabled}, " +
                    "BatchSize: {BatchSize}, ParallelLoading: {ParallelEnabled}", 
                    _configuration.EnableAsyncDataLoading,
                    _configuration.DataLoadingBatchSize,
                    _configuration.EnableParallelLoading);
            }
            finally
            {
                _operationSemaphore.Release();
            }
        }

        #endregion

        #region Async Data Loading

        /// <summary>
        /// Asynchrónne načíta dáta zo zdroja
        /// </summary>
        public async Task<AsyncLoadingResult<List<Dictionary<string, object>>>> LoadDataAsync<T>(
            Func<IProgress<AsyncLoadingProgress>, CancellationToken, Task<T>> dataLoader,
            CancellationToken cancellationToken = default)
        {
            if (_configuration == null)
                return AsyncLoadingResult<List<Dictionary<string, object>>>.Error("Service not initialized");

            if (!_configuration.EnableAsyncDataLoading)
                return AsyncLoadingResult<List<Dictionary<string, object>>>.Error("Async data loading is disabled");

            await _operationSemaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                _logger.LogInformation("📥 LoadDataAsync START - InstanceId: {InstanceId}", _componentInstanceId);

                var stopwatch = Stopwatch.StartNew();
                _currentOperationCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                var progress = new Progress<AsyncLoadingProgress>(OnProgressChanged);

                try
                {
                    var data = await dataLoader(progress, _currentOperationCts.Token).ConfigureAwait(false);
                    
                    // Konvertuj dáta na dictionary format
                    var resultData = ConvertToGridData(data);
                    
                    stopwatch.Stop();
                    
                    var result = AsyncLoadingResult<List<Dictionary<string, object>>>.Success(
                        resultData, resultData.Count, stopwatch.Elapsed);

                    _logger.LogInformation("✅ LoadDataAsync COMPLETED - Items: {ItemCount}, Duration: {Duration}ms", 
                        resultData.Count, stopwatch.ElapsedMilliseconds);

                    OperationCompleted?.Invoke(this, 
                        AsyncLoadingResult<object>.Success(resultData, resultData.Count, stopwatch.Elapsed));

                    return result;
                }
                catch (OperationCanceledException)
                {
                    _logger.LogWarning("⏹️ LoadDataAsync CANCELLED - InstanceId: {InstanceId}", _componentInstanceId);
                    return AsyncLoadingResult<List<Dictionary<string, object>>>.Cancelled();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "❌ ERROR in LoadDataAsync - InstanceId: {InstanceId}", _componentInstanceId);
                    return AsyncLoadingResult<List<Dictionary<string, object>>>.Error(ex.Message, ex);
                }
            }
            finally
            {
                _currentOperationCts?.Dispose();
                _currentOperationCts = null;
                _operationSemaphore.Release();
            }
        }

        /// <summary>
        /// Asynchrónne načíta dáta s batch processing
        /// </summary>
        public async Task<AsyncLoadingResult<List<Dictionary<string, object>>>> LoadDataBatchAsync<T>(
            Func<int, int, IProgress<AsyncLoadingProgress>, CancellationToken, Task<IEnumerable<T>>> batchLoader,
            int totalCount,
            CancellationToken cancellationToken = default)
        {
            if (_configuration == null)
                return AsyncLoadingResult<List<Dictionary<string, object>>>.Error("Service not initialized");

            await _operationSemaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                _logger.LogInformation("📥 LoadDataBatchAsync START - TotalCount: {TotalCount}, BatchSize: {BatchSize}", 
                    totalCount, _configuration.DataLoadingBatchSize);

                var stopwatch = Stopwatch.StartNew();
                _currentOperationCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                var progress = new Progress<AsyncLoadingProgress>(OnProgressChanged);
                var allData = new List<Dictionary<string, object>>();

                try
                {
                    int batchSize = _configuration.DataLoadingBatchSize;
                    int processedCount = 0;

                    for (int offset = 0; offset < totalCount; offset += batchSize)
                    {
                        _currentOperationCts.Token.ThrowIfCancellationRequested();

                        int currentBatchSize = Math.Min(batchSize, totalCount - offset);
                        var batchData = await batchLoader(offset, currentBatchSize, progress, _currentOperationCts.Token)
                            .ConfigureAwait(false);

                        var convertedBatch = ConvertToGridData(batchData);
                        allData.AddRange(convertedBatch);
                        
                        processedCount += convertedBatch.Count;

                        // Report progress
                        var progressInfo = new AsyncLoadingProgress
                        {
                            ProcessedItems = processedCount,
                            TotalItems = totalCount,
                            PercentageCompleted = (double)processedCount / totalCount * 100,
                            StatusMessage = $"Loaded {processedCount} of {totalCount} items",
                            LastUpdate = DateTime.UtcNow
                        };

                        OnProgressChanged(progressInfo);

                        // Delay ak je potrebný
                        if (_configuration.ProgressReportingInterval > 0)
                        {
                            await Task.Delay(Math.Min(50, _configuration.ProgressReportingInterval / 10), 
                                _currentOperationCts.Token).ConfigureAwait(false);
                        }
                    }

                    stopwatch.Stop();
                    
                    var result = AsyncLoadingResult<List<Dictionary<string, object>>>.Success(
                        allData, allData.Count, stopwatch.Elapsed);

                    _logger.LogInformation("✅ LoadDataBatchAsync COMPLETED - Items: {ItemCount}, Duration: {Duration}ms", 
                        allData.Count, stopwatch.ElapsedMilliseconds);

                    return result;
                }
                catch (OperationCanceledException)
                {
                    _logger.LogWarning("⏹️ LoadDataBatchAsync CANCELLED");
                    return AsyncLoadingResult<List<Dictionary<string, object>>>.Cancelled();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "❌ ERROR in LoadDataBatchAsync");
                    return AsyncLoadingResult<List<Dictionary<string, object>>>.Error(ex.Message, ex);
                }
            }
            finally
            {
                _currentOperationCts?.Dispose();
                _currentOperationCts = null;
                _operationSemaphore.Release();
            }
        }

        /// <summary>
        /// Asynchrónne načíta dáta so streaming support
        /// </summary>
        public async Task<AsyncLoadingResult<IAsyncEnumerable<Dictionary<string, object>>>> LoadDataStreamAsync<T>(
            Func<IProgress<AsyncLoadingProgress>, CancellationToken, IAsyncEnumerable<T>> streamLoader,
            CancellationToken cancellationToken = default)
        {
            if (_configuration == null)
                return AsyncLoadingResult<IAsyncEnumerable<Dictionary<string, object>>>.Error("Service not initialized");

            if (!_configuration.EnableDataStreaming)
                return AsyncLoadingResult<IAsyncEnumerable<Dictionary<string, object>>>.Error("Data streaming is disabled");

            _logger.LogInformation("📥 LoadDataStreamAsync START - InstanceId: {InstanceId}", _componentInstanceId);

            var stopwatch = Stopwatch.StartNew();
            _currentOperationCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            var progress = new Progress<AsyncLoadingProgress>(OnProgressChanged);

            try
            {
                var dataStream = streamLoader(progress, _currentOperationCts.Token);
                var convertedStream = ConvertStreamToGridData(dataStream);
                
                stopwatch.Stop();
                
                return AsyncLoadingResult<IAsyncEnumerable<Dictionary<string, object>>>.Success(
                    convertedStream, 0, stopwatch.Elapsed);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ ERROR in LoadDataStreamAsync");
                return AsyncLoadingResult<IAsyncEnumerable<Dictionary<string, object>>>.Error(ex.Message, ex);
            }
        }

        #endregion

        #region Data Processing

        /// <summary>
        /// Asynchrónne spracuje dáta na pozadí
        /// </summary>
        public async Task<AsyncLoadingResult<TResult>> ProcessDataAsync<TInput, TResult>(
            TInput inputData,
            Func<TInput, IProgress<AsyncLoadingProgress>, CancellationToken, Task<TResult>> processor,
            CancellationToken cancellationToken = default)
        {
            await _operationSemaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                _logger.LogInformation("⚙️ ProcessDataAsync START - InstanceId: {InstanceId}", _componentInstanceId);

                var stopwatch = Stopwatch.StartNew();
                _currentOperationCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                var progress = new Progress<AsyncLoadingProgress>(OnProgressChanged);

                try
                {
                    var result = await processor(inputData, progress, _currentOperationCts.Token).ConfigureAwait(false);
                    stopwatch.Stop();

                    _logger.LogInformation("✅ ProcessDataAsync COMPLETED - Duration: {Duration}ms", 
                        stopwatch.ElapsedMilliseconds);

                    return AsyncLoadingResult<TResult>.Success(result, 1, stopwatch.Elapsed);
                }
                catch (OperationCanceledException)
                {
                    _logger.LogWarning("⏹️ ProcessDataAsync CANCELLED");
                    return AsyncLoadingResult<TResult>.Cancelled();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "❌ ERROR in ProcessDataAsync");
                    return AsyncLoadingResult<TResult>.Error(ex.Message, ex);
                }
            }
            finally
            {
                _currentOperationCts?.Dispose();
                _currentOperationCts = null;
                _operationSemaphore.Release();
            }
        }

        /// <summary>
        /// Parallel processing viacerých items
        /// </summary>
        public async Task<AsyncLoadingResult<List<TResult>>> ProcessDataParallelAsync<TInput, TResult>(
            IEnumerable<TInput> inputData,
            Func<TInput, CancellationToken, Task<TResult>> processor,
            CancellationToken cancellationToken = default)
        {
            if (_configuration == null)
                return AsyncLoadingResult<List<TResult>>.Error("Service not initialized");

            if (!_configuration.EnableParallelLoading)
                return AsyncLoadingResult<List<TResult>>.Error("Parallel processing is disabled");

            var dataList = inputData.ToList();
            _logger.LogInformation("⚙️ ProcessDataParallelAsync START - Items: {ItemCount}, MaxConcurrency: {MaxConcurrency}", 
                dataList.Count, _configuration.MaxConcurrentTasks);

            var stopwatch = Stopwatch.StartNew();
            _currentOperationCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            try
            {
                var semaphore = new SemaphoreSlim(_configuration.MaxConcurrentTasks, _configuration.MaxConcurrentTasks);
                var tasks = dataList.Select(async item =>
                {
                    await semaphore.WaitAsync(_currentOperationCts.Token).ConfigureAwait(false);
                    try
                    {
                        return await processor(item, _currentOperationCts.Token).ConfigureAwait(false);
                    }
                    finally
                    {
                        semaphore.Release();
                    }
                });

                var results = await Task.WhenAll(tasks).ConfigureAwait(false);
                stopwatch.Stop();

                _logger.LogInformation("✅ ProcessDataParallelAsync COMPLETED - Items: {ItemCount}, Duration: {Duration}ms", 
                    results.Length, stopwatch.ElapsedMilliseconds);

                return AsyncLoadingResult<List<TResult>>.Success(results.ToList(), results.Length, stopwatch.Elapsed);
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("⏹️ ProcessDataParallelAsync CANCELLED");
                return AsyncLoadingResult<List<TResult>>.Cancelled();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ ERROR in ProcessDataParallelAsync");
                return AsyncLoadingResult<List<TResult>>.Error(ex.Message, ex);
            }
        }

        #endregion

        #region Operation Management

        /// <summary>
        /// Zruší aktuálne bežiacu operáciu
        /// </summary>
        public async Task CancelCurrentOperationAsync()
        {
            _logger.LogInformation("⏹️ CancelCurrentOperationAsync - InstanceId: {InstanceId}", _componentInstanceId);

            _currentOperationCts?.Cancel();

            if (_currentTask != null)
            {
                try
                {
                    await _currentTask.ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    _logger.LogDebug("✅ Operation cancelled successfully");
                }
            }
        }

        /// <summary>
        /// Čaká na dokončenie všetkých operácií
        /// </summary>
        public async Task WaitForCompletionAsync(CancellationToken cancellationToken = default)
        {
            if (_currentTask != null)
            {
                try
                {
                    await _currentTask.WaitAsync(cancellationToken).ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    _logger.LogDebug("WaitForCompletionAsync cancelled");
                }
            }
        }

        /// <summary>
        /// Získa stav aktuálnej operácie
        /// </summary>
        public AsyncLoadingProgress? GetCurrentOperationStatus()
        {
            return _currentProgress;
        }

        #endregion

        #region Private Methods

        private void OnProgressChanged(AsyncLoadingProgress progress)
        {
            _currentProgress = progress;
            ProgressChanged?.Invoke(this, progress);

            _logger.LogTrace("📊 Progress: {Percentage:F1}% - {StatusMessage}", 
                progress.PercentageCompleted, progress.StatusMessage);
        }

        private List<Dictionary<string, object>> ConvertToGridData<T>(T data)
        {
            var result = new List<Dictionary<string, object>>();

            if (data is IEnumerable<object> enumerable)
            {
                foreach (var item in enumerable)
                {
                    var dict = ConvertObjectToDictionary(item);
                    result.Add(dict);
                }
            }
            else if (data != null)
            {
                var dict = ConvertObjectToDictionary(data);
                result.Add(dict);
            }

            return result;
        }

        private async IAsyncEnumerable<Dictionary<string, object>> ConvertStreamToGridData<T>(IAsyncEnumerable<T> stream)
        {
            await foreach (var item in stream.ConfigureAwait(false))
            {
                if (item != null)
                {
                    yield return ConvertObjectToDictionary(item);
                }
            }
        }

        private Dictionary<string, object> ConvertObjectToDictionary(object obj)
        {
            var result = new Dictionary<string, object>();

            if (obj is Dictionary<string, object> dict)
            {
                return new Dictionary<string, object>(dict);
            }

            // Use reflection na konverziu object properties
            var properties = obj.GetType().GetProperties();
            foreach (var prop in properties)
            {
                try
                {
                    var value = prop.GetValue(obj);
                    result[prop.Name] = value ?? string.Empty;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to get property value: {PropertyName}", prop.Name);
                    result[prop.Name] = string.Empty;
                }
            }

            return result;
        }

        #endregion

        #region Dispose

        /// <summary>
        /// Vyčistí resources a pozastaví všetky operácie
        /// </summary>
        public async ValueTask DisposeAsync()
        {
            if (_isDisposed) return;

            _logger.LogDebug("🧹 BackgroundProcessingService.DisposeAsync - InstanceId: {InstanceId}", 
                _componentInstanceId);

            await CancelCurrentOperationAsync().ConfigureAwait(false);

            _currentOperationCts?.Dispose();
            _operationSemaphore?.Dispose();

            _isDisposed = true;
        }

        #endregion
    }
}