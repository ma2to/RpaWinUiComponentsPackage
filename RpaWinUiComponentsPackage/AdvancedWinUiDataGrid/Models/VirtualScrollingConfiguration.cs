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
            CellRecyclingPoolSize = 200
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
                CellRecyclingPoolSize = CellRecyclingPoolSize
            };
        }

        #endregion
    }
}