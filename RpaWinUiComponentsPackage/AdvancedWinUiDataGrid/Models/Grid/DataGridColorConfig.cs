// Models/DataGridColorConfig.cs - ✅ AKTUALIZOVANÉ s Zebra Row colors
using Microsoft.UI;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Windows.UI;

namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Models.Grid
{
    /// <summary>
    /// Individual color konfigurácia pre DataGrid s Zebra Rows - nastavuje sa iba pri inicializácii
    /// ✅ PUBLIC API - umožňuje nastaviť jednotlivé farby namiesto celých themes
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
        private Color? _alternateRowColor;  // ✅ Zebra rows effect
        private Color? _hoverColor;
        private Color? _editingCellColor;

        // ✅ NOVÉ: Cell Selection farby
        private Color? _focusedCellColor;
        private Color? _copiedCellColor;
        private Color? _validationErrorBorderColor;

        #endregion

        #region ✅ Individual Color Properties (null = default farba)

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
        /// ✅ AKTUALIZOVANÉ: Farba alternatívnych riadkov - zebra effect (null = žiadny effect)
        /// Každý druhý neprázdny riadok bude mať túto farbu pozadia
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

        /// <summary>
        /// ✅ NOVÉ: Farba pozadia bunky ktorá má focus (null = default)
        /// </summary>
        public Color? FocusedCellColor
        {
            get => _focusedCellColor;
            set => SetProperty(ref _focusedCellColor, value);
        }

        /// <summary>
        /// ✅ NOVÉ: Farba pozadia bunky ktorá bola skopírovaná (Ctrl+C) (null = default)
        /// </summary>
        public Color? CopiedCellColor
        {
            get => _copiedCellColor;
            set => SetProperty(ref _copiedCellColor, value);
        }

        /// <summary>
        /// ✅ NOVÉ: Farba border pre bunky s validation error (null = default)
        /// </summary>
        public Color? ValidationErrorBorderColor
        {
            get => _validationErrorBorderColor;
            set => SetProperty(ref _validationErrorBorderColor, value);
        }

        #endregion

        #region ✅ Resolved Colors (vráti custom color alebo default)

        /// <summary>
        /// Vráti skutočnú farbu pre CellBackground (custom alebo default)
        /// </summary>
        public Color ResolvedCellBackgroundColor => CellBackgroundColor ?? Colors.White;

        /// <summary>
        /// Vráti skutočnú farbu pre CellBorder (custom alebo default)
        /// </summary>
        public Color ResolvedCellBorderColor => CellBorderColor ?? Colors.LightGray;

        /// <summary>
        /// Vráti skutočnú farbu pre CellText (custom alebo default)
        /// </summary>
        public Color ResolvedCellTextColor => CellTextColor ?? Colors.Black;

        /// <summary>
        /// Vráti skutočnú farbu pre HeaderBackground (custom alebo default)
        /// </summary>
        public Color ResolvedHeaderBackgroundColor => HeaderBackgroundColor ?? Color.FromArgb(255, 240, 240, 240);

        /// <summary>
        /// Vráti skutočnú farbu pre HeaderText (custom alebo default)
        /// </summary>
        public Color ResolvedHeaderTextColor => HeaderTextColor ?? Colors.Black;

        /// <summary>
        /// Vráti skutočnú farbu pre ValidationError (custom alebo default)
        /// </summary>
        public Color ResolvedValidationErrorColor => ValidationErrorColor ?? Colors.Red;

        /// <summary>
        /// Vráti skutočnú farbu pre Selection (custom alebo default)
        /// </summary>
        public Color ResolvedSelectionColor => SelectionColor ?? Color.FromArgb(100, 0, 120, 215);

        /// <summary>
        /// ✅ AKTUALIZOVANÉ: Vráti skutočnú farbu pre AlternateRow/Zebra (custom alebo default)
        /// </summary>
        public Color ResolvedAlternateRowColor => AlternateRowColor ?? Color.FromArgb(20, 0, 0, 0);

        /// <summary>
        /// Vráti skutočnú farbu pre Hover (custom alebo default)
        /// </summary>
        public Color ResolvedHoverColor => HoverColor ?? Color.FromArgb(50, 0, 120, 215);

        /// <summary>
        /// Vráti skutočnú farbu pre EditingCell (custom alebo default)
        /// </summary>
        public Color ResolvedEditingCellColor => EditingCellColor ?? Color.FromArgb(30, 255, 255, 0);

        /// <summary>
        /// ✅ NOVÉ: Vráti skutočnú farbu pre FocusedCell (custom alebo default)
        /// </summary>
        public Color ResolvedFocusedCellColor => FocusedCellColor ?? Color.FromArgb(80, 0, 120, 215);

        /// <summary>
        /// ✅ NOVÉ: Vráti skutočnú farbu pre CopiedCell (custom alebo default)
        /// </summary>
        public Color ResolvedCopiedCellColor => CopiedCellColor ?? Color.FromArgb(60, 34, 139, 34);

        /// <summary>
        /// ✅ NOVÉ: Vráti skutočnú farbu pre ValidationErrorBorder (custom alebo default)
        /// </summary>
        public Color ResolvedValidationErrorBorderColor => ValidationErrorBorderColor ?? Colors.Red;

        /// <summary>
        /// Alias pre ResolvedAlternateRowColor - pre zebra rows
        /// </summary>
        public Color ResolvedZebraRowColor => ResolvedAlternateRowColor;

        /// <summary>
        /// Alias pre ResolvedCellTextColor - pre focused text
        /// </summary>
        public Color ResolvedFocusedTextColor => ResolvedCellTextColor;

        /// <summary>
        /// Alias pre ResolvedCellTextColor - pre všeobecný text
        /// </summary>
        public Color ResolvedTextColor => ResolvedCellTextColor;

        #endregion

        #region ✅ Helper Methods

        /// <summary>
        /// Skontroluje či má nastavené nejaké custom farby
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
        /// ✅ NOVÉ: Kontroluje či je zebra rows effect povolený
        /// </summary>
        public bool IsZebraRowsEnabled => AlternateRowColor.HasValue;

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

        #region ✅ Static Factory Methods s Zebra Support

        /// <summary>
        /// Vytvorí config iba s default farbami (všetky null)
        /// </summary>
        public static DataGridColorConfig Default => new DataGridColorConfig();

        /// <summary>
        /// Vytvorí default konfiguráciu s explicitne nastavenými farbami
        /// </summary>
        public static DataGridColorConfig CreateDefault()
        {
            return new DataGridColorConfig
            {
                CellBackgroundColor = Colors.White,
                CellBorderColor = Colors.LightGray,
                CellTextColor = Colors.Black,
                HeaderBackgroundColor = Color.FromArgb(255, 240, 240, 240),
                HeaderTextColor = Colors.Black,
                ValidationErrorColor = Colors.Red,
                SelectionColor = Color.FromArgb(100, 0, 120, 215),
                AlternateRowColor = Color.FromArgb(20, 0, 0, 0), // Jemný zebra effect
                HoverColor = Color.FromArgb(50, 0, 120, 215),
                EditingCellColor = Color.FromArgb(30, 255, 255, 0)
            };
        }

        /// <summary>
        /// Vytvorí light color scheme s jemným zebra effect
        /// </summary>
        public static DataGridColorConfig Light => new DataGridColorConfig
        {
            CellBackgroundColor = Colors.White,
            CellBorderColor = Colors.LightGray,
            CellTextColor = Colors.Black,
            HeaderBackgroundColor = Color.FromArgb(255, 240, 240, 240),
            HeaderTextColor = Colors.Black,
            ValidationErrorColor = Colors.Red,
            AlternateRowColor = Color.FromArgb(10, 0, 0, 0) // Jemný zebra effect
        };

        /// <summary>
        /// Vytvorí dark color scheme s jemným zebra effect
        /// </summary>
        public static DataGridColorConfig Dark => new DataGridColorConfig
        {
            CellBackgroundColor = Color.FromArgb(255, 32, 32, 32),
            CellBorderColor = Color.FromArgb(255, 64, 64, 64),
            CellTextColor = Colors.White,
            HeaderBackgroundColor = Color.FromArgb(255, 48, 48, 48),
            HeaderTextColor = Colors.White,
            ValidationErrorColor = Color.FromArgb(255, 255, 100, 100),
            AlternateRowColor = Color.FromArgb(20, 255, 255, 255) // Jemný zebra effect
        };

        /// <summary>
        /// Vytvorí blue color scheme s jemným zebra effect
        /// </summary>
        public static DataGridColorConfig Blue => new DataGridColorConfig
        {
            CellBackgroundColor = Color.FromArgb(255, 248, 250, 255),
            CellBorderColor = Color.FromArgb(255, 200, 220, 255),
            CellTextColor = Color.FromArgb(255, 20, 40, 80),
            HeaderBackgroundColor = Color.FromArgb(255, 230, 240, 255),
            HeaderTextColor = Color.FromArgb(255, 20, 40, 80),
            AlternateRowColor = Color.FromArgb(15, 0, 100, 200) // Jemný zebra effect
        };

        /// <summary>
        /// ✅ NOVÉ: Vytvorí config s výrazným zebra effect (pre demonstráciu)
        /// </summary>
        public static DataGridColorConfig WithStrongZebra => new DataGridColorConfig
        {
            CellBackgroundColor = Colors.White,
            CellBorderColor = Colors.LightGray,
            CellTextColor = Colors.Black,
            HeaderBackgroundColor = Color.FromArgb(255, 240, 240, 240),
            HeaderTextColor = Colors.Black,
            ValidationErrorColor = Colors.Red,
            AlternateRowColor = Color.FromArgb(50, 0, 120, 215) // Výrazný zebra effect
        };

        /// <summary>
        /// ✅ NOVÉ: Vytvorí config bez zebra effect (iba jednotné farby)
        /// </summary>
        public static DataGridColorConfig WithoutZebra => new DataGridColorConfig
        {
            CellBackgroundColor = Colors.White,
            CellBorderColor = Colors.LightGray,
            CellTextColor = Colors.Black,
            HeaderBackgroundColor = Color.FromArgb(255, 240, 240, 240),
            HeaderTextColor = Colors.Black,
            ValidationErrorColor = Colors.Red
            // AlternateRowColor = null (žiadny zebra effect)
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
            return $"DataGridColorConfig: {CustomColorsCount}/10 custom colors, Zebra: {IsZebraRowsEnabled}";
        }
    }
}