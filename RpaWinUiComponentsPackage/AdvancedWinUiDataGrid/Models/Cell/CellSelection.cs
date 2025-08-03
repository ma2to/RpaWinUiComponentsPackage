// Models/CellSelection.cs - ✅ NOVÝ: Model pre jednu cell selection
using System;

namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Models.Cell
{
    /// <summary>
    /// Model pre jednu cell selection - INTERNAL
    /// Reprezentuje jednu vybranú bunku v DataGrid
    /// </summary>
    internal class CellSelection : IEquatable<CellSelection>
    {
        #region Properties

        /// <summary>
        /// Index riadku
        /// </summary>
        public int RowIndex { get; set; }

        /// <summary>
        /// Index stĺpca
        /// </summary>
        public int ColumnIndex { get; set; }

        /// <summary>
        /// Názov stĺpca
        /// </summary>
        public string ColumnName { get; set; } = string.Empty;

        /// <summary>
        /// Hodnota bunky
        /// </summary>
        public object? Value { get; set; }

        /// <summary>
        /// Je bunka focused
        /// </summary>
        public bool IsFocused { get; set; }

        /// <summary>
        /// Je bunka v copied stave
        /// </summary>
        public bool IsCopied { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Vytvorí prázdnu cell selection
        /// </summary>
        public CellSelection()
        {
        }

        /// <summary>
        /// Vytvorí cell selection s pozíciou
        /// </summary>
        public CellSelection(int rowIndex, int columnIndex, string columnName)
        {
            RowIndex = rowIndex;
            ColumnIndex = columnIndex;
            ColumnName = columnName;
        }

        /// <summary>
        /// Vytvorí cell selection s pozíciou a hodnotou
        /// </summary>
        public CellSelection(int rowIndex, int columnIndex, string columnName, object? value)
        {
            RowIndex = rowIndex;
            ColumnIndex = columnIndex;
            ColumnName = columnName;
            Value = value;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Konvertuje na CellPosition
        /// </summary>
        public CellPosition ToCellPosition()
        {
            return new CellPosition
            {
                Row = RowIndex,
                Column = ColumnIndex,
                ColumnName = ColumnName
            };
        }

        /// <summary>
        /// Vytvorí CellSelection z CellPosition
        /// </summary>
        public static CellSelection FromCellPosition(CellPosition position, object? value = null)
        {
            return new CellSelection
            {
                RowIndex = position.Row,
                ColumnIndex = position.Column,
                ColumnName = position.ColumnName,
                Value = value
            };
        }

        #endregion

        #region IEquatable

        public bool Equals(CellSelection? other)
        {
            if (other == null) return false;
            return RowIndex == other.RowIndex && ColumnIndex == other.ColumnIndex;
        }

        public override bool Equals(object? obj)
        {
            return Equals(obj as CellSelection);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(RowIndex, ColumnIndex);
        }

        #endregion

        #region ToString

        public override string ToString()
        {
            return $"CellSelection[{RowIndex},{ColumnIndex}]{ColumnName}={Value}";
        }

        #endregion
    }
}