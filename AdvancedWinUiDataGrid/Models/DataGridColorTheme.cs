// Models/DataGridColorTheme.cs - ✅ OPRAVENÝ na INTERNAL
using Microsoft.UI;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Windows.UI;

namespace RpaWinUiComponents.AdvancedWinUiDataGrid
{
    /// <summary>
    /// ✅ INTERNAL: Stará implementácia color themes - NAHRADENÁ DataGridColorConfig
    /// Ponechaná pre backward compatibility ale nie je súčasťou PUBLIC API
    /// </summary>
    internal class DataGridColorTheme : INotifyPropertyChanged
    {
        #region Private Fields

        private Color _cellBackgroundColor = Colors.White;
        private Color _cellBorderColor = Colors.LightGray;
        private Color _cellTextColor = Colors.Black;
        private Color _headerBackgroundColor = Color.FromArgb(255, 240, 240, 240);
        private Color _headerTextColor = Colors.Black;
        private Color _validationErrorColor = Colors.Red;
        private Color _selectionColor = Color.FromArgb(100, 0, 120, 215);
        private Color _alternateRowColor = Color.FromArgb(20, 0, 0, 0);
        private Color _hoverColor = Color.FromArgb(50, 0, 120, 215);
        private Color _editingCellColor = Color.FromArgb(30, 255, 255, 0);

        #endregion

        #region Static Default Themes

        /// <summary>
        /// Default light theme
        /// </summary>
        public static DataGridColorTheme Light => new()
        {
            CellBackgroundColor = Colors.White,
            CellBorderColor = Colors.LightGray,
            CellTextColor = Colors.Black,
            HeaderBackgroundColor = Color.FromArgb(255, 240, 240, 240),
            HeaderTextColor = Colors.Black,
            ValidationErrorColor = Colors.Red,
            SelectionColor = Color.FromArgb(100, 0, 120, 215),
            AlternateRowColor = Color.FromArgb(20, 0, 0, 0),
            HoverColor = Color.FromArgb(50, 0, 120, 215),
            EditingCellColor = Color.FromArgb(30, 255, 255, 0)
        };

        /// <summary>
        /// Dark theme
        /// </summary>
        public static DataGridColorTheme Dark => new()
        {
            CellBackgroundColor = Color.FromArgb(255, 32, 32, 32),
            CellBorderColor = Color.FromArgb(255, 64, 64, 64),
            CellTextColor = Colors.White,
            HeaderBackgroundColor = Color.FromArgb(255, 48, 48, 48),
            HeaderTextColor = Colors.White,
            ValidationErrorColor = Color.FromArgb(255, 255, 100, 100),
            SelectionColor = Color.FromArgb(100, 100, 180, 255),
            AlternateRowColor = Color.FromArgb(20, 255, 255, 255),
            HoverColor = Color.FromArgb(50, 100, 180, 255),
            EditingCellColor = Color.FromArgb(30, 255, 255, 100)
        };

        /// <summary>
        /// Blue theme
        /// </summary>
        public static DataGridColorTheme Blue => new()
        {
            CellBackgroundColor = Color.FromArgb(255, 248, 250, 255),
            CellBorderColor = Color.FromArgb(255, 200, 220, 255),
            CellTextColor = Color.FromArgb(255, 20, 40, 80),
            HeaderBackgroundColor = Color.FromArgb(255, 230, 240, 255),
            HeaderTextColor = Color.FromArgb(255, 20, 40, 80),
            ValidationErrorColor = Color.FromArgb(255, 220, 50, 50),
            SelectionColor = Color.FromArgb(100, 50, 150, 255),
            AlternateRowColor = Color.FromArgb(15, 0, 100, 200),
            HoverColor = Color.FromArgb(40, 50, 150, 255),
            EditingCellColor = Color.FromArgb(25, 100, 200, 255)
        };

        /// <summary>
        /// Green theme  
        /// </summary>
        public static DataGridColorTheme Green => new()
        {
            CellBackgroundColor = Color.FromArgb(255, 248, 255, 248),
            CellBorderColor = Color.FromArgb(255, 200, 230, 200),
            CellTextColor = Color.FromArgb(255, 20, 60, 20),
            HeaderBackgroundColor = Color.FromArgb(255, 230, 245, 230),
            HeaderTextColor = Color.FromArgb(255, 20, 60, 20),
            ValidationErrorColor = Color.FromArgb(255, 200, 50, 50),
            SelectionColor = Color.FromArgb(100, 100, 200, 100),
            AlternateRowColor = Color.FromArgb(15, 0, 150, 0),
            HoverColor = Color.FromArgb(40, 100, 200, 100),
            EditingCellColor = Color.FromArgb(25, 150, 255, 150)
        };

        #endregion

        #region Color Properties

        /// <summary>
        /// Farba pozadia bunky
        /// </summary>
        public Color CellBackgroundColor
        {
            get => _cellBackgroundColor;
            set => SetProperty(ref _cellBackgroundColor, value);
        }

