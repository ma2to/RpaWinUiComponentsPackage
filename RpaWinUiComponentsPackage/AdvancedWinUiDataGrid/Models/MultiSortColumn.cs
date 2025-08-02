// Models/MultiSortColumn.cs - ✅ PUBLIC model pre Multi-Sort funkcionalitu
using System;

namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Models
{
    /// <summary>
    /// Reprezentuje stĺpec s multi-sort informáciami - ✅ PUBLIC API
    /// </summary>
    public class MultiSortColumn
    {
        /// <summary>
        /// Názov stĺpca
        /// </summary>
        public string ColumnName { get; set; } = string.Empty;

        /// <summary>
        /// Smer sortovania
        /// </summary>
        public SortDirection Direction { get; set; } = SortDirection.None;

        /// <summary>
        /// Priorita sortovania (1 = najvyššia, 2, 3...)
        /// </summary>
        public int Priority { get; set; } = 1;

        /// <summary>
        /// Čas pridania do Multi-Sort (pre tracking poradia)
        /// </summary>
        public DateTime AddedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Vytvorí nový MultiSortColumn
        /// </summary>
        public MultiSortColumn() { }

        /// <summary>
        /// Vytvorí nový MultiSortColumn s parametrami
        /// </summary>
        public MultiSortColumn(string columnName, SortDirection direction, int priority = 1)
        {
            ColumnName = columnName ?? throw new ArgumentNullException(nameof(columnName));
            Direction = direction;
            Priority = priority;
            AddedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Získa display text pre UI (napr. "Name ↑ (1)")
        /// </summary>
        public string GetDisplayText()
        {
            var arrow = Direction switch
            {
                SortDirection.Ascending => "↑",
                SortDirection.Descending => "↓",
                _ => ""
            };

            return $"{ColumnName} {arrow} ({Priority})";
        }

        /// <summary>
        /// Získa symbol pre UI indikátor
        /// </summary>
        public string GetSortSymbol()
        {
            return Direction switch
            {
                SortDirection.Ascending => "▲",
                SortDirection.Descending => "▼",
                _ => ""
            };
        }

        /// <summary>
        /// Kopíruje MultiSortColumn
        /// </summary>
        public MultiSortColumn Clone()
        {
            return new MultiSortColumn(ColumnName, Direction, Priority)
            {
                AddedAt = AddedAt
            };
        }

        public override string ToString()
        {
            return $"{ColumnName} {Direction} (Priority: {Priority})";
        }

        public override bool Equals(object? obj)
        {
            return obj is MultiSortColumn other && 
                   ColumnName.Equals(other.ColumnName, StringComparison.OrdinalIgnoreCase);
        }

        public override int GetHashCode()
        {
            return ColumnName.GetHashCode(StringComparison.OrdinalIgnoreCase);
        }
    }
}