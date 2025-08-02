// Models/DragSelectionState.cs - ✅ INTERNAL Drag Selection state management
using System;
using Windows.Foundation;

namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Models
{
    /// <summary>
    /// Internal model pre spravovanie drag selection state - INTERNAL
    /// </summary>
    internal class DragSelectionState
    {
        #region Properties

        /// <summary>
        /// Či prebieha drag operácia
        /// </summary>
        public bool IsDragging { get; set; }

        /// <summary>
        /// Počiatočná pozícia drag operácie
        /// </summary>
        public Point StartPoint { get; set; }

        /// <summary>
        /// Aktuálna pozícia počas drag operácie
        /// </summary>
        public Point CurrentPoint { get; set; }

        /// <summary>
        /// Počiatočná bunka drag operácie
        /// </summary>
        public CellPosition? StartCell { get; set; }

        /// <summary>
        /// Aktuálna bunka počas drag operácie
        /// </summary>
        public CellPosition? CurrentCell { get; set; }

        /// <summary>
        /// Rectangle pre visualization
        /// </summary>
        public Rect SelectionRectangle
        {
            get
            {
                if (!IsDragging) return Rect.Empty;

                var x = Math.Min(StartPoint.X, CurrentPoint.X);
                var y = Math.Min(StartPoint.Y, CurrentPoint.Y);
                var width = Math.Abs(CurrentPoint.X - StartPoint.X);
                var height = Math.Abs(CurrentPoint.Y - StartPoint.Y);

                return new Rect(x, y, width, height);
            }
        }

        /// <summary>
        /// Či je drag operácia dostatočne veľká na započatie selection
        /// </summary>
        public bool IsValidDragDistance
        {
            get
            {
                if (!IsDragging) return false;

                var deltaX = Math.Abs(CurrentPoint.X - StartPoint.X);
                var deltaY = Math.Abs(CurrentPoint.Y - StartPoint.Y);

                // Minimálna vzdialenosť pre drag (5 pixelov)
                return deltaX > 5 || deltaY > 5;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Začne drag operáciu
        /// </summary>
        public void StartDrag(Point startPoint, CellPosition startCell)
        {
            IsDragging = true;
            StartPoint = startPoint;
            CurrentPoint = startPoint;
            StartCell = startCell;
            CurrentCell = startCell;
        }

        /// <summary>
        /// Aktualizuje drag operáciu
        /// </summary>
        public void UpdateDrag(Point currentPoint, CellPosition? currentCell)
        {
            if (!IsDragging) return;

            CurrentPoint = currentPoint;
            CurrentCell = currentCell;
        }

        /// <summary>
        /// Ukončí drag operáciu
        /// </summary>
        public void EndDrag()
        {
            IsDragging = false;
            StartPoint = new Point();
            CurrentPoint = new Point();
            StartCell = null;
            CurrentCell = null;
        }

        /// <summary>
        /// Získa rozsah buniek pre selection
        /// </summary>
        public (int StartRow, int StartColumn, int EndRow, int EndColumn) GetSelectionRange()
        {
            if (StartCell == null || CurrentCell == null)
                return (0, 0, 0, 0);

            var startRow = Math.Min(StartCell.Row, CurrentCell.Row);
            var endRow = Math.Max(StartCell.Row, CurrentCell.Row);
            var startCol = Math.Min(StartCell.Column, CurrentCell.Column);
            var endCol = Math.Max(StartCell.Column, CurrentCell.Column);

            return (startRow, startCol, endRow, endCol);
        }

        /// <summary>
        /// Diagnostické informácie
        /// </summary>
        public string GetDiagnosticInfo()
        {
            if (!IsDragging)
                return "Not dragging";

            var (startRow, startCol, endRow, endCol) = GetSelectionRange();
            return $"Dragging: [{startRow},{startCol}] → [{endRow},{endCol}], " +
                   $"Rect: {SelectionRectangle.Width:F0}x{SelectionRectangle.Height:F0}";
        }

        #endregion
    }
}