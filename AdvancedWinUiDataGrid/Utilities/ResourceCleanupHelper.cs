// Utilities/ResourceCleanupHelper.cs
using System;
using System.Collections.Generic;
using System.Runtime;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

namespace RpaWinUiComponents.AdvancedWinUiDataGrid
{
    /// <summary>
    /// Utility trieda pre správu a cleanup zdrojov v DataGrid komponente.
    /// Poskytuje automatické uvoľňovanie pamäte a optimalizácie výkonu.
    /// </summary>
    internal static class ResourceCleanupHelper
    {
        #region Private fields

        private static readonly List<WeakReference<IDisposable>> _trackedResources = new();
        private static readonly Timer _cleanupTimer;
        private static readonly object _lockObject = new();
        private static readonly SemaphoreSlim _cleanupSemaphore = new(1, 1);

        // Štatistiky
        private static int _totalAllocations = 0;
        private static int _totalDeallocations = 0;
        private static DateTime _lastCleanup = DateTime.UtcNow;

        #endregion

        #region Static constructor

        static ResourceCleanupHelper()
        {
            // Automatický cleanup každých 30 sekúnd
            _cleanupTimer = new Timer(async _ => await PerformAutomaticCleanupAsync(),
                null, TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(30));
        }

        #endregion

        #region Resource tracking

        /// <summary>
        /// Zaregistruje zdroj pre automatické sledovanie a cleanup.
        /// </summary>
        /// <param name="resource">IDisposable zdroj na sledovanie</param>
        public static void TrackResource(IDisposable resource)
        {
            if (resource == null)
                return;

            lock (_lockObject)
            {
                _trackedResources.Add(new WeakReference<IDisposable>(resource));
                Interlocked.Increment(ref _totalAllocations);
            }
        }