        /// <summary>
        /// Farba okraja bunky
        /// </summary>
        public Color CellBorderColor
        {
            get => _cellBorderColor;
            set => SetProperty(ref _cellBorderColor, value);
        }

        /// <summary>
        /// Farba textu v bunke
        /// </summary>
        public Color CellTextColor
        {
            get => _cellTextColor;
            set => SetProperty(ref _cellTextColor, value);
        }

        /// <summary>
        /// Farba pozadia header-u
        /// </summary>
        public Color HeaderBackgroundColor
        {
            get => _headerBackgroundColor;
            set => SetProperty(ref _headerBackgroundColor, value);
        }

        /// <summary>
        /// Farba textu header-u
        /// </summary>
        public Color HeaderTextColor
        {
            get => _headerTextColor;
            set => SetProperty(ref _headerTextColor, value);
        }

        /// <summary>
        /// Farba validačných chýb (červené orámovanie)
        /// </summary>
        public Color ValidationErrorColor
        {
            get => _validationErrorColor;
            set => SetProperty(ref _validationErrorColor, value);
        }

        /// <summary>
        /// Farba označenia buniek
        /// </summary>
        public Color SelectionColor
        {
            get => _selectionColor;
            set => SetProperty(ref _selectionColor, value);
        }

        /// <summary>
        /// Farba alternatívnych riadkov (zebra effect)
        /// </summary>
        public Color AlternateRowColor
        {
            get => _alternateRowColor;
            set => SetProperty(ref _alternateRowColor, value);
        }

        /// <summary>
        /// Farba pri hover nad bunkou
        /// </summary>
        public Color HoverColor
        {
            get => _hoverColor;
            set => SetProperty(ref _hoverColor, value);
        }

        /// <summary>
        /// Farba bunky ktorá sa edituje
        /// </summary>
        public Color EditingCellColor
        {
            get => _editingCellColor;
            set => SetProperty(ref _editingCellColor, value);
        }

        #endregion

        #region Utility Methods

        /// <summary>
        /// Vytvorí kópiu color theme
        /// </summary>
        public DataGridColorTheme Clone()
        {
            return new DataGridColorTheme
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
        /// Aplikuje farby z iného theme (merge operation)
        /// </summary>
        public void ApplyFrom(DataGridColorTheme otherTheme)
        {
            if (otherTheme == null) return;

            CellBackgroundColor = otherTheme.CellBackgroundColor;
            CellBorderColor = otherTheme.CellBorderColor;
            CellTextColor = otherTheme.CellTextColor;
            HeaderBackgroundColor = otherTheme.HeaderBackgroundColor;
            HeaderTextColor = otherTheme.HeaderTextColor;
            ValidationErrorColor = otherTheme.ValidationErrorColor;
            SelectionColor = otherTheme.SelectionColor;
            AlternateRowColor = otherTheme.AlternateRowColor;
            HoverColor = otherTheme.HoverColor;
            EditingCellColor = otherTheme.EditingCellColor;
        }

        /// <summary>
        /// Resetuje na default light theme
        /// </summary>
        public void Reset()
        {
            ApplyFrom(Light);
        }

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
            return $"DataGridColorTheme(Cell: {CellBackgroundColor}, Header: {HeaderBackgroundColor}, Validation: {ValidationErrorColor})";
        }
    }

    /// <summary>
    /// ✅ INTERNAL: Builder pattern pre DataGridColorTheme (NAHRADENÉ DataGridColorConfig)
    /// </summary>
    internal class DataGridColorThemeBuilder
    {
        private readonly DataGridColorTheme _theme = new();

        public static DataGridColorThemeBuilder Create() => new();

        public DataGridColorThemeBuilder WithCellBackground(Color color)
        {
            _theme.CellBackgroundColor = color;
            return this;
        }

        public DataGridColorThemeBuilder WithCellBorder(Color color)
        {
            _theme.CellBorderColor = color;
            return this;
        }

        public DataGridColorThemeBuilder WithCellText(Color color)
        {
            _theme.CellTextColor = color;
            return this;
        }

        public DataGridColorThemeBuilder WithHeaderBackground(Color color)
        {
            _theme.HeaderBackgroundColor = color;
            return this;
        }

        public DataGridColorThemeBuilder WithHeaderText(Color color)
        {
            _theme.HeaderTextColor = color;
            return this;
        }

        public DataGridColorThemeBuilder WithValidationError(Color color)
        {
            _theme.ValidationErrorColor = color;
            return this;
        }

        public DataGridColorThemeBuilder WithSelection(Color color)
        {
            _theme.SelectionColor = color;
            return this;
        }

        public DataGridColorThemeBuilder WithAlternateRow(Color color)
        {
            _theme.AlternateRowColor = color;
            return this;
        }

        public DataGridColorThemeBuilder WithHover(Color color)
        {
            _theme.HoverColor = color;
            return this;
        }

        public DataGridColorThemeBuilder WithEditingCell(Color color)
        {
            _theme.EditingCellColor = color;
            return this;
        }

        public DataGridColorTheme Build() => _theme;
    }
}