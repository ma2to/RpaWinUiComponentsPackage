// Models/VirtualScrollingConfiguration.cs - ✅ NOVÝ: Virtual Scrolling Configuration
using System;

namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Models
{
    /// <summary>
    /// Konfigurácia pre virtual scrolling - optimalizácia výkonu pre veľké datasety
    /// </summary>
    public class VirtualScrollingConfiguration
    {
        #region Properties

        /// <summary>
        /// Povolenie horizontal virtualization pre stĺpce
        /// </summary>
        public bool EnableHorizontalVirtualization { get; set; } = true;

        /// <summary>
        /// Povolenie vertical virtualization pre riadky
        /// </summary>
        public bool EnableVerticalVirtualization { get; set; } = true;

        /// <summary>
        /// Počet stĺpcov ktoré sa majú vykresliť pred/za viewport
        /// </summary>
        public int ColumnBufferSize { get; set; } = 5;

        /// <summary>
        /// Počet riadkov ktoré sa majú vykresliť pred/za viewport
        /// </summary>
        public int RowBufferSize { get; set; } = 10;

        /// <summary>
        /// Minimálna šírka stĺpca pre virtualization
        /// </summary>
        public double MinColumnWidth { get; set; } = 50.0;

        /// <summary>
        /// Minimálna výška riadka pre virtualization
        /// </summary>
        public double MinRowHeight { get; set; } = 25.0;

        /// <summary>
        /// Povolenie variable row heights (pre wrapping text)
        /// </summary>
        public bool EnableVariableRowHeights { get; set; } = false;

        /// <summary>
        /// Maximálna výška riadka (pre variable heights)
        /// </summary>
        public double MaxRowHeight { get; set; } = 200.0;

        /// <summary>
        /// Povolenie smooth scrolling animácií
        /// </summary>
        public bool EnableSmoothScrolling { get; set; } = true;

        /// <summary>
        /// Rýchlosť smooth scrolling animácie (ms)
        /// </summary>
        public int SmoothScrollingDuration { get; set; } = 200;

        /// <summary>
        /// Prahová hodnota pre memory monitoring (MB)
        /// </summary>
        public double MemoryThresholdMB { get; set; } = 100.0;

        /// <summary>
        /// Interval pre memory monitoring (ms)
        /// </summary>
        public int MemoryMonitoringInterval { get; set; } = 5000;

        /// <summary>
        /// Povolenie memory monitoring
        /// </summary>
        public bool EnableMemoryMonitoring { get; set; } = true;

        /// <summary>
        /// Automatické cleanup pri prekročení memory threshold
        /// </summary>
        public bool EnableAutoCleanup { get; set; } = true;

        /// <summary>
        /// Počet buniek ktoré sa majú recyklovať
        /// </summary>
        public int CellRecyclingPoolSize { get; set; } = 100;

        /// <summary>
        /// ✅ NOVÉ: Minimálny počet riadkov pre aktiváciu virtual scrolling
        /// </summary>
        public int MinRowsForActivation { get; set; } = 100;

        /// <summary>
        /// ✅ NOVÉ: Počet viditeľných riadkov vo viewport
        /// </summary>
        public int VisibleRowCount { get; set; } = 50;

        /// <summary>
        /// ✅ NOVÉ: Optimalizovaná výška riadku pre výkonnosť
        /// </summary>
        public double OptimizedRowHeight { get; set; } = 36.0;

        /// <summary>
        /// ✅ NOVÉ: Throttling delay pre scroll events v ms (16ms = 60 FPS)
        /// </summary>
        public int ScrollThrottleMs { get; set; } = 16;

        /// <summary>
        /// ✅ NOVÉ: Povoliť lazy loading UI elementov
        /// </summary>
        public bool EnableLazyLoading { get; set; } = true;

        /// <summary>
        /// ✅ NOVÉ: Maximálny počet UI elementov v cache
        /// </summary>
        public int MaxCachedUIElements { get; set; } = 200;

        /// <summary>
        /// ✅ NOVÉ: Povoliť viewport change events pre diagnostiku
        /// </summary>
        public bool EnableViewportChangeEvents { get; set; } = false;

        /// <summary>
        /// ✅ NOVÉ: Povoliť selective invalidation (len zmenené oblasti)
        /// </summary>
        public bool EnableSelectiveInvalidation { get; set; } = true;

        /// <summary>
        /// ✅ NOVÉ: Povoliť diagnostické informácie
        /// </summary>
        public bool EnableDiagnostics { get; set; } = false;

        #endregion

        #region Static Configurations

        /// <summary>
        /// Základná konfigurácia pre malé datasety (< 1000 riadkov)
        /// </summary>
        public static VirtualScrollingConfiguration Basic => new()
        {
            EnableHorizontalVirtualization = false,
            EnableVerticalVirtualization = true,
            ColumnBufferSize = 3,
            RowBufferSize = 5,
            EnableVariableRowHeights = false,
            EnableSmoothScrolling = true,
            SmoothScrollingDuration = 150,
            EnableMemoryMonitoring = false,
            CellRecyclingPoolSize = 50
        };

        /// <summary>
        /// Optimalizovaná konfigurácia pre stredné datasety (1000-10000 riadkov)
        /// </summary>
        public static VirtualScrollingConfiguration Optimized => new()
        {
            EnableHorizontalVirtualization = true,
            EnableVerticalVirtualization = true,
            ColumnBufferSize = 5,
            RowBufferSize = 10,
            EnableVariableRowHeights = true,
            MaxRowHeight = 150.0,
            EnableSmoothScrolling = true,
            SmoothScrollingDuration = 200,
            EnableMemoryMonitoring = true,
            MemoryThresholdMB = 50.0,
            CellRecyclingPoolSize = 100
        };

        /// <summary>
        /// Pokročilá konfigurácia pre veľké datasety (10000+ riadkov)
        /// </summary>
        public static VirtualScrollingConfiguration Advanced => new()
        {
            EnableHorizontalVirtualization = true,
            EnableVerticalVirtualization = true,
            ColumnBufferSize = 8,
            RowBufferSize = 15,
            EnableVariableRowHeights = true,
            MaxRowHeight = 200.0,
            EnableSmoothScrolling = true,
            SmoothScrollingDuration = 250,
            EnableMemoryMonitoring = true,
            MemoryThresholdMB = 100.0,
            MemoryMonitoringInterval = 3000,
            EnableAutoCleanup = true,
            CellRecyclingPoolSize = 200,
            // ✅ NOVÉ optimalizácie
            MinRowsForActivation = 50,
            VisibleRowCount = 30,
            OptimizedRowHeight = 36.0,
            ScrollThrottleMs = 8,
            EnableLazyLoading = true,
            MaxCachedUIElements = 100,
            EnableSelectiveInvalidation = true,
            EnableDiagnostics = false
        };

        /// <summary>
        /// High-performance konfigurácia pre enterprise datasety (100000+ riadkov)
        /// </summary>
        public static VirtualScrollingConfiguration HighPerformance => new()
        {
            EnableHorizontalVirtualization = true,
            EnableVerticalVirtualization = true,
            ColumnBufferSize = 10,
            RowBufferSize = 20,
            MinColumnWidth = 30.0,
            MinRowHeight = 20.0,
            EnableVariableRowHeights = false, // Vypnuté pre max výkon
            EnableSmoothScrolling = false, // Vypnuté pre max výkon
            EnableMemoryMonitoring = true,
            MemoryThresholdMB = 200.0,
            MemoryMonitoringInterval = 2000,
            EnableAutoCleanup = true,
            CellRecyclingPoolSize = 500
        };

        #endregion

        #region Methods

        /// <summary>
        /// Validuje konfiguráciu a nastaví rozumné defaults
        /// </summary>
        public void Validate()
        {
            if (ColumnBufferSize < 0) ColumnBufferSize = 5;
            if (RowBufferSize < 0) RowBufferSize = 10;
            if (MinColumnWidth < 10) MinColumnWidth = 10;
            if (MinRowHeight < 15) MinRowHeight = 15;
            if (MaxRowHeight < MinRowHeight) MaxRowHeight = MinRowHeight * 2;
            if (SmoothScrollingDuration < 50) SmoothScrollingDuration = 50;
            if (MemoryThresholdMB < 10) MemoryThresholdMB = 10;
            if (MemoryMonitoringInterval < 1000) MemoryMonitoringInterval = 1000;
            if (CellRecyclingPoolSize < 10) CellRecyclingPoolSize = 10;
        }

        /// <summary>
        /// Vytvorí kópiu konfigurácie
        /// </summary>
        public VirtualScrollingConfiguration Clone()
        {
            return new VirtualScrollingConfiguration
            {
                EnableHorizontalVirtualization = EnableHorizontalVirtualization,
                EnableVerticalVirtualization = EnableVerticalVirtualization,
                ColumnBufferSize = ColumnBufferSize,
                RowBufferSize = RowBufferSize,
                MinColumnWidth = MinColumnWidth,
                MinRowHeight = MinRowHeight,
                EnableVariableRowHeights = EnableVariableRowHeights,
                MaxRowHeight = MaxRowHeight,
                EnableSmoothScrolling = EnableSmoothScrolling,
                SmoothScrollingDuration = SmoothScrollingDuration,
                MemoryThresholdMB = MemoryThresholdMB,
                MemoryMonitoringInterval = MemoryMonitoringInterval,
                EnableMemoryMonitoring = EnableMemoryMonitoring,
                EnableAutoCleanup = EnableAutoCleanup,
                CellRecyclingPoolSize = CellRecyclingPoolSize,
                // ✅ NOVÉ properties
                MinRowsForActivation = MinRowsForActivation,
                VisibleRowCount = VisibleRowCount,
                OptimizedRowHeight = OptimizedRowHeight,
                ScrollThrottleMs = ScrollThrottleMs,
                EnableLazyLoading = EnableLazyLoading,
                MaxCachedUIElements = MaxCachedUIElements,
                EnableViewportChangeEvents = EnableViewportChangeEvents,
                EnableSelectiveInvalidation = EnableSelectiveInvalidation,
                EnableDiagnostics = EnableDiagnostics
            };
        }

        /// <summary>
        /// ✅ NOVÉ: Celkový počet renderovaných riadkov (visible + buffer)
        /// </summary>
        public int TotalRenderedRows => VisibleRowCount + (RowBufferSize * 2);

        /// <summary>
        /// ✅ NOVÉ: Celková výška viewport-u
        /// </summary>
        public double ViewportHeight => VisibleRowCount * OptimizedRowHeight;

        /// <summary>
        /// ✅ NOVÉ: Vypočíta memory footprint reduction percentage
        /// </summary>
        public double CalculateMemoryReduction(int totalRows)
        {
            if (!EnableVerticalVirtualization || totalRows <= MinRowsForActivation)
                return 0.0;

            var renderedRows = Math.Min(TotalRenderedRows, totalRows);
            return Math.Round((1.0 - (double)renderedRows / totalRows) * 100, 2);
        }

        /// <summary>
        /// ✅ NOVÉ: Získa diagnostické informácie o konfigurácii
        /// </summary>
        public string GetDiagnosticInfo()
        {
            return $"VirtualScrolling: " +
                   $"Vertical={EnableVerticalVirtualization}, " +
                   $"Horizontal={EnableHorizontalVirtualization}, " +
                   $"Viewport: {VisibleRowCount}+{RowBufferSize*2} rows, " +
                   $"Height: {OptimizedRowHeight}px, " +
                   $"Throttle: {ScrollThrottleMs}ms, " +
                   $"Cache: {MaxCachedUIElements} elements, " +
                   $"LazyLoad: {EnableLazyLoading}, " +
                   $"SelectiveInvalidation: {EnableSelectiveInvalidation}";
        }

        /// <summary>
        /// ✅ NOVÉ: Kontroluje či konfigurácia vyžaduje virtualizáciu pre daný dataset
        /// </summary>
        public bool ShouldActivateVirtualization(int totalRows, int totalColumns = 0)
        {
            var verticalActivation = EnableVerticalVirtualization && totalRows >= MinRowsForActivation;
            var horizontalActivation = EnableHorizontalVirtualization && totalColumns >= 20; // Predpoklad
            
            return verticalActivation || horizontalActivation;
        }

        /// <summary>
        /// ✅ NOVÉ: Optimalizuje konfiguráciu na základe dataset size
        /// </summary>
        public VirtualScrollingConfiguration OptimizeForDataset(int totalRows, int totalColumns = 0)
        {
            var optimized = Clone();

            // Optimalizácia na základe veľkosti datasetu
            if (totalRows < 500)
            {
                // Malý dataset - vypni virtualizáciu
                optimized.EnableVerticalVirtualization = false;
                optimized.EnableHorizontalVirtualization = false;
            }
            else if (totalRows < 5000)
            {
                // Stredný dataset - základná virtualizácia
                optimized.VisibleRowCount = Math.Min(50, optimized.VisibleRowCount);
                optimized.RowBufferSize = Math.Min(10, optimized.RowBufferSize);
                optimized.ScrollThrottleMs = 16; // 60 FPS
            }
            else if (totalRows < 50000)
            {
                // Veľký dataset - optimalizovaná virtualizácia
                optimized.VisibleRowCount = Math.Min(30, optimized.VisibleRowCount);
                optimized.RowBufferSize = Math.Min(8, optimized.RowBufferSize);
                optimized.ScrollThrottleMs = 8; // 120 FPS
                optimized.EnableLazyLoading = true;
            }
            else
            {
                // Enterprise dataset - maximálna optimalizácia
                optimized.VisibleRowCount = Math.Min(25, optimized.VisibleRowCount);
                optimized.RowBufferSize = Math.Min(5, optimized.RowBufferSize);
                optimized.ScrollThrottleMs = 4; // 240 FPS
                optimized.EnableLazyLoading = true;
                optimized.EnableSelectiveInvalidation = true;
                optimized.EnableVariableRowHeights = false; // Vypni pre max výkon
                optimized.EnableSmoothScrolling = false; // Vypni animácie
            }

            return optimized;
        }

        #endregion
    }
}