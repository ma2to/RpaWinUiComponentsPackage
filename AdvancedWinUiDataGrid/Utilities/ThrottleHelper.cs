// Utilities/ThrottleHelper.cs
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace RpaWinUiComponents.AdvancedWinUiDataGrid.Utilities
{
    /// <summary>
    /// Helper trieda pre throttling operácií
    /// </summary>
    public class ThrottleHelper
    {
        private readonly ConcurrentDictionary<string, CancellationTokenSource> _pendingOperations = new();
        private int _debounceTimeMs = 300;

        /// <summary>
        /// Nastaví debounce time
        /// </summary>
        public void SetDebounceTime(int milliseconds)
        {
            _debounceTimeMs = Math.Max(0, milliseconds);
        }

        /// <summary>
        /// Vykoná throttled operáciu
        /// </summary>
        public async Task<T> ThrottleAsync<T>(string key, Func<T> operation)
        {
            if (_debounceTimeMs <= 0)
            {
                // Immediate execution
                return operation();
            }

            // Cancel previous operation with the same key
            if (_pendingOperations.TryRemove(key, out var previousCts))
            {
                previousCts.Cancel();
                previousCts.Dispose();
            }

            // Create new cancellation token
            var cts = new CancellationTokenSource();
            _pendingOperations[key] = cts;

            try
            {
                // Wait for debounce time
                await Task.Delay(_debounceTimeMs, cts.Token);

                // If not cancelled, execute operation
                if (!cts.Token.IsCancellationRequested)
                {
                    return operation();
                }
            }
            catch (OperationCanceledException)
            {
                // Operation was cancelled, return default
            }
            finally
            {
                // Cleanup
                _pendingOperations.TryRemove(key, out _);
                cts.Dispose();
            }

            return default(T)!;
        }

        /// <summary>
        /// Vykoná throttled async operáciu
        /// </summary>
        public async Task<T> ThrottleAsync<T>(string key, Func<Task<T>> operation)
        {
            if (_debounceTimeMs <= 0)
            {
                return await operation();
            }

            if (_pendingOperations.TryRemove(key, out var previousCts))
            {
                previousCts.Cancel();
                previousCts.Dispose();
            }

            var cts = new CancellationTokenSource();
            _pendingOperations[key] = cts;

            try
            {
                await Task.Delay(_debounceTimeMs, cts.Token);

                if (!cts.Token.IsCancellationRequested)
                {
                    return await operation();
                }
            }
            catch (OperationCanceledException)
            {
                // Operation was cancelled
            }
            finally
            {
                _pendingOperations.TryRemove(key, out _);
                cts.Dispose();
            }

            return default(T)!;
        }

        /// <summary>
        /// Zruší všetky pending operácie
        /// </summary>
        public void CancelAll()
        {
            foreach (var kvp in _pendingOperations)
            {
                kvp.Value.Cancel();
                kvp.Value.Dispose();
            }
            _pendingOperations.Clear();
        }

        /// <summary>
        /// Dispose helper
        /// </summary>
        public void Dispose()
        {
            CancelAll();
        }
    }
}