        /// <summary>
        /// Odregistruje zdroj z automatického sledovania.
        /// </summary>
        /// <param name="resource">IDisposable zdroj na odregistrovanie</param>
        public static void UntrackResource(IDisposable resource)
        {
            if (resource == null)
                return;

            lock (_lockObject)
            {
                for (int i = _trackedResources.Count - 1; i >= 0; i--)
                {
                    if (_trackedResources[i].TryGetTarget(out var trackedResource) &&
                        ReferenceEquals(trackedResource, resource))
                    {
                        _trackedResources.RemoveAt(i);
                        Interlocked.Increment(ref _totalDeallocations);
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Vyčistí všetky mŕtve weak references zo sledovania.
        /// </summary>
        public static void CleanupDeadReferences()
        {
            lock (_lockObject)
            {
                var deadReferences = 0;

                for (int i = _trackedResources.Count - 1; i >= 0; i--)
                {
                    if (!_trackedResources[i].TryGetTarget(out _))
                    {
                        _trackedResources.RemoveAt(i);
                        deadReferences++;
                    }
                }

                if (deadReferences > 0)
                {
                    System.Diagnostics.Debug.WriteLine($"Vyčistených {deadReferences} mŕtvych referencií");
                }
            }
        }

        #endregion

        #region Manual cleanup operations

        /// <summary>
        /// Vykoná manuálny cleanup všetkých sledovaných zdrojov.
        /// </summary>
        /// <param name="forceDispose">Či vykonať force dispose na živých zdrojoch</param>
        /// <returns>Počet vyčistených zdrojov</returns>
        public static async Task<int> PerformManualCleanupAsync(bool forceDispose = false)
        {
            await _cleanupSemaphore.WaitAsync();
            try
            {
                return await PerformCleanupInternalAsync(forceDispose);
            }
            finally
            {
                _cleanupSemaphore.Release();
            }
        }

        /// <summary>
        /// Vykoná automatický cleanup (bez force dispose).
        /// </summary>
        private static async Task PerformAutomaticCleanupAsync()
        {
            if (!_cleanupSemaphore.Wait(100)) // Non-blocking check
                return;

            try
            {
                await PerformCleanupInternalAsync(forceDispose: false);
            }
            finally
            {
                _cleanupSemaphore.Release();
            }
        }

        /// <summary>
        /// Vnútorná implementácia cleanup logiky.
        /// </summary>
        private static async Task<int> PerformCleanupInternalAsync(bool forceDispose)
        {
            var cleanedCount = 0;
            var resourcesToCleanup = new List<IDisposable>();

            // Zbierať zdroje na cleanup
            lock (_lockObject)
            {
                for (int i = _trackedResources.Count - 1; i >= 0; i--)
                {
                    var weakRef = _trackedResources[i];

                    if (!weakRef.TryGetTarget(out var resource))
                    {
                        // Mŕtva referencia
                        _trackedResources.RemoveAt(i);
                        cleanedCount++;
                    }
                    else if (forceDispose)
                    {
                        // Force dispose na živých zdrojoch
                        resourcesToCleanup.Add(resource);
                        _trackedResources.RemoveAt(i);
                    }
                }
            }

            // Dispose zdrojov mimo lock-u
            foreach (var resource in resourcesToCleanup)
            {
                try
                {
                    resource.Dispose();
                    cleanedCount++;
                    Interlocked.Increment(ref _totalDeallocations);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Chyba pri dispose zdroja: {ex.Message}");
                }
            }

            _lastCleanup = DateTime.UtcNow;
            await Task.CompletedTask;

            if (cleanedCount > 0)
            {
                System.Diagnostics.Debug.WriteLine($"Cleanup dokončený: {cleanedCount} zdrojov vyčistených");
            }

            return cleanedCount;
        }

        #endregion

        #region Memory management

        /// <summary>
        /// Vykoná garbage collection s optimalizáciami.
        /// </summary>
        /// <param name="aggressive">Či vykonať agresívny GC</param>
        public static void ForceGarbageCollection(bool aggressive = false)
        {
            try
            {
                if (aggressive)
                {
                    // Agresívny cleanup
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                    GC.Collect();
                }
                else
                {
                    // Štandardný cleanup
                    GC.Collect(0, GCCollectionMode.Optimized);
                }

                // Compact Large Object Heap ak je potreba
                if (aggressive)
                {
                    GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
                    GC.Collect();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Chyba pri garbage collection: {ex.Message}");
            }
        }

        /// <summary>
        /// Získa informácie o využití pamäte.
        /// </summary>
        /// <returns>Objekt s informáciami o pamäti</returns>
        public static MemoryInfo GetMemoryInfo()
        {
            try
            {
                var totalMemory = GC.GetTotalMemory(false);
                var gen0Collections = GC.CollectionCount(0);
                var gen1Collections = GC.CollectionCount(1);
                var gen2Collections = GC.CollectionCount(2);

                int trackedResourcesCount;
                lock (_lockObject)
                {
                    trackedResourcesCount = _trackedResources.Count;
                }

                return new MemoryInfo
                {
                    TotalMemoryBytes = totalMemory,
                    TrackedResourcesCount = trackedResourcesCount,
                    Gen0Collections = gen0Collections,
                    Gen1Collections = gen1Collections,
                    Gen2Collections = gen2Collections,
                    TotalAllocations = _totalAllocations,
                    TotalDeallocations = _totalDeallocations,
                    LastCleanupTime = _lastCleanup
                };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Chyba pri získavaní memory info: {ex.Message}");
                return new MemoryInfo(); // Return empty info
            }
        }

        #endregion

        #region Collection management

        /// <summary>
        /// Optimalizuje kolekcie odstránením prázdnych slot-ov.
        /// </summary>
        /// <typeparam name="T">Typ elementov v kolekcii</typeparam>
        /// <param name="collection">Kolekcia na optimalizáciu</param>
        /// <returns>Počet odstránených prázdnych slot-ov</returns>
        public static int OptimizeCollection<T>(IList<T> collection) where T : class
        {
            if (collection == null)
                return 0;

            var removedCount = 0;

            for (int i = collection.Count - 1; i >= 0; i--)
            {
                if (collection[i] == null)
                {
                    collection.RemoveAt(i);
                    removedCount++;
                }
            }

            return removedCount;
        }

        /// <summary>
        /// Vyčistí kolekciu od objektov ktoré spĺňajú predikát.
        /// </summary>
        /// <typeparam name="T">Typ elementov v kolekcii</typeparam>
        /// <param name="collection">Kolekcia na vyčistenie</param>
        /// <param name="shouldRemove">Predikát pre určenie čo odstrániť</param>
        /// <returns>Počet odstránených elementov</returns>
        public static int CleanupCollection<T>(IList<T> collection, Func<T, bool> shouldRemove)
        {
            if (collection == null || shouldRemove == null)
                return 0;

            var removedCount = 0;

            for (int i = collection.Count - 1; i >= 0; i--)
            {
                try
                {
                    if (shouldRemove(collection[i]))
                    {
                        // Dispose ak je to možné
                        if (collection[i] is IDisposable disposable)
                        {
                            disposable.Dispose();
                        }

                        collection.RemoveAt(i);
                        removedCount++;
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Chyba pri cleanup collection item: {ex.Message}");
                }
            }

            return removedCount;
        }

        #endregion

        #region Cache management

        /// <summary>
        /// Vyčistí cache štruktúry starších ako určitý vek.
        /// </summary>
        /// <typeparam name="TKey">Typ kľúča</typeparam>
        /// <typeparam name="TValue">Typ hodnoty</typeparam>
        /// <param name="cache">Cache slovník</param>
        /// <param name="getAge">Funkcia pre získanie veku hodnoty</param>
        /// <param name="maxAgeMinutes">Maximálny vek v minútach</param>
        /// <returns>Počet vyčistených položiek</returns>
        public static int CleanupCache<TKey, TValue>(
            IDictionary<TKey, TValue> cache,
            Func<TValue, DateTime> getAge,
            int maxAgeMinutes) where TKey : notnull
        {
            if (cache == null || getAge == null)
                return 0;

            var cutoffTime = DateTime.UtcNow.AddMinutes(-maxAgeMinutes);
            var keysToRemove = new List<TKey>();

            foreach (var kvp in cache)
            {
                try
                {
                    var age = getAge(kvp.Value);
                    if (age < cutoffTime)
                    {
                        keysToRemove.Add(kvp.Key);
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Chyba pri určovaní veku cache položky: {ex.Message}");
                    keysToRemove.Add(kvp.Key); // Remove problematic items
                }
            }

            foreach (var key in keysToRemove)
            {
                if (cache.TryGetValue(key, out var value) && value is IDisposable disposable)
                {
                    disposable.Dispose();
                }
                cache.Remove(key);
            }

            return keysToRemove.Count;
        }

        #endregion

        #region Shutdown and disposal

        /// <summary>
        /// Vykoná finálny cleanup pri shutdown aplikácie.
        /// </summary>
        public static async Task ShutdownCleanupAsync()
        {
            try
            {
                // Stop cleanup timer
                _cleanupTimer?.Dispose();

                // Force cleanup všetkých zdrojov
                await PerformManualCleanupAsync(forceDispose: true);

                // Force garbage collection
                ForceGarbageCollection(aggressive: true);

                _cleanupSemaphore?.Dispose();

                System.Diagnostics.Debug.WriteLine("Shutdown cleanup dokončený");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Chyba pri shutdown cleanup: {ex.Message}");
            }
        }

        #endregion

        #region Statistics and monitoring

        /// <summary>
        /// Získa štatistiky cleanup operácií.
        /// </summary>
        /// <returns>Objekt so štatistikami</returns>
        public static CleanupStatistics GetCleanupStatistics()
        {
            lock (_lockObject)
            {
                return new CleanupStatistics
                {
                    TrackedResourcesCount = _trackedResources.Count,
                    TotalAllocations = _totalAllocations,
                    TotalDeallocations = _totalDeallocations,
                    LastCleanupTime = _lastCleanup,
                    MemoryInfo = GetMemoryInfo()
                };
            }
        }

        #endregion
    }

    #region Helper classes

    /// <summary>
    /// Informácie o využití pamäte.
    /// </summary>
    internal class MemoryInfo
    {
        public long TotalMemoryBytes { get; set; }
        public int TrackedResourcesCount { get; set; }
        public int Gen0Collections { get; set; }
        public int Gen1Collections { get; set; }
        public int Gen2Collections { get; set; }
        public int TotalAllocations { get; set; }
        public int TotalDeallocations { get; set; }
        public DateTime LastCleanupTime { get; set; }

        public double MemoryMB => TotalMemoryBytes / (1024.0 * 1024.0);
        public int PendingResources => TotalAllocations - TotalDeallocations;

        public override string ToString()
        {
            return $"Memory: {MemoryMB:F1}MB, Tracked: {TrackedResourcesCount}, Pending: {PendingResources}";
        }
    }

    /// <summary>
    /// Štatistiky cleanup operácií.
    /// </summary>
    internal class CleanupStatistics
    {
        public int TrackedResourcesCount { get; set; }
        public int TotalAllocations { get; set; }
        public int TotalDeallocations { get; set; }
        public DateTime LastCleanupTime { get; set; }
        public MemoryInfo? MemoryInfo { get; set; }

        public TimeSpan TimeSinceLastCleanup => DateTime.UtcNow - LastCleanupTime;
        public double CleanupEfficiency => TotalAllocations > 0 ? (double)TotalDeallocations / TotalAllocations : 0;

        public override string ToString()
        {
            return $"Allocations: {TotalAllocations}, Deallocations: {TotalDeallocations}, " +
                   $"Efficiency: {CleanupEfficiency:P1}, Last cleanup: {TimeSinceLastCleanup:mm\\:ss} ago";
        }
    }

    /// <summary>
    /// Disposable wrapper pre automatické tracking zdrojov.
    /// </summary>
    internal class TrackedResource : IDisposable
    {
        private readonly IDisposable _resource;
        private bool _disposed = false;

        public TrackedResource(IDisposable resource)
        {
            _resource = resource ?? throw new ArgumentNullException(nameof(resource));
            ResourceCleanupHelper.TrackResource(this);
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                try
                {
                    _resource?.Dispose();
                }
                finally
                {
                    ResourceCleanupHelper.UntrackResource(this);
                    _disposed = true;
                }
            }
        }
    }

    #endregion
}