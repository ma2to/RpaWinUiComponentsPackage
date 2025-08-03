// Models/CellRange.cs - ✅ INTERNAL Cell range models pre advanced copy/paste
using System;
using System.Collections.Generic;
using System.Linq;

namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Models.Cell
{
    /// <summary>
    /// Reprezentuje range buniek pre copy/paste operations - INTERNAL
    /// </summary>
    internal class CellRange
    {
        #region Properties

        /// <summary>
        /// Počiatočný riadok (0-based)
        /// </summary>
        public int StartRow { get; set; }

        /// <summary>
        /// Počiatočný stĺpec (0-based)
        /// </summary>
        public int StartColumn { get; set; }

        /// <summary>
        /// Koncový riadok (0-based, inclusive)
        /// </summary>
        public int EndRow { get; set; }

        /// <summary>
        /// Koncový stĺpec (0-based, inclusive)
        /// </summary>
        public int EndColumn { get; set; }

        /// <summary>
        /// Počet riadkov v range
        /// </summary>
        public int RowCount => EndRow - StartRow + 1;

        /// <summary>
        /// Počet stĺpcov v range
        /// </summary>
        public int ColumnCount => EndColumn - StartColumn + 1;

        /// <summary>
        /// Celkový počet buniek v range
        /// </summary>
        public int CellCount => RowCount * ColumnCount;

        /// <summary>
        /// Či je range validný
        /// </summary>
        public bool IsValid => StartRow >= 0 && StartColumn >= 0 && 
                              EndRow >= StartRow && EndColumn >= StartColumn;

        #endregion

        #region Constructors

        /// <summary>
        /// Vytvorí range z pozícií
        /// </summary>
        public CellRange(int startRow, int startColumn, int endRow, int endColumn)
        {
            StartRow = Math.Min(startRow, endRow);
            StartColumn = Math.Min(startColumn, endColumn);
            EndRow = Math.Max(startRow, endRow);
            EndColumn = Math.Max(startColumn, endColumn);
        }

        /// <summary>
        /// Vytvorí range pre jednu bunku
        /// </summary>
        public CellRange(int row, int column) : this(row, column, row, column)
        {
        }

        #endregion

        #region Static Factory Methods

        /// <summary>
        /// Vytvorí range z cell positions
        /// </summary>
        public static CellRange FromPositions(List<CellPosition> positions)
        {
            if (!positions.Any())
                return new CellRange(0, 0, 0, 0);

            var minRow = positions.Min(p => p.Row);
            var maxRow = positions.Max(p => p.Row);
            var minCol = positions.Min(p => p.Column);
            var maxCol = positions.Max(p => p.Column);

            return new CellRange(minRow, minCol, maxRow, maxCol);
        }

        /// <summary>
        /// Vytvorí range z selection positions
        /// </summary>
        public static CellRange FromSelection(List<(int Row, int Column)> selection)
        {
            if (!selection.Any())
                return new CellRange(0, 0, 0, 0);

            var minRow = selection.Min(s => s.Row);
            var maxRow = selection.Max(s => s.Row);
            var minCol = selection.Min(s => s.Column);
            var maxCol = selection.Max(s => s.Column);

            return new CellRange(minRow, minCol, maxRow, maxCol);
        }

        #endregion

        #region Range Operations

        /// <summary>
        /// Skontroluje či range obsahuje pozíciu
        /// </summary>
        public bool Contains(int row, int column)
        {
            return row >= StartRow && row <= EndRow && 
                   column >= StartColumn && column <= EndColumn;
        }

        /// <summary>
        /// Skontroluje či range obsahuje cell position
        /// </summary>
        public bool Contains(CellPosition position)
        {
            return Contains(position.Row, position.Column);
        }

        /// <summary>
        /// Alias pre Contains - pre ExtendedSelectionMode compatibility
        /// </summary>
        public bool ContainsCell(int row, int column)
        {
            return Contains(row, column);
        }

        /// <summary>
        /// Alias pre CellCount - pre ExtendedSelectionMode compatibility
        /// </summary>
        public int GetCellCount()
        {
            return CellCount;
        }

        /// <summary>
        /// Získa všetky pozície v range
        /// </summary>
        public List<CellPosition> GetAllPositions()
        {
            var positions = new List<CellPosition>();

            for (int row = StartRow; row <= EndRow; row++)
            {
                for (int col = StartColumn; col <= EndColumn; col++)
                {
                    positions.Add(new CellPosition 
                    { 
                        Row = row, 
                        Column = col,
                        ColumnName = $"Column_{col}" // Fallback name
                    });
                }
            }

            return positions;
        }

        /// <summary>
        /// Expanduje range o zadaný počet buniek
        /// </summary>
        public CellRange Expand(int rows, int columns)
        {
            return new CellRange(
                Math.Max(0, StartRow - rows),
                Math.Max(0, StartColumn - columns),
                EndRow + rows,
                EndColumn + columns
            );
        }

        /// <summary>
        /// Presunie range o offset
        /// </summary>
        public CellRange Offset(int rowOffset, int columnOffset)
        {
            return new CellRange(
                StartRow + rowOffset,
                StartColumn + columnOffset,
                EndRow + rowOffset,
                EndColumn + columnOffset
            );
        }

        /// <summary>
        /// Intersection s iným range
        /// </summary>
        public CellRange? Intersect(CellRange other)
        {
            var startRow = Math.Max(StartRow, other.StartRow);
            var startCol = Math.Max(StartColumn, other.StartColumn);
            var endRow = Math.Min(EndRow, other.EndRow);
            var endCol = Math.Min(EndColumn, other.EndColumn);

            if (startRow <= endRow && startCol <= endCol)
            {
                return new CellRange(startRow, startCol, endRow, endCol);
            }

            return null; // No intersection
        }

        /// <summary>
        /// Union s iným range
        /// </summary>
        public CellRange Union(CellRange other)
        {
            return new CellRange(
                Math.Min(StartRow, other.StartRow),
                Math.Min(StartColumn, other.StartColumn),
                Math.Max(EndRow, other.EndRow),
                Math.Max(EndColumn, other.EndColumn)
            );
        }

        #endregion

        #region Diagnostics

        /// <summary>
        /// String reprezentácia range
        /// </summary>
        public override string ToString()
        {
            if (StartRow == EndRow && StartColumn == EndColumn)
            {
                return $"[{StartRow},{StartColumn}]";
            }
            return $"[{StartRow},{StartColumn}]-[{EndRow},{EndColumn}] ({RowCount}x{ColumnCount})";
        }

        /// <summary>
        /// Diagnostic info
        /// </summary>
        public string GetDiagnosticInfo()
        {
            return $"Range {ToString()} - Cells: {CellCount}, Valid: {IsValid}";
        }

        #endregion

        #region Equality

        public override bool Equals(object? obj)
        {
            if (obj is CellRange other)
            {
                return StartRow == other.StartRow &&
                       StartColumn == other.StartColumn &&
                       EndRow == other.EndRow &&
                       EndColumn == other.EndColumn;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(StartRow, StartColumn, EndRow, EndColumn);
        }

        #endregion
    }

    /// <summary>
    /// Range clipboard data pre copy/paste operations - INTERNAL
    /// </summary>
    internal class RangeClipboardData
    {
        #region Properties

        /// <summary>
        /// Source range
        /// </summary>
        public CellRange SourceRange { get; set; } = new(0, 0, 0, 0);

        /// <summary>
        /// Data pre každú bunku v range
        /// </summary>
        public Dictionary<string, object?> CellData { get; set; } = new();

        /// <summary>
        /// Column names pre range
        /// </summary>
        public List<string> ColumnNames { get; set; } = new();

        /// <summary>
        /// Timestamp kedy boli data skopírované
        /// </summary>
        public DateTime CopiedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Operation type (Copy, Cut)
        /// </summary>
        public ClipboardOperation Operation { get; set; } = ClipboardOperation.Copy;

        /// <summary>
        /// Source instance ID
        /// </summary>
        public string SourceInstanceId { get; set; } = string.Empty;

        #endregion

        #region Data Access

        /// <summary>
        /// Získa hodnotu pre pozíciu
        /// </summary>
        public object? GetCellValue(int row, int column)
        {
            var key = $"{row}_{column}";
            return CellData.TryGetValue(key, out var value) ? value : null;
        }

        /// <summary>
        /// Nastaví hodnotu pre pozíciu
        /// </summary>
        public void SetCellValue(int row, int column, object? value)
        {
            var key = $"{row}_{column}";
            CellData[key] = value;
        }

        /// <summary>
        /// Skontroluje či má data pre pozíciu
        /// </summary>
        public bool HasDataForPosition(int row, int column)
        {
            var key = $"{row}_{column}";
            return CellData.ContainsKey(key);
        }

        #endregion

        #region Conversion Methods

        /// <summary>
        /// Konvertuje na CSV format
        /// </summary>
        public string ToCsv()
        {
            var lines = new List<string>();

            for (int row = SourceRange.StartRow; row <= SourceRange.EndRow; row++)
            {
                var rowValues = new List<string>();
                for (int col = SourceRange.StartColumn; col <= SourceRange.EndColumn; col++)
                {
                    var value = GetCellValue(row, col)?.ToString() ?? string.Empty;
                    
                    // Escape CSV special characters
                    if (value.Contains(",") || value.Contains("\"") || value.Contains("\n"))
                    {
                        value = $"\"{value.Replace("\"", "\"\"")}\"";
                    }
                    
                    rowValues.Add(value);
                }
                lines.Add(string.Join(",", rowValues));
            }

            return string.Join("\n", lines);
        }

        /// <summary>
        /// Konvertuje na tab-separated format
        /// </summary>
        public string ToTabSeparated()
        {
            var lines = new List<string>();

            for (int row = SourceRange.StartRow; row <= SourceRange.EndRow; row++)
            {
                var rowValues = new List<string>();
                for (int col = SourceRange.StartColumn; col <= SourceRange.EndColumn; col++)
                {
                    var value = GetCellValue(row, col)?.ToString() ?? string.Empty;
                    rowValues.Add(value);
                }
                lines.Add(string.Join("\t", rowValues));
            }

            return string.Join("\n", lines);
        }

        #endregion

        #region Diagnostics

        /// <summary>
        /// Diagnostic info
        /// </summary>
        public string GetDiagnosticInfo()
        {
            return $"RangeClipboard - Range: {SourceRange}, " +
                   $"DataCount: {CellData.Count}, Operation: {Operation}, " +
                   $"Age: {(DateTime.UtcNow - CopiedAt).TotalSeconds:F1}s";
        }

        #endregion
    }

    /// <summary>
    /// Clipboard operation types - INTERNAL
    /// </summary>
    internal enum ClipboardOperation
    {
        Copy,
        Cut
    }
}