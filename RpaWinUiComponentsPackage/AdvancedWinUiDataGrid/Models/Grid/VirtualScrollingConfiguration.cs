// Models/Grid/VirtualScrollingConfiguration.cs - ✅ NOVÉ: Virtual Scrolling Configuration
using System;

namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Models.Grid
{
    /// <summary>
    /// ✅ NOVÉ: Konfigurácia pre virtual scrolling
    /// </summary>
    public class VirtualScrollingConfiguration
    {
        /// <summary>
        /// Či je virtual scrolling povolený
        /// </summary>
        public bool IsEnabled { get; set; } = true;

        /// <summary>
        /// Počet viditeľných riadkov v viewporte
        /// </summary>
        public int VisibleRows { get; set; } = 50;

        /// <summary>
        /// Buffer size - koľko riadkov navyše renderovať mimo viewport
        /// </summary>
        public int BufferSize { get; set; } = 10;

        /// <summary>
        /// Minimálny počet riadkov na aktiváciu virtual scrolling
        /// </summary>
        public int MinRowsForVirtualization { get; set; } = 100;

        /// <summary>
        /// Výška jedného riadku (fixed pre virtual scrolling)
        /// </summary>
        public double RowHeight { get; set; } = 36.0;

        /// <summary>
        /// Či sa má použiť smooth scrolling
        /// </summary>
        public bool EnableSmoothScrolling { get; set; } = true;

        /// <summary>
        /// Delay pre scroll throttling (ms)
        /// </summary>
        public int ScrollThrottleDelay { get; set; } = 16; // ~60fps

        /// <summary>
        /// Validuje konfiguráciu
        /// </summary>
        public void Validate()
        {
            if (VisibleRows <= 0)
                throw new ArgumentException("VisibleRows musí byť > 0");

            if (BufferSize < 0)
                throw new ArgumentException("BufferSize musí byť >= 0");

            if (MinRowsForVirtualization <= 0)
                throw new ArgumentException("MinRowsForVirtualization musí byť > 0");

            if (RowHeight <= 0)
                throw new ArgumentException("RowHeight musí byť > 0");

            if (ScrollThrottleDelay < 0)
                throw new ArgumentException("ScrollThrottleDelay musí byť >= 0");
        }

        /// <summary>
        /// Vytvorí default konfiguráciu
        /// </summary>
        public static VirtualScrollingConfiguration Default => new()
        {
            IsEnabled = true,
            VisibleRows = 50,
            BufferSize = 10,
            MinRowsForVirtualization = 100,
            RowHeight = 36.0,
            EnableSmoothScrolling = true,
            ScrollThrottleDelay = 16
        };

        /// <summary>
        /// Vytvorí kópiu konfigurácie
        /// </summary>
        public VirtualScrollingConfiguration Clone()
        {
            return new VirtualScrollingConfiguration
            {
                IsEnabled = IsEnabled,
                VisibleRows = VisibleRows,
                BufferSize = BufferSize,
                MinRowsForVirtualization = MinRowsForVirtualization,
                RowHeight = RowHeight,
                EnableSmoothScrolling = EnableSmoothScrolling,
                ScrollThrottleDelay = ScrollThrottleDelay
            };
        }
    }

    /// <summary>
    /// ✅ NOVÉ: Virtual Scrolling viewport info
    /// </summary>
    public class VirtualScrollingViewport
    {
        /// <summary>
        /// Index prvého viditeľného riadku
        /// </summary>
        public int FirstVisibleRowIndex { get; set; }

        /// <summary>
        /// Index posledného viditeľného riadku
        /// </summary>
        public int LastVisibleRowIndex { get; set; }

        /// <summary>
        /// Index prvého renderovaného riadku (s bufferom)
        /// </summary>
        public int FirstRenderedRowIndex { get; set; }

        /// <summary>
        /// Index posledného renderovaného riadku (s bufferom)
        /// </summary>
        public int LastRenderedRowIndex { get; set; }

        /// <summary>
        /// Aktuálna scroll pozícia
        /// </summary>
        public double ScrollPosition { get; set; }

        /// <summary>
        /// Celková výška všetkých riadkov
        /// </summary>
        public double TotalHeight { get; set; }

        /// <summary>
        /// Výška viewport-u
        /// </summary>
        public double ViewportHeight { get; set; }

        /// <summary>
        /// Počet celkových riadkov
        /// </summary>
        public int TotalRowCount { get; set; }

        /// <summary>
        /// Počet renderovaných riadkov
        /// </summary>
        public int RenderedRowCount => LastRenderedRowIndex - FirstRenderedRowIndex + 1;

        /// <summary>
        /// Či je viewport validný
        /// </summary>
        public bool IsValid => FirstVisibleRowIndex >= 0 && LastVisibleRowIndex >= FirstVisibleRowIndex;
    }
}