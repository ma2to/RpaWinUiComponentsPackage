// Models/Cell/CellPosition.cs - ✅ NOVÝ: Pozícia bunky v DataGrid
using System;

namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Models.Cell
{
    /// <summary>
    /// Pozícia bunky v DataGrid - INTERNAL
    /// </summary>
    internal class CellPosition : IEquatable<CellPosition>
    {
        #region Properties

        public int Row { get; set; }
        public int Column { get; set; }
        public string ColumnName { get; set; } = string.Empty;

        #endregion

        #region Constructors

        /// <summary>
        /// Prázdny constructor
        /// </summary>
        public CellPosition()
        {
        }

        /// <summary>
        /// Constructor s row a column
        /// </summary>
        public CellPosition(int row, int column)
        {
            Row = row;
            Column = column;
        }

        /// <summary>
        /// Constructor s row, column a column name
        /// </summary>
        public CellPosition(int row, int column, string columnName)
        {
            Row = row;
            Column = column;
            ColumnName = columnName ?? string.Empty;
        }

        #endregion

        #region IEquatable

        public bool Equals(CellPosition? other)
        {
            if (other == null) return false;
            return Row == other.Row && Column == other.Column;
        }

        public override bool Equals(object? obj)
        {
            return Equals(obj as CellPosition);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Row, Column);
        }

        #endregion

        #region ToString

        public override string ToString()
        {
            return $"[{Row},{Column}]{ColumnName}";
        }

        #endregion
    }
}