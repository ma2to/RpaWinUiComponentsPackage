// Models/DataGridColorConfig.cs - ✅ NOVÝ SÚBOR pre individual color settings
using Microsoft.UI;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Windows.UI;

namespace RpaWinUiComponents.AdvancedWinUiDataGrid
{
    /// <summary>
    /// Konfigurácia farieb pre DataGrid - nastavuje sa pri inicializácii - ✅ PUBLIC API
    /// Umožňuje nastavovať jednotlivé farby namiesto predpripravených themes
    /// </summary>
    public class DataGridColorConfig : INotifyPropertyChanged
    {
        #region Private Fields

        private Color? _cellBackgroundColor;
        private Color? _cellBorderColor;
        private Color? _cellTextColor;
        private Color? _headerBackgroundColor;
        private Color? _headerTextColor;
        private Color? _validationErrorColor;
        private Color? _selectionColor;
        private Color? _alternateRowColor;
        private Color? _hoverColor;
        private Color? _editingCellColor;

        #endregion

        #region ✅ Optional Color Properties (null = použije sa default)

        /// <summary>
        /// Farba pozadia bunky (null = default)
        /// </summary>
        public Color? CellBackgroundColor
        {
            get => _cellBackgroundColor;
            set => SetProperty(ref _cellBackgroundColor, value);
        }

        /// <summary>
        /// Farba okraja bunky (null = default)
        /// </summary>
        public Color? CellBorderColor
        {
            get => _cellBorderColor;
            set => SetProperty(ref _cellBorderColor, value);
        }

        /// <summary>
        /// Farba textu v bunke (null = default)
        /// </summary>
        public Color? CellTextColor
        {
            get => _cellTextColor;
            set => SetProperty(ref _cellTextColor, value);
        }

        /// <summary>
        /// Farba pozadia header-u (null = default)
        /// </summary>
        public Color? HeaderBackgroundColor
        {
            get => _headerBackgroundColor;
            set => SetProperty(ref _headerBackgroundColor, value);
        }

        /// <summary>
        /// Farba textu header-u (null = default)
        /// </summary>
        public Color? HeaderTextColor
        {
            get => _headerTextColor;
            set => SetProperty(ref _headerTextColor, value);
        }

        /// <summary>
        /// Farba validačných chýb - červené orámovanie (null = default red)
        /// </summary>
        public Color? ValidationErrorColor
        {
            get => _validationErrorColor;
            set => SetProperty(ref _validationErrorColor, value);
        }

        /// <summary>
        /// Farba označenia buniek (null = default)
        /// </summary>
        public Color? SelectionColor
        {
            get => _selectionColor;
            set => SetProperty(ref _selectionColor, value);
        }

        /// <summary>
        /// Farba alternatívnych riadkov - zebra effect (null = žiadny effect)
        /// </summary>
        public Color? AlternateRowColor
        {
            get => _alternateRowColor;
            set => SetProperty(ref _alternateRowColor, value);
        }

        /// <summary>
        /// Farba pri hover nad bunkou (null = default)
        /// </summary>
        public Color? HoverColor
        {
            get => _hoverColor;
            set => SetProperty(ref _hoverColor, value);
        }

        /// <summary>
        /// Farba bunky ktorá sa edituje (null = default)
        /// </summary>
        public Color? EditingCellColor
        {
            get => _editingCellColor;
            set => SetProperty(ref _editingCellColor, value);
        }

        #endregion

        #region ✅ Default Values (používajú sa ak nie je nastavená custom farba)

        /// <summary>
        /// Získa default farbu pre CellBackground
        /// </summary>
        public static Color GetDefaultCellBackgroundColor() => Colors.White;

        /// <summary>
        /// Získa default farbu pre CellBorder
        /// </summary>
        public static Color GetDefaultCellBorderColor() => Colors.LightGray;

        /// <summary>
        /// Získa default farbu pre CellText
        /// </summary>
        public static Color GetDefaultCellTextColor() => Colors.Black;

