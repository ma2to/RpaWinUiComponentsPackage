// Models/RowDisplayInfo.cs - ✅ NOVÝ: Model pre display informácie riadku
using System.Collections.Generic;

namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Models.Row
{
    /// <summary>
    /// Model pre display informácie riadku - INTERNAL
    /// Obsahuje všetky informácie potrebné pre zobrazenie riadku v UI
    /// </summary>
    internal class RowDisplayInfo
    {
        #region Properties

        /// <summary>
        /// Index riadku v DataGrid
        /// </summary>
        public int RowIndex { get; set; }

        /// <summary>
        /// Je riadok prázdny
        /// </summary>
        public bool IsEmpty { get; set; }

        /// <summary>
        /// Je riadok validný
        /// </summary>
        public bool IsValid { get; set; }

        /// <summary>
        /// Je riadok vybraný
        /// </summary>
        public bool IsSelected { get; set; }

        /// <summary>
        /// Je riadok focused
        /// </summary>
        public bool IsFocused { get; set; }

        /// <summary>
        /// Je riadok v alternate (zebra) farbe
        /// </summary>
        public bool IsAlternate { get; set; }

        /// <summary>
        /// Je riadok zebra row (alias pre IsAlternate)
        /// </summary>
        public bool IsZebraRow 
        { 
            get => IsAlternate; 
            set => IsAlternate = value; 
        }

        /// <summary>
        /// Je riadok párny non-empty row
        /// </summary>
        public bool IsEvenNonEmptyRow { get; set; }

        /// <summary>
        /// Dátové bunky riadku (alias pre CellData)
        /// </summary>
        public Dictionary<string, object?> Data 
        { 
            get => CellData; 
            set => CellData = value; 
        }

        /// <summary>
        /// Je riadok označený checkbox-om (ak je checkbox column povolený)
        /// </summary>
        public bool IsChecked { get; set; }

        /// <summary>
        /// Má riadok validačné chyby
        /// </summary>
        public bool HasValidationErrors { get; set; }

        /// <summary>
        /// Počet validačných chýb v riadku
        /// </summary>
        public int ValidationErrorCount { get; set; }

        /// <summary>
        /// Zoznam validačných chýb
        /// </summary>
        public List<string> ValidationErrors { get; set; } = new();

        /// <summary>
        /// Dátové bunky riadku
        /// </summary>
        public Dictionary<string, object?> CellData { get; set; } = new();

        /// <summary>
        /// Pozícia riadku pre virtual scrolling
        /// </summary>
        public double VerticalOffset { get; set; }

        /// <summary>
        /// Výška riadku (pre variable row heights)
        /// </summary>
        public double RowHeight { get; set; } = 30.0;

        /// <summary>
        /// Je riadok viditeľný (virtual scrolling)
        /// </summary>
        public bool IsVisible { get; set; } = true;

        #endregion

        #region Constructors

        /// <summary>
        /// Vytvorí prázdny RowDisplayInfo
        /// </summary>
        public RowDisplayInfo()
        {
        }

        /// <summary>
        /// Vytvorí RowDisplayInfo s indexom
        /// </summary>
        public RowDisplayInfo(int rowIndex)
        {
            RowIndex = rowIndex;
        }

        /// <summary>
        /// Vytvorí RowDisplayInfo s dátami
        /// </summary>
        public RowDisplayInfo(int rowIndex, Dictionary<string, object?> cellData)
        {
            RowIndex = rowIndex;
            CellData = cellData ?? new();
            IsEmpty = cellData == null || cellData.Count == 0;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Získa hodnotu bunky
        /// </summary>
        public object? GetCellValue(string columnName)
        {
            return CellData.TryGetValue(columnName, out var value) ? value : null;
        }

        /// <summary>
        /// Nastaví hodnotu bunky
        /// </summary>
        public void SetCellValue(string columnName, object? value)
        {
            CellData[columnName] = value;
        }

        /// <summary>
        /// Pridá validačnú chybu
        /// </summary>
        public void AddValidationError(string error)
        {
            if (!ValidationErrors.Contains(error))
            {
                ValidationErrors.Add(error);
                ValidationErrorCount = ValidationErrors.Count;
                HasValidationErrors = ValidationErrorCount > 0;
                IsValid = !HasValidationErrors;
            }
        }

        /// <summary>
        /// Vymaže všetky validačné chyby
        /// </summary>
        public void ClearValidationErrors()
        {
            ValidationErrors.Clear();
            ValidationErrorCount = 0;
            HasValidationErrors = false;
            IsValid = true;
        }

        /// <summary>
        /// Skontroluje či je riadok prázdny
        /// </summary>
        public void UpdateEmptyStatus(List<string>? excludeColumns = null)
        {
            excludeColumns ??= new List<string> { "DeleteRows", "ValidAlerts", "CheckBoxState" };

            var nonSpecialCells = CellData.Where(kvp => !excludeColumns.Contains(kvp.Key));
            IsEmpty = !nonSpecialCells.Any(kvp => 
                kvp.Value != null && !string.IsNullOrWhiteSpace(kvp.Value.ToString()));
        }

        /// <summary>
        /// CSS class alebo style name pre tento riadok
        /// </summary>
        public string GetRowStyleClass()
        {
            if (IsEmpty) return "empty-row";
            if (IsZebraRow) return "zebra-row";
            return "normal-row";
        }

        /// <summary>
        /// Klonuje RowDisplayInfo
        /// </summary>
        public RowDisplayInfo Clone()
        {
            return new RowDisplayInfo
            {
                RowIndex = RowIndex,
                IsEmpty = IsEmpty,
                IsValid = IsValid,
                IsSelected = IsSelected,
                IsFocused = IsFocused,
                IsAlternate = IsAlternate,
                IsChecked = IsChecked,
                HasValidationErrors = HasValidationErrors,
                ValidationErrorCount = ValidationErrorCount,
                ValidationErrors = new List<string>(ValidationErrors),
                CellData = new Dictionary<string, object?>(CellData),
                VerticalOffset = VerticalOffset,
                RowHeight = RowHeight,
                IsVisible = IsVisible
            };
        }

        #endregion

        #region Factory Methods

        /// <summary>
        /// Vytvorí prázdny riadok
        /// </summary>
        public static RowDisplayInfo CreateEmpty(int rowIndex, List<string> columnNames)
        {
            var cellData = new Dictionary<string, object?>();
            foreach (var columnName in columnNames)
            {
                cellData[columnName] = null;
            }

            return new RowDisplayInfo(rowIndex, cellData)
            {
                IsEmpty = true,
                IsValid = true
            };
        }

        /// <summary>
        /// Vytvorí z row data
        /// </summary>
        public static RowDisplayInfo FromRowData(int rowIndex, Dictionary<string, object?> rowData)
        {
            var info = new RowDisplayInfo(rowIndex, rowData);
            info.UpdateEmptyStatus();
            return info;
        }

        #endregion

        #region ToString

        public override string ToString()
        {
            var status = IsEmpty ? "Empty" : IsValid ? "Valid" : "Invalid";
            var selection = IsSelected ? "Selected" : IsFocused ? "Focused" : "";
            var validation = HasValidationErrors ? $"Errors:{ValidationErrorCount}" : "";
            
            return $"Row[{RowIndex}] {status} {selection} {validation}".Trim();
        }

        #endregion
    }
}