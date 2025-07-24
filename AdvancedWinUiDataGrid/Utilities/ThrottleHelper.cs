// Utilities/ThrottleHelper.cs
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace RpaWinUiComponents.AdvancedWinUiDataGrid
{
    /// <summary>
    /// Utility trieda pre throttling a debouncing operácií.
    /// Poskytuje mechanizmy pre optimalizáciu výkonu pri častých operáciách.
    /// </summary>
    internal static class ThrottleHelper
    {
        #region Private fields

        private static readonly ConcurrentDictionary<string, Timer> _debounceTimers = new();
        private static readonly ConcurrentDictionary<string, DateTime> _lastThrottleExecution = new();
        private static readonly object _lockObject = new();

        #endregion

        #region Debounce operations

        /// <summary>
        /// Debounce akciu - vykoná ju až po určitom čase nečinnosti.
        /// </summary>
        /// <param name="key">Unikátny kľúč pre identifikáciu debounce skupiny</param>
        /// <param name="action">Akcia na vykonanie</param>
        /// <param name="delayMs">Delay v milisekundách</param>
        public static void Debounce(string key, Action action, int delayMs)
        {
            if (string.IsNullOrEmpty(key) || action == null || delayMs <= 0)
                return;

            // Zrušiť existujúci timer pre tento kľúč
            if (_debounceTimers.TryRemove(key, out var existingTimer))
            {
                existingTimer?.Dispose();
            }

            // Vytvoriť nový timer
            var timer = new Timer(_ =>
            {
                try
                {
                    action();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Chyba pri debounced akcii '{key}': {ex.Message}");
                }
                finally
                {
                    // Vyčistiť timer po vykonaní
                    if (_debounceTimers.TryRemove(key, out var completedTimer))
                    {
                        completedTimer?.Dispose();
                    }
                }
            }, null, delayMs, Timeout.Infinite);

            _debounceTimers.TryAdd(key, timer);
        }

        /// <summary>
        /// Async verzia debounce.
        /// </summary>
        /// <param name="key">Unikátny kľúč pre identifikáciu debounce skupiny</param>
        /// <param name="asyncAction">Async akcia na vykonanie</param>
        /// <param name="delayMs">Delay v milisekundách</param>
        public static void DebounceAsync(string key, Func<Task> asyncAction, int delayMs)
        {
            Debounce(key, () =>
            {
                Task.Run(async () =>
                {
                    try
                    {
                        await asyncAction();
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Chyba pri async debounced akcii '{key}': {ex.Message}");
                    }
                });
            }, delayMs);
        }

        /// <summary>
        /// Zruší debounce pre konkrétny kľúč.
        /// </summary>
        /// <param name="key">Kľúč debounce skupiny</param>
        public static void CancelDebounce(string key)
        {
            if (_debounceTimers.TryRemove(key, out var timer))
            {
                timer?.Dispose();
            }
        }

        /// <summary>
        /// Zruší všetky aktívne debounce operácie.
        /// </summary>
        public static void CancelAllDebounce()
        {
            var timers = _debounceTimers.Values;
            _debounceTimers.Clear();

            foreach (var timer in timers)
            {
                timer?.Dispose();
            }
        }

        #endregion

        #region Throttle operations

        /// <summary>
        /// Throttle akciu - obmedzí maximálnu frekvenciu vykonávania.
        /// </summary>
        /// <param name="key">Unikátny kľúč pre identifikáciu throttle skupiny</param>
        /// <param name="action">Akcia na vykonanie</param>
        /// <param name="intervalMs">Minimálny interval medzi vykonaniami v ms</param>
        /// <returns>True ak bola akcia vykonaná, false ak bola throttled</returns>
        public static bool Throttle(string key, Action action, int intervalMs)
        {
            if (string.IsNullOrEmpty(key) || action == null || intervalMs <= 0)
                return false;

            lock (_lockObject)
            {
                var now = DateTime.UtcNow;

                if (_lastThrottleExecution.TryGetValue(key, out var lastExecution))
                {
                    var timeSinceLastExecution = now - lastExecution;
                    if (timeSinceLastExecution.TotalMilliseconds < intervalMs)
                    {
                        // Throttled - príliš skoro od posledného vykonania
                        return false;
                    }
                }

                // Vykonať akciu a zaznamenať čas
                try
                {
                    action();
                    _lastThrottleExecution[key] = now;
                    return true;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Chyba pri throttled akcii '{key}': {ex.Message}");
                    return false;
                }
            }
        }

        /// <summary>
        /// Async verzia throttle.
        /// </summary>
        /// <param name="key">Unikátny kľúč pre identifikáciu throttle skupiny</param>
        /// <param name="asyncAction">Async akcia na vykonanie</param>
        /// <param name="intervalMs">Minimálny interval medzi vykonaniami v ms</param>
        /// <returns>True ak bola akcia spustená, false ak bola throttled</returns>
        public static bool ThrottleAsync(string key, Func<Task> asyncAction, int intervalMs)
        {
            return Throttle(key, () =>
            {
                Task.Run(async () =>
                {
                    try
                    {
                        await asyncAction();
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Chyba pri async throttled akcii '{key}': {ex.Message}");
                    }
                });
            }, intervalMs);
        }

        /// <summary>
        /// Resetuje throttle históriu pre konkrétny kľúč.
        /// </summary>
        /// <param name="key">Kľúč throttle skupiny</param>
        public static void ResetThrottle(string key)
        {
            _lastThrottleExecution.TryRemove(key, out _);
        }

        /// <summary>
        /// Resetuje všetky throttle histórie.
        /// </summary>
        public static void ResetAllThrottles()
        {
            _lastThrottleExecution.Clear();
        }

        #endregion

        #region Batch operations

        /// <summary>
        /// Vykoná batch operáciu s throttling medzi batch-mi.
        /// </summary>
        /// <typeparam name="T">Typ dát na spracovanie</typeparam>
        /// <param name="items">Kolekcia položiek na spracovanie</param>
        /// <param name="batchSize">Veľkosť batch-u</param>
        /// <param name="batchAction">Akcia na vykonanie pre každý batch</param>
        /// <param name="delayBetweenBatchesMs">Delay medzi batch-mi v ms</param>
        /// <param name="cancellationToken">Token pre zrušenie</param>
        /// <returns>Task pre asynchrónne spracovanie</returns>
        public static async Task ProcessInBatchesAsync<T>(
            IEnumerable<T> items,
            int batchSize,
            Func<IEnumerable<T>, Task> batchAction,
            int delayBetweenBatchesMs = 1,
            CancellationToken cancellationToken = default)
        {
            if (items == null || batchAction == null || batchSize <= 0)
                return;

            var itemsList = items.ToList();
            var totalBatches = (int)Math.Ceiling((double)itemsList.Count / batchSize);

            for (int i = 0; i < totalBatches; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var batch = itemsList.Skip(i * batchSize).Take(batchSize);

                try
                {
                    await batchAction(batch);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Chyba pri spracovaní batch {i + 1}/{totalBatches}: {ex.Message}");
                    throw;
                }

                // Delay medzi batch-mi (okrem posledného)
                if (i < totalBatches - 1 && delayBetweenBatchesMs > 0)
                {
                    await Task.Delay(delayBetweenBatchesMs, cancellationToken);
                }
            }
        }

        /// <summary>
        /// Vykoná batch operáciu so synchronnou akciou.
        /// </summary>
        /// <typeparam name="T">Typ dát na spracovanie</typeparam>
        /// <param name="items">Kolekcia položiek na spracovanie</param>
        /// <param name="batchSize">Veľkosť batch-u</param>
        /// <param name="batchAction">Akcia na vykonanie pre každý batch</param>
        /// <param name="delayBetweenBatchesMs">Delay medzi batch-mi v ms</param>
        /// <param name="cancellationToken">Token pre zrušenie</param>
        /// <returns>Task pre asynchrónne spracovanie</returns>
        public static async Task ProcessInBatchesAsync<T>(
            IEnumerable<T> items,
            int batchSize,
            Action<IEnumerable<T>> batchAction,
            int delayBetweenBatchesMs = 1,
            CancellationToken cancellationToken = default)
        {
            await ProcessInBatchesAsync(
                items,
                batchSize,
                batch =>
                {
                    batchAction(batch);
                    return Task.CompletedTask;
                },
                delayBetweenBatchesMs,
                cancellationToken);
        }

        #endregion

        #region Rate limiting

        /// <summary>
        /// Implementuje rate limiting pre operácie.
        /// </summary>
        /// <param name="key">Unikátny kľúč pre rate limiter</param>
        /// <param name="maxOperations">Maximálny počet operácií</param>
        /// <param name="timeWindowMs">Časové okno v ms</param>
        /// <returns>True ak je operácia povolená</returns>
        public static bool IsOperationAllowed(string key, int maxOperations, int timeWindowMs)
        {
            // Jednoduchá implementácia - v produkčnej verzii by sme mali sliding window
            var throttleKey = $"rate_limit_{key}";
            var operationKey = $"rate_count_{key}";

            lock (_lockObject)
            {
                var now = DateTime.UtcNow;

                // Resetovať počítadlo ak prešlo časové okno
                if (_lastThrottleExecution.TryGetValue(throttleKey, out var lastReset))
                {
                    if ((now - lastReset).TotalMilliseconds >= timeWindowMs)
                    {
                        _lastThrottleExecution[operationKey] = new DateTime(0); // Reset counter
                        _lastThrottleExecution[throttleKey] = now;
                    }
                }
                else
                {
                    _lastThrottleExecution[throttleKey] = now;
                    _lastThrottleExecution[operationKey] = new DateTime(0);
                }

                // Skontrolovať počet operácií
                var operationCount = (int)(_lastThrottleExecution.GetValueOrDefault(operationKey).Ticks % int.MaxValue);

                if (operationCount >= maxOperations)
                {
                    return false; // Rate limited
                }

                // Inkrementovať počítadlo
                _lastThrottleExecution[operationKey] = new DateTime(operationCount + 1);
                return true;
            }
        }

        #endregion

        #region Cleanup

        /// <summary>
        /// Vyčistí všetky throttle a debounce štruktúry.
        /// </summary>
        public static void Cleanup()
        {
            CancelAllDebounce();
            ResetAllThrottles();
        }

        /// <summary>
        /// Vyčistí staré záznamy throttle histórie.
        /// </summary>
        /// <param name="olderThanMinutes">Vymaže záznamy staršie ako tento počet minút</param>
        public static void CleanupOldThrottleHistory(int olderThanMinutes = 60)
        {
            var cutoffTime = DateTime.UtcNow.AddMinutes(-olderThanMinutes);
            var keysToRemove = new List<string>();

            foreach (var kvp in _lastThrottleExecution)
            {
                if (kvp.Value < cutoffTime)
                {
                    keysToRemove.Add(kvp.Key);
                }
            }

            foreach (var key in keysToRemove)
            {
                _lastThrottleExecution.TryRemove(key, out _);
            }
        }

        #endregion

        #region Performance monitoring

        /// <summary>
        /// Získa štatistiky throttling operácií.
        /// </summary>
        /// <returns>Anonymný objekt so štatistikami</returns>
        public static object GetThrottlingStatistics()
        {
            return new
            {
                ActiveDebounceOperations = _debounceTimers.Count,
                ThrottleHistoryEntries = _lastThrottleExecution.Count,
                MemoryEstimateKB = (_debounceTimers.Count + _lastThrottleExecution.Count) * 0.1 // Rough estimate
            };
        }

        #endregion
    }

    #region Helper classes

    /// <summary>
    /// Utility trieda pre jednorazové throttling objekty.
    /// </summary>
    internal class ThrottleInstance : IDisposable
    {
        private readonly string _key;
        private bool _disposed = false;

        public ThrottleInstance(string key)
        {
            _key = key ?? throw new ArgumentNullException(nameof(key));
        }

        public bool Execute(Action action, int intervalMs)
        {
            return ThrottleHelper.Throttle(_key, action, intervalMs);
        }

        public void Debounce(Action action, int delayMs)
        {
            ThrottleHelper.Debounce(_key, action, delayMs);
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                ThrottleHelper.CancelDebounce(_key);
                ThrottleHelper.ResetThrottle(_key);
                _disposed = true;
            }
        }
    }

    #endregion
}