        /// <summary>
        /// Získa default farbu pre HeaderBackground
        /// </summary>
        public static Color GetDefaultHeaderBackgroundColor() => Color.FromArgb(255, 240, 240, 240);

        /// <summary>
        /// Získa default farbu pre HeaderText
        /// </summary>
        public static Color GetDefaultHeaderTextColor() => Colors.Black;

        /// <summary>
        /// Získa default farbu pre ValidationError
        /// </summary>
        public static Color GetDefaultValidationErrorColor() => Colors.Red;

        /// <summary>
        /// Získa default farbu pre Selection
        /// </summary>
        public static Color GetDefaultSelectionColor() => Color.FromArgb(100, 0, 120, 215);

        /// <summary>
        /// Získa default farbu pre AlternateRow
        /// </summary>
        public static Color GetDefaultAlternateRowColor() => Color.FromArgb(20, 0, 0, 0);

        /// <summary>
        /// Získa default farbu pre Hover
        /// </summary>
        public static Color GetDefaultHoverColor() => Color.FromArgb(50, 0, 120, 215);

        /// <summary>
        /// Získa default farbu pre EditingCell
        /// </summary>
        public static Color GetDefaultEditingCellColor() => Color.FromArgb(30, 255, 255, 0);

        #endregion

        #region ✅ Resolved Colors (vráti custom color alebo default)

        /// <summary>
        /// Vráti skutočnú farbu pre CellBackground (custom alebo default)
        /// </summary>
        public Color ResolvedCellBackgroundColor => CellBackgroundColor ?? GetDefaultCellBackgroundColor();

        /// <summary>
        /// Vráti skutočnú farbu pre CellBorder (custom alebo default)
        /// </summary>
        public Color ResolvedCellBorderColor => CellBorderColor ?? GetDefaultCellBorderColor();

        /// <summary>
        /// Vráti skutočnú farbu pre CellText (custom alebo default)
        /// </summary>
        public Color ResolvedCellTextColor => CellTextColor ?? GetDefaultCellTextColor();

        /// <summary>
        /// Vráti skutočnú farbu pre HeaderBackground (custom alebo default)
        /// </summary>
        public Color ResolvedHeaderBackgroundColor => HeaderBackgroundColor ?? GetDefaultHeaderBackgroundColor();

        /// <summary>
        /// Vráti skutočnú farbu pre HeaderText (custom alebo default)
        /// </summary>
        public Color ResolvedHeaderTextColor => HeaderTextColor ?? GetDefaultHeaderTextColor();

        /// <summary>
        /// Vráti skutočnú farbu pre ValidationError (custom alebo default)
        /// </summary>
        public Color ResolvedValidationErrorColor => ValidationErrorColor ?? GetDefaultValidationErrorColor();

        /// <summary>
        /// Vráti skutočnú farbu pre Selection (custom alebo default)
        /// </summary>
        public Color ResolvedSelectionColor => SelectionColor ?? GetDefaultSelectionColor();

        /// <summary>
        /// Vráti skutočnú farbu pre AlternateRow (custom alebo default)
        /// </summary>
        public Color ResolvedAlternateRowColor => AlternateRowColor ?? GetDefaultAlternateRowColor();

        /// <summary>
        /// Vráti skutočnú farbu pre Hover (custom alebo default)
        /// </summary>
        public Color ResolvedHoverColor => HoverColor ?? GetDefaultHoverColor();

        /// <summary>
        /// Vráti skutočnú farbu pre EditingCell (custom alebo default)
        /// </summary>
        public Color ResolvedEditingCellColor => EditingCellColor ?? GetDefaultEditingCellColor();

        #endregion

        #region ✅ Helper Methods

        /// <summary>
        /// Skontroluje či má nastavenú nejakú custom farbu
        /// </summary>
        public bool HasAnyCustomColors =>
            CellBackgroundColor.HasValue || CellBorderColor.HasValue || CellTextColor.HasValue ||
            HeaderBackgroundColor.HasValue || HeaderTextColor.HasValue || ValidationErrorColor.HasValue ||
            SelectionColor.HasValue || AlternateRowColor.HasValue || HoverColor.HasValue || EditingCellColor.HasValue;

