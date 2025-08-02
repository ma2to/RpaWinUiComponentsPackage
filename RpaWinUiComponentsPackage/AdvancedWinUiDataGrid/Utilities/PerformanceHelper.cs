// AdvancedWinUiDataGrid/Utilities/PerformanceHelper.cs - ✅ NOVÝ SÚBOR
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime;
using System.Threading.Tasks;

namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Utilities
{
    /// <summary>
    /// Helper trieda pre performance monitoring a optimization - INTERNAL
    /// </summary>
    internal class PerformanceHelper
    {
        private readonly ILogger _logger;
        private readonly Dictionary<string, Stopwatch> _activeOperations = new();
        private readonly Dictionary<string, List<double>> _operationHistory = new();

        public PerformanceHelper(ILogger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Začne meranie operácie
        /// </summary>
        public void StartOperation(string operationName)
        {
            var stopwatch = Stopwatch.StartNew();
            _activeOperations[operationName] = stopwatch;
            _logger.LogTrace("⏱️ Performance: {Operation} START", operationName);
        }

        /// <summary>
        /// Dokončí meranie operácie a vráti trvanie
        /// </summary>
        public double EndOperation(string operationName)
        {
            if (_activeOperations.TryGetValue(operationName, out var stopwatch))
            {
                stopwatch.Stop();
                var duration = stopwatch.Elapsed.TotalMilliseconds;

                // Uloži do histórie
                if (!_operationHistory.ContainsKey(operationName))
                    _operationHistory[operationName] = new List<double>();

                _operationHistory[operationName].Add(duration);

                // Log performance warning ak je operácia pomalá
                if (duration > 1000) // Viac ako 1 sekunda
                {
                    _logger.LogWarning("⚠️ Performance: SLOW {Operation} - {Duration:F2}ms", operationName, duration);
                }
                else if (duration > 100) // Viac ako 100ms
                {
                    _logger.LogDebug("⚡ Performance: {Operation} - {Duration:F2}ms", operationName, duration);
                }
                else
                {
                    _logger.LogTrace("⏱️ Performance: {Operation} - {Duration:F2}ms", operationName, duration);
                }

                _activeOperations.Remove(operationName);
                return Math.Round(duration, 2);
            }

            _logger.LogWarning("⚠️ Performance: EndOperation called for unknown operation: {Operation}", operationName);
            return 0;
        }

        /// <summary>
        /// Získa štatistiky pre operáciu
        /// </summary>
        public OperationStats GetOperationStats(string operationName)
        {
            if (!_operationHistory.ContainsKey(operationName))
                return new OperationStats { OperationName = operationName };

            var history = _operationHistory[operationName];
            var stats = new OperationStats
            {
                OperationName = operationName,
                CallCount = history.Count,
                TotalTime = history.Sum(),
                AverageTime = history.Average(),
                MinTime = history.Min(),
                MaxTime = history.Max()
            };

            return stats;
        }

        /// <summary>
        /// Získa memory usage info
        /// </summary>
        public MemoryUsageInfo GetMemoryUsage()
        {
            var memoryBefore = GC.GetTotalMemory(false);
            var gen0 = GC.CollectionCount(0);
            var gen1 = GC.CollectionCount(1);
            var gen2 = GC.CollectionCount(2);

            return new MemoryUsageInfo
            {
                TotalMemoryBytes = memoryBefore,
                Gen0Collections = gen0,
                Gen1Collections = gen1,
                Gen2Collections = gen2,
                IsServerGC = GCSettings.IsServerGC
            };
        }

        /// <summary>
        /// Force garbage collection ak je potrebná
        /// </summary>
        public async Task OptimizeMemoryAsync()
        {
            await Task.Run(() =>
            {
                var memoryBefore = GC.GetTotalMemory(false);

                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();

                var memoryAfter = GC.GetTotalMemory(true);
                var freed = memoryBefore - memoryAfter;

                _logger.LogDebug("🧠 Memory optimized: {FreedMB:F1} MB freed", freed / 1024.0 / 1024.0);
            });
        }

        /// <summary>
        /// Diagnostické informácie o performance
        /// </summary>
        public string GetDiagnosticSummary()
        {
            var summary = new List<string>();

            foreach (var kvp in _operationHistory)
            {
                var stats = GetOperationStats(kvp.Key);
                summary.Add($"{stats.OperationName}: {stats.CallCount}x, avg {stats.AverageTime:F1}ms");
            }

            var memory = GetMemoryUsage();
            summary.Add($"Memory: {memory.TotalMemoryBytes / 1024 / 1024:F1}MB");

            return string.Join(", ", summary);
        }
    }

    /// <summary>
    /// Štatistiky operácie
    /// </summary>
    internal class OperationStats
    {
        public string OperationName { get; set; } = string.Empty;
        public int CallCount { get; set; }
        public double TotalTime { get; set; }
        public double AverageTime { get; set; }
        public double MinTime { get; set; }
        public double MaxTime { get; set; }

        public override string ToString()
        {
            return $"{OperationName}: {CallCount}x, avg {AverageTime:F1}ms (min {MinTime:F1}ms, max {MaxTime:F1}ms)";
        }
    }

    /// <summary>
    /// Memory usage informácie
    /// </summary>
    internal class MemoryUsageInfo
    {
        public long TotalMemoryBytes { get; set; }
        public int Gen0Collections { get; set; }
        public int Gen1Collections { get; set; }
        public int Gen2Collections { get; set; }
        public bool IsServerGC { get; set; }

        public override string ToString()
        {
            return $"Memory: {TotalMemoryBytes / 1024 / 1024:F1}MB, GC: {Gen0Collections}/{Gen1Collections}/{Gen2Collections}, Server: {IsServerGC}";
        }
    }
}