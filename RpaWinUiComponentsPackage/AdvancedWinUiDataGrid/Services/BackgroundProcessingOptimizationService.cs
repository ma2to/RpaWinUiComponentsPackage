// Services/BackgroundProcessingOptimizationService.cs - ✅ NOVÉ: Background Processing Optimization Service
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Services
{
    /// <summary>
    /// ✅ NOVÉ: Background Processing Optimization Service - channel-based task processing, priority queues
    /// </summary>
    internal class BackgroundProcessingOptimizationService : IDisposable
    {
        #region Private Fields

        private readonly ILogger _logger;
        private readonly object _lockObject = new();
        private bool _isDisposed = false;

        // ✅ NOVÉ: Channel-based task processing
        private readonly Channel<BackgroundTask> _highPriorityChannel;
        private readonly Channel<BackgroundTask> _mediumPriorityChannel;
        private readonly Channel<BackgroundTask> _lowPriorityChannel;
        private readonly ChannelWriter<BackgroundTask> _highPriorityWriter;
        private readonly ChannelWriter<BackgroundTask> _mediumPriorityWriter;
        private readonly ChannelWriter<BackgroundTask> _lowPriorityWriter;

        // ✅ NOVÉ: Worker tasks
        private readonly List<Task> _workerTasks = new();
        private readonly CancellationTokenSource _cancellationTokenSource = new();

        // ✅ NOVÉ: Task tracking
        private readonly ConcurrentDictionary<string, BackgroundTask> _runningTasks = new();
        private readonly ConcurrentDictionary<string, TaskCompletionSource<BackgroundTaskResult>> _taskCompletions = new();

        // ✅ NOVÉ: Performance monitoring
        private int _totalTasksProcessed = 0;
        private int _tasksCompleted = 0;
        private int _tasksFailed = 0;
        private int _tasksCancelled = 0;
        private readonly List<double> _taskExecutionTimes = new();

        // ✅ NOVÉ: Configuration
        private BackgroundProcessingConfiguration _config;

        #endregion

        #region Events

        /// <summary>
        /// Event vyvolaný pri dokončení background task
        /// </summary>
        public event EventHandler<BackgroundTaskCompletedEventArgs>? TaskCompleted;

        /// <summary>
        /// Event vyvolaný pri chybe background task
        /// </summary>
        public event EventHandler<BackgroundTaskErrorEventArgs>? TaskError;

        /// <summary>
        /// Event vyvolaný pri performance warning
        /// </summary>
        public event EventHandler<BackgroundProcessingWarningEventArgs>? PerformanceWarning;

        #endregion

        #region Constructor & Initialization

        public BackgroundProcessingOptimizationService(
            BackgroundProcessingConfiguration? config = null,
            ILogger? logger = null)
        {
            _logger = logger ?? NullLogger.Instance;
            _config = config ?? BackgroundProcessingConfiguration.Default;

            // Create channels with bounded capacity for backpressure
            var channelOptions = new BoundedChannelOptions(_config.MaxQueueSize)
            {
                FullMode = BoundedChannelFullMode.Wait,
                SingleReader = false,
                SingleWriter = false
            };

            _highPriorityChannel = Channel.CreateBounded<BackgroundTask>(channelOptions);
            _mediumPriorityChannel = Channel.CreateBounded<BackgroundTask>(channelOptions);
            _lowPriorityChannel = Channel.CreateBounded<BackgroundTask>(channelOptions);

            _highPriorityWriter = _highPriorityChannel.Writer;
            _mediumPriorityWriter = _mediumPriorityChannel.Writer;
            _lowPriorityWriter = _lowPriorityChannel.Writer;

            // Start worker tasks
            StartWorkerTasks();

            _logger.LogDebug("🚀 BackgroundProcessingOptimizationService initialized - " +
                "Workers: {WorkerCount}, MaxQueue: {MaxQueue}",
                _config.MaxConcurrentTasks, _config.MaxQueueSize);
        }

        #endregion

        #region Task Submission

        /// <summary>
        /// ✅ NOVÉ: Submit background task for processing
        /// </summary>
        public async Task<string> SubmitTaskAsync<T>(
            Func<CancellationToken, Task<T>> taskFunc,
            BackgroundTaskPriority priority = BackgroundTaskPriority.Medium,
            string? taskId = null,
            TimeSpan? timeout = null,
            CancellationToken cancellationToken = default)
        {
            if (_isDisposed)
                throw new ObjectDisposedException(nameof(BackgroundProcessingOptimizationService));

            var task = new BackgroundTask
            {
                Id = taskId ?? Guid.NewGuid().ToString("N")[..8],
                Priority = priority,
                TaskFunction = async (ct) => 
                {
                    var result = await taskFunc(ct);
                    return new BackgroundTaskResult { Result = result, IsSuccess = true };
                },
                SubmittedAt = DateTime.UtcNow,
                Timeout = timeout ?? TimeSpan.FromMilliseconds(_config.DefaultTaskTimeoutMs)
            };

            // Create completion source for this task
            var completionSource = new TaskCompletionSource<BackgroundTaskResult>();
            _taskCompletions[task.Id] = completionSource;

            // Submit to appropriate channel based on priority
            var writer = priority switch
            {
                BackgroundTaskPriority.High => _highPriorityWriter,
                BackgroundTaskPriority.Medium => _mediumPriorityWriter,
                BackgroundTaskPriority.Low => _lowPriorityWriter,
                _ => _mediumPriorityWriter
            };

            try
            {
                await writer.WriteAsync(task, cancellationToken);
                _logger.LogTrace("📝 Background task submitted - ID: {TaskId}, Priority: {Priority}",
                    task.Id, priority);
            }
            catch (Exception ex)
            {
                _taskCompletions.TryRemove(task.Id, out _);
                _logger.LogError(ex, "❌ Error submitting background task - ID: {TaskId}", task.Id);
                throw;
            }

            return task.Id;
        }

        /// <summary>
        /// ✅ NOVÉ: Submit fire-and-forget background task
        /// </summary>
        public async Task<string> SubmitFireAndForgetTaskAsync(
            Func<CancellationToken, Task> taskFunc,
            BackgroundTaskPriority priority = BackgroundTaskPriority.Low,
            string? taskId = null,
            CancellationToken cancellationToken = default)
        {
            return await SubmitTaskAsync(async (ct) =>
            {
                await taskFunc(ct);
                return true; // Fire and forget - just return success
            }, priority, taskId, null, cancellationToken);
        }

        /// <summary>
        /// ✅ NOVÉ: Wait for task completion
        /// </summary>
        public async Task<BackgroundTaskResult> WaitForTaskAsync(string taskId, 
            CancellationToken cancellationToken = default)
        {
            if (_taskCompletions.TryGetValue(taskId, out var completionSource))
            {
                try
                {
                    var result = await completionSource.Task.WaitAsync(cancellationToken);
                    _taskCompletions.TryRemove(taskId, out _);
                    return result;
                }
                catch (TimeoutException)
                {
                    _logger.LogWarning("⏰ Background task timeout - ID: {TaskId}", taskId);
                    return new BackgroundTaskResult 
                    { 
                        IsSuccess = false, 
                        ErrorMessage = "Task timeout" 
                    };
                }
            }

            throw new InvalidOperationException($"Task {taskId} not found or already completed");
        }

        #endregion

        #region Task Processing

        /// <summary>
        /// ✅ NOVÉ: Start worker tasks for processing
        /// </summary>
        private void StartWorkerTasks()
        {
            // Start high priority workers
            for (int i = 0; i < Math.Max(1, _config.MaxConcurrentTasks / 2); i++)
            {
                var workerTask = Task.Run(() => ProcessTasksAsync(_highPriorityChannel.Reader, 
                    BackgroundTaskPriority.High, _cancellationTokenSource.Token));
                _workerTasks.Add(workerTask);
            }

            // Start medium priority workers
            for (int i = 0; i < Math.Max(1, _config.MaxConcurrentTasks / 3); i++)
            {
                var workerTask = Task.Run(() => ProcessTasksAsync(_mediumPriorityChannel.Reader,
                    BackgroundTaskPriority.Medium, _cancellationTokenSource.Token));
                _workerTasks.Add(workerTask);
            }

            // Start low priority workers
            for (int i = 0; i < Math.Max(1, _config.MaxConcurrentTasks / 6); i++)
            {
                var workerTask = Task.Run(() => ProcessTasksAsync(_lowPriorityChannel.Reader,
                    BackgroundTaskPriority.Low, _cancellationTokenSource.Token));
                _workerTasks.Add(workerTask);
            }

            _logger.LogDebug("✅ Started {WorkerCount} background worker tasks", _workerTasks.Count);
        }

        /// <summary>
        /// ✅ NOVÉ: Process tasks from channel
        /// </summary>
        private async Task ProcessTasksAsync(ChannelReader<BackgroundTask> reader,
            BackgroundTaskPriority priority, CancellationToken cancellationToken)
        {
            await foreach (var task in reader.ReadAllAsync(cancellationToken))
            {
                if (cancellationToken.IsCancellationRequested)
                    break;

                await ProcessSingleTaskAsync(task, cancellationToken);
            }

            _logger.LogDebug("🔄 Background worker stopped - Priority: {Priority}", priority);
        }

        /// <summary>
        /// ✅ NOVÉ: Process single background task
        /// </summary>
        private async Task ProcessSingleTaskAsync(BackgroundTask task, CancellationToken cancellationToken)
        {
            var stopwatch = Stopwatch.StartNew();
            BackgroundTaskResult result = new()
            {
                IsSuccess = false,
                ErrorMessage = "Task not executed",
                ExecutionTimeMs = 0
            };

            try
            {
                _runningTasks[task.Id] = task;
                task.StartedAt = DateTime.UtcNow;

                _logger.LogTrace("🔄 Processing background task - ID: {TaskId}, Priority: {Priority}",
                    task.Id, task.Priority);

                // Create timeout cancellation token
                using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                timeoutCts.CancelAfter(task.Timeout);

                // Execute the task
                result = await task.TaskFunction(timeoutCts.Token);
                result.ExecutionTimeMs = stopwatch.ElapsedMilliseconds;

                _tasksCompleted++;
                UpdateTaskMetrics(stopwatch.ElapsedMilliseconds);

                _logger.LogTrace("✅ Background task completed - ID: {TaskId}, Duration: {Duration}ms",
                    task.Id, stopwatch.ElapsedMilliseconds);

                // Notify completion
                TaskCompleted?.Invoke(this, new BackgroundTaskCompletedEventArgs(task, result));
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                result = new BackgroundTaskResult
                {
                    IsSuccess = false,
                    ErrorMessage = "Task was cancelled",
                    ExecutionTimeMs = stopwatch.ElapsedMilliseconds
                };

                _tasksCancelled++;
                _logger.LogDebug("🛑 Background task cancelled - ID: {TaskId}", task.Id);
            }
            catch (OperationCanceledException) // Timeout
            {
                result = new BackgroundTaskResult
                {
                    IsSuccess = false,
                    ErrorMessage = "Task timeout",
                    ExecutionTimeMs = stopwatch.ElapsedMilliseconds
                };

                _tasksFailed++;
                _logger.LogWarning("⏰ Background task timeout - ID: {TaskId}, Timeout: {Timeout}ms",
                    task.Id, task.Timeout.TotalMilliseconds);
            }
            catch (Exception ex)
            {
                result = new BackgroundTaskResult
                {
                    IsSuccess = false,
                    ErrorMessage = ex.Message,
                    Exception = ex,
                    ExecutionTimeMs = stopwatch.ElapsedMilliseconds
                };

                _tasksFailed++;
                _logger.LogError(ex, "❌ Background task failed - ID: {TaskId}", task.Id);

                // Notify error
                TaskError?.Invoke(this, new BackgroundTaskErrorEventArgs(task, ex));
            }
            finally
            {
                stopwatch.Stop();
                _runningTasks.TryRemove(task.Id, out _);
                _totalTasksProcessed++;

                // Complete the task
                if (_taskCompletions.TryRemove(task.Id, out var completionSource))
                {
                    completionSource.SetResult(result);
                }
            }
        }

        #endregion

        #region Performance Monitoring

        /// <summary>
        /// ✅ NOVÉ: Update task execution metrics
        /// </summary>
        private void UpdateTaskMetrics(double executionTimeMs)
        {
            lock (_lockObject)
            {
                _taskExecutionTimes.Add(executionTimeMs);

                // Keep only last 100 measurements
                if (_taskExecutionTimes.Count > 100)
                    _taskExecutionTimes.RemoveAt(0);

                // Check for performance warnings
                var avgExecutionTime = _taskExecutionTimes.Average();
                if (executionTimeMs > _config.PerformanceWarningThresholdMs)
                {
                    var warning = new BackgroundProcessingWarningEventArgs(
                        executionTimeMs,
                        avgExecutionTime,
                        _runningTasks.Count,
                        GetQueueSizes(),
                        $"Task execution time {executionTimeMs:F2}ms exceeds threshold {_config.PerformanceWarningThresholdMs}ms"
                    );

                    PerformanceWarning?.Invoke(this, warning);

                    _logger.LogWarning("⚠️ Background Processing Performance Warning - " +
                        "Execution: {ExecutionTime:F2}ms, Average: {AvgTime:F2}ms, " +
                        "Running: {RunningCount}, Queued: {QueuedTotal}",
                        executionTimeMs, avgExecutionTime, _runningTasks.Count, 
                        warning.QueueSizes.High + warning.QueueSizes.Medium + warning.QueueSizes.Low);
                }
            }
        }

        /// <summary>
        /// ✅ NOVÉ: Get current queue sizes
        /// </summary>
        private BackgroundProcessingQueueSizes GetQueueSizes()
        {
            return new BackgroundProcessingQueueSizes
            {
                High = _highPriorityChannel.Reader.CanCount ? _highPriorityChannel.Reader.Count : 0,
                Medium = _mediumPriorityChannel.Reader.CanCount ? _mediumPriorityChannel.Reader.Count : 0,
                Low = _lowPriorityChannel.Reader.CanCount ? _lowPriorityChannel.Reader.Count : 0
            };
        }

        #endregion

        #region Task Management

        /// <summary>
        /// ✅ NOVÉ: Cancel specific task
        /// </summary>
        public bool CancelTask(string taskId)
        {
            if (_runningTasks.TryGetValue(taskId, out var task))
            {
                // Task is running - can't cancel directly, but will be cancelled when worker checks
                _logger.LogDebug("🛑 Task cancellation requested - ID: {TaskId}", taskId);
                return true;
            }

            if (_taskCompletions.TryRemove(taskId, out var completionSource))
            {
                completionSource.SetCanceled();
                _logger.LogDebug("🛑 Queued task cancelled - ID: {TaskId}", taskId);
                return true;
            }

            return false;
        }

        /// <summary>
        /// ✅ NOVÉ: Get task status
        /// </summary>
        public BackgroundTaskStatus GetTaskStatus(string taskId)
        {
            if (_runningTasks.ContainsKey(taskId))
                return BackgroundTaskStatus.Running;

            if (_taskCompletions.ContainsKey(taskId))
                return BackgroundTaskStatus.Queued;

            return BackgroundTaskStatus.NotFound;
        }

        /// <summary>
        /// ✅ NOVÉ: Get all running tasks
        /// </summary>
        public List<BackgroundTask> GetRunningTasks()
        {
            return _runningTasks.Values.ToList();
        }

        /// <summary>
        /// ✅ NOVÉ: Clear all queued tasks
        /// </summary>
        public async Task ClearAllQueuedTasksAsync()
        {
            try
            {
                // Complete all channels to prevent new writes
                _highPriorityWriter.Complete();
                _mediumPriorityWriter.Complete();
                _lowPriorityWriter.Complete();

                // Cancel all pending completions
                var pendingTasks = _taskCompletions.Keys.ToList();
                foreach (var taskId in pendingTasks)
                {
                    if (_taskCompletions.TryRemove(taskId, out var completionSource))
                    {
                        completionSource.SetCanceled();
                    }
                }

                _logger.LogInformation("🧹 Cleared all queued background tasks - Count: {Count}", 
                    pendingTasks.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error clearing queued tasks");
                throw;
            }
        }

        #endregion

        #region Public Properties & Configuration

        /// <summary>
        /// Konfigurácia background processing
        /// </summary>
        public BackgroundProcessingConfiguration Configuration => _config;

        /// <summary>
        /// Aktualizuje konfiguráciu
        /// </summary>
        public void UpdateConfiguration(BackgroundProcessingConfiguration config)
        {
            lock (_lockObject)
            {
                config.Validate();
                _config = config.Clone();
                _logger.LogDebug("⚙️ Background processing configuration updated");
            }
        }

        /// <summary>
        /// Získa štatistiky background processing
        /// </summary>
        public BackgroundProcessingStats GetStats()
        {
            var avgExecutionTime = _taskExecutionTimes.Count > 0 ? _taskExecutionTimes.Average() : 0;
            var queueSizes = GetQueueSizes();

            return new BackgroundProcessingStats
            {
                TotalTasksProcessed = _totalTasksProcessed,
                TasksCompleted = _tasksCompleted,
                TasksFailed = _tasksFailed,
                TasksCancelled = _tasksCancelled,
                RunningTasksCount = _runningTasks.Count,
                QueueSizes = queueSizes,
                AverageExecutionTimeMs = avgExecutionTime,
                WorkerTasksCount = _workerTasks.Count,
                RecentExecutionTimes = new List<double>(_taskExecutionTimes)
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
                    // Signal cancellation to all workers
                    _cancellationTokenSource.Cancel();

                    // Complete all channels
                    _highPriorityWriter.Complete();
                    _mediumPriorityWriter.Complete();
                    _lowPriorityWriter.Complete();

                    // Wait for workers to finish with timeout
                    Task.WaitAll(_workerTasks.ToArray(), TimeSpan.FromSeconds(5));

                    // Dispose cancellation token source
                    _cancellationTokenSource.Dispose();

                    // Cancel all pending tasks
                    foreach (var completionSource in _taskCompletions.Values)
                    {
                        completionSource.TrySetCanceled();
                    }
                    _taskCompletions.Clear();

                    _logger.LogDebug("🔄 BackgroundProcessingOptimizationService disposed");
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "⚠️ Error during BackgroundProcessingOptimizationService disposal");
                }

                _isDisposed = true;
            }
        }

        #endregion
    }

    #region Supporting Classes and Enums

    /// <summary>
    /// ✅ NOVÉ: Background task
    /// </summary>
    public class BackgroundTask
    {
        public string Id { get; set; } = string.Empty;
        public BackgroundTaskPriority Priority { get; set; }
        public Func<CancellationToken, Task<BackgroundTaskResult>> TaskFunction { get; set; } = null!;
        public DateTime SubmittedAt { get; set; }
        public DateTime? StartedAt { get; set; }
        public TimeSpan Timeout { get; set; }
        public string? Description { get; set; }
        public Dictionary<string, object>? Metadata { get; set; }
    }

    /// <summary>
    /// ✅ NOVÉ: Background task result
    /// </summary>
    public class BackgroundTaskResult
    {
        public bool IsSuccess { get; set; }
        public object? Result { get; set; }
        public string? ErrorMessage { get; set; }
        public Exception? Exception { get; set; }
        public long ExecutionTimeMs { get; set; }
    }

    /// <summary>
    /// ✅ NOVÉ: Background task priority
    /// </summary>
    public enum BackgroundTaskPriority
    {
        Low = 0,
        Medium = 1,
        High = 2
    }

    /// <summary>
    /// ✅ NOVÉ: Background task status
    /// </summary>
    public enum BackgroundTaskStatus
    {
        NotFound,
        Queued,
        Running,
        Completed,
        Failed,
        Cancelled
    }

    /// <summary>
    /// ✅ NOVÉ: Queue sizes for monitoring
    /// </summary>
    public class BackgroundProcessingQueueSizes
    {
        public int High { get; set; }
        public int Medium { get; set; }
        public int Low { get; set; }
        public int Total => High + Medium + Low;
    }

    /// <summary>
    /// ✅ NOVÉ: Event args for task completion
    /// </summary>
    public class BackgroundTaskCompletedEventArgs : EventArgs
    {
        public BackgroundTask Task { get; }
        public BackgroundTaskResult Result { get; }

        public BackgroundTaskCompletedEventArgs(BackgroundTask task, BackgroundTaskResult result)
        {
            Task = task;
            Result = result;
        }
    }

    /// <summary>
    /// ✅ NOVÉ: Event args for task errors
    /// </summary>
    public class BackgroundTaskErrorEventArgs : EventArgs
    {
        public BackgroundTask Task { get; }
        public Exception Exception { get; }

        public BackgroundTaskErrorEventArgs(BackgroundTask task, Exception exception)
        {
            Task = task;
            Exception = exception;
        }
    }

    /// <summary>
    /// ✅ NOVÉ: Event args for performance warnings
    /// </summary>
    public class BackgroundProcessingWarningEventArgs : EventArgs
    {
        public double ExecutionTimeMs { get; }
        public double AverageExecutionTimeMs { get; }
        public int RunningTasksCount { get; }
        public BackgroundProcessingQueueSizes QueueSizes { get; }
        public string Message { get; }

        public BackgroundProcessingWarningEventArgs(double executionTime, double averageExecutionTime,
            int runningTasksCount, BackgroundProcessingQueueSizes queueSizes, string message)
        {
            ExecutionTimeMs = executionTime;
            AverageExecutionTimeMs = averageExecutionTime;
            RunningTasksCount = runningTasksCount;
            QueueSizes = queueSizes;
            Message = message;
        }
    }

    /// <summary>
    /// ✅ NOVÉ: Background processing statistics
    /// </summary>
    public class BackgroundProcessingStats
    {
        public int TotalTasksProcessed { get; set; }
        public int TasksCompleted { get; set; }
        public int TasksFailed { get; set; }
        public int TasksCancelled { get; set; }
        public int RunningTasksCount { get; set; }
        public BackgroundProcessingQueueSizes QueueSizes { get; set; } = new();
        public double AverageExecutionTimeMs { get; set; }
        public int WorkerTasksCount { get; set; }
        public List<double> RecentExecutionTimes { get; set; } = new();

        public double SuccessRate => TotalTasksProcessed > 0 
            ? (double)TasksCompleted / TotalTasksProcessed * 100 
            : 0;

        public override string ToString()
        {
            return $"BackgroundProcessing: Processed={TotalTasksProcessed}, " +
                   $"Success={TasksCompleted}, Failed={TasksFailed}, Cancelled={TasksCancelled}, " +
                   $"Running={RunningTasksCount}, Queued={QueueSizes.Total}, " +
                   $"Workers={WorkerTasksCount}, AvgTime={AverageExecutionTimeMs:F2}ms, " +
                   $"SuccessRate={SuccessRate:F1}%";
        }
    }

    #endregion
}