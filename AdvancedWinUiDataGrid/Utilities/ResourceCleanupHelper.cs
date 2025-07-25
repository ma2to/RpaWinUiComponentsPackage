// Utilities/ResourceCleanupHelper.cs - ✅ OPRAVENÝ accessibility
using System;
using System.Runtime;
using System.Threading.Tasks;

namespace RpaWinUiComponents.AdvancedWinUiDataGrid.Utilities
{
    /// <summary>
    /// Helper trieda pre cleanup zdrojov a pamäte - ✅ INTERNAL
    /// </summary>
    internal class ResourceCleanupHelper  // ✅ CHANGED: public -> internal
    {
        private readonly object _cleanupLock = new object();
        private DateTime _lastCleanup = DateTime.MinValue;
        private readonly TimeSpan _cleanupInterval = TimeSpan.FromMinutes(1);

        /// <summary>
        /// Vykoná garbage collection ak je potrebná
        /// </summary>
        public async Task ForceGarbageCollectionAsync()
        {
            await Task.Run(() =>
            {
                lock (_cleanupLock)
                {
                    var now = DateTime.UtcNow;
                    if (now - _lastCleanup < _cleanupInterval)
                        return;

                    // Force garbage collection
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                    GC.Collect();

                    // Compact Large Object Heap
                    GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
                    GC.Collect();

                    _lastCleanup = now;
                }
            });
        }

        /// <summary>
        /// Vykoná cleanup slabých referencií
        /// </summary>
        public void CleanupWeakReferences<T>(System.Collections.Generic.List<WeakReference<T>> weakReferences)
            where T : class
        {
            for (int i = weakReferences.Count - 1; i >= 0; i--)
            {
                if (!weakReferences[i].TryGetTarget(out _))
                {
                    weakReferences.RemoveAt(i);
                }
            }
        }

        /// <summary>
        /// Uvoľní large object ak je potrebné
        /// </summary>
        public void ReleaseLargeObject<T>(ref T? largeObject) where T : class, IDisposable
        {
            if (largeObject != null)
            {
                try
                {
                    largeObject.Dispose();
                }
                catch
                {
                    // Ignore disposal errors
                }
                finally
                {
                    largeObject = null;
                }
            }
        }

        /// <summary>
        /// Kontroluje dostupnú pamäť
        /// </summary>
        public bool IsLowMemory()
        {
            const long lowMemoryThreshold = 100 * 1024 * 1024; // 100 MB

            try
            {
                var availableMemory = GC.GetTotalMemory(false);
                return availableMemory > lowMemoryThreshold;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Získa memory usage info
        /// </summary>
        public MemoryInfo GetMemoryInfo()
        {
            return new MemoryInfo
            {
                TotalMemory = GC.GetTotalMemory(false),
                Gen0Collections = GC.CollectionCount(0),
                Gen1Collections = GC.CollectionCount(1),
                Gen2Collections = GC.CollectionCount(2),
                IsServerGC = GCSettings.IsServerGC,
                LargeObjectHeapCompactionMode = GCSettings.LargeObjectHeapCompactionMode
            };
        }
    }

    /// <summary>
    /// Informácie o pamäti - ✅ INTERNAL
    /// </summary>
    internal class MemoryInfo  // ✅ CHANGED: public -> internal
    {
        public long TotalMemory { get; set; }
        public int Gen0Collections { get; set; }
        public int Gen1Collections { get; set; }
        public int Gen2Collections { get; set; }
        public bool IsServerGC { get; set; }
        public GCLargeObjectHeapCompactionMode LargeObjectHeapCompactionMode { get; set; }

        public override string ToString()
        {
            return $"Memory: {TotalMemory / 1024 / 1024:F1} MB, GC: {Gen0Collections}/{Gen1Collections}/{Gen2Collections}, Server: {IsServerGC}";
        }
    }
}