        /// <summary>
        /// Získa počet nastavených custom farieb
        /// </summary>
        public int CustomColorsCount =>
            (CellBackgroundColor.HasValue ? 1 : 0) + (CellBorderColor.HasValue ? 1 : 0) + (CellTextColor.HasValue ? 1 : 0) +
            (HeaderBackgroundColor.HasValue ? 1 : 0) + (HeaderTextColor.HasValue ? 1 : 0) + (ValidationErrorColor.HasValue ? 1 : 0) +
            (SelectionColor.HasValue ? 1 : 0) + (AlternateRowColor.HasValue ? 1 : 0) + (HoverColor.HasValue ? 1 : 0) + (EditingCellColor.HasValue ? 1 : 0);

        /// <summary>
        /// Vytvorí kópiu color config
        /// </summary>
        public DataGridColorConfig Clone()
        {
            return new DataGridColorConfig
            {
                CellBackgroundColor = CellBackgroundColor,
                CellBorderColor = CellBorderColor,
                CellTextColor = CellTextColor,
                HeaderBackgroundColor = HeaderBackgroundColor,
                HeaderTextColor = HeaderTextColor,
                ValidationErrorColor = ValidationErrorColor,
                SelectionColor = SelectionColor,
                AlternateRowColor = AlternateRowColor,
                HoverColor = HoverColor,
                EditingCellColor = EditingCellColor
            };
        }

        /// <summary>
        /// Resetuje všetky farby na default (nastaví všetky na null)
        /// </summary>
        public void ResetToDefaults()
        {
            CellBackgroundColor = null;
            CellBorderColor = null;
            CellTextColor = null;
            HeaderBackgroundColor = null;
            HeaderTextColor = null;
            ValidationErrorColor = null;
            SelectionColor = null;
            AlternateRowColor = null;
            HoverColor = null;
            EditingCellColor = null;
        }

        #endregion

        #region ✅ Static Factory Methods

        /// <summary>
        /// Vytvorí config iba s default farbami (všetky null)
        /// </summary>
        public static DataGridColorConfig Default => new DataGridColorConfig();

        /// <summary>
        /// Vytvorí light color scheme
        /// </summary>
        public static DataGridColorConfig Light => new DataGridColorConfig
        {
            CellBackgroundColor = Colors.White,
            CellBorderColor = Colors.LightGray,
            CellTextColor = Colors.Black,
            HeaderBackgroundColor = Color.FromArgb(255, 240, 240, 240),
            HeaderTextColor = Colors.Black,
            ValidationErrorColor = Colors.Red
        };

        /// <summary>
        /// Vytvorí dark color scheme
        /// </summary>
        public static DataGridColorConfig Dark => new DataGridColorConfig
        {
            CellBackgroundColor = Color.FromArgb(255, 32, 32, 32),
            CellBorderColor = Color.FromArgb(255, 64, 64, 64),
            CellTextColor = Colors.White,
            HeaderBackgroundColor = Color.FromArgb(255, 48, 48, 48),
            HeaderTextColor = Colors.White,
            ValidationErrorColor = Color.FromArgb(255, 255, 100, 100)
        };

        /// <summary>
        /// Vytvorí blue color scheme
        /// </summary>
        public static DataGridColorConfig Blue => new DataGridColorConfig
        {
            CellBackgroundColor = Color.FromArgb(255, 248, 250, 255),
            CellBorderColor = Color.FromArgb(255, 200, 220, 255),
            CellTextColor = Color.FromArgb(255, 20, 40, 80),
            HeaderBackgroundColor = Color.FromArgb(255, 230, 240, 255),
            HeaderTextColor = Color.FromArgb(255, 20, 40, 80)
        };

        #endregion

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        #endregion

        public override string ToString()
        {
            return $"DataGridColorConfig: {CustomColorsCount}/10 custom colors set, HasCustom: {HasAnyCustomColors}";
        }
    }
}