// Models/ColumnDefinition.cs - ROZŠÍRENÉ
using System;

namespace RpaWinUiComponents.AdvancedWinUiDataGrid
{
    /// <summary>
    /// Definícia stĺpca v DataGrid - PUBLIC API
    /// </summary>
    public class ColumnDefinition
    {
        /// <summary>
        /// Názov stĺpca (jedinečný identifikátor)
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Dátový typ stĺpca
        /// </summary>
        public Type DataType { get; set; } = typeof(string);

        /// <summary>
        /// Header text (zobrazovaný názov)
        /// </summary>
        public string? Header { get; set; }

        /// <summary>
        /// Šírka stĺpca
        /// </summary>
        public double Width { get; set; } = 150;

        /// <summary>
        /// Minimálna šírka stĺpca
        /// </summary>
        public double MinWidth { get; set; } = 50;

        /// <summary>
        /// Maximálna šírka stĺpca (0 = neobmedzená)
        /// </summary>
        public double MaxWidth { get; set; } = 0;

        /// <summary>
        /// Či je stĺpec viditeľný
        /// </summary>
        public bool IsVisible { get; set; } = true;

        /// <summary>
        /// Či je stĺpec editovateľný
        /// </summary>
        public bool IsEditable { get; set; } = true;

        /// <summary>
        /// Či je stĺpec sortovateľný
        /// </summary>
        public bool IsSortable { get; set; } = true;

        /// <summary>
        /// Či je stĺpec filtrovateľný
        /// </summary>
        public bool IsFilterable { get; set; } = true;

        /// <summary>
        /// Či sa má stĺpec zmeniť podľa obsahu
        /// </summary>
        public bool AutoResize { get; set; } = false;

        /// <summary>
        /// Výchozí hodnota pre nové bunky
        /// </summary>
        public object? DefaultValue { get; set; }

        /// <summary>
        /// Format string pre zobrazenie hodnôt
        /// </summary>
        public string? DisplayFormat { get; set; }

        /// <summary>
        /// Tooltip text pre header
        /// </summary>
        public string? ToolTip { get; set; }

        /// <summary>
        /// CSS class alebo style pre stĺpec
        /// </summary>
        public string? CssClass { get; set; }

        /// <summary>
        /// Poradie stĺpca (pre sorting)
        /// </summary>
        public int Order { get; set; } = 0;

        /// <summary>
        /// Či je toto špeciálny stĺpec (DeleteRows, ValidAlerts)
        /// </summary>
        public bool IsSpecialColumn => Name == "DeleteRows" || Name == "ValidAlerts";

        /// <summary>
        /// Typ špeciálneho stĺpca
        /// </summary>
        public SpecialColumnType SpecialColumnType
        {
            get
            {
                return Name switch
                {
                    "DeleteRows" => SpecialColumnType.DeleteButton,
                    "ValidAlerts" => SpecialColumnType.ValidationAlerts,
                    _ => SpecialColumnType.None
                };
            }
        }

        // Konštruktory
        public ColumnDefinition() { }

        public ColumnDefinition(string name, Type dataType)
        {
            Name = name;
            DataType = dataType;
            Header = name; // Default header je názov stĺpca
        }

        public ColumnDefinition(string name, Type dataType, string header) : this(name, dataType)
        {
            Header = header;
        }

        /// <summary>
        /// Validuje definíciu stĺpca
        /// </summary>
        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(Name))
                throw new ArgumentException("Názov stĺpca nemôže byť prázdny");

            if (DataType == null)
                throw new ArgumentException("DataType nemôže byť null");

            if (Width < 0)
                throw new ArgumentException("Width nemôže byť záporná");

            if (MinWidth < 0)
                throw new ArgumentException("MinWidth nemôže byť záporná");

            if (MaxWidth < 0)
                throw new ArgumentException("MaxWidth nemôže byť záporná");

            if (MaxWidth > 0 && MinWidth > MaxWidth)
                throw new ArgumentException("MinWidth nemôže byť väčšia ako MaxWidth");

            if (Width < MinWidth)
                throw new ArgumentException("Width nemôže byť menšia ako MinWidth");

            if (MaxWidth > 0 && Width > MaxWidth)
                throw new ArgumentException("Width nemôže byť väčšia ako MaxWidth");
        }

        /// <summary>
        /// Vytvorí kópiu definície stĺpca
        /// </summary>
        public ColumnDefinition Clone()
        {
            return new ColumnDefinition
            {
                Name = Name,
                DataType = DataType,
                Header = Header,
                Width = Width,
                MinWidth = MinWidth,
                MaxWidth = MaxWidth,
                IsVisible = IsVisible,
                IsEditable = IsEditable,
                IsSortable = IsSortable,
                IsFilterable = IsFilterable,
                AutoResize = AutoResize,
                DefaultValue = DefaultValue,
                DisplayFormat = DisplayFormat,
                ToolTip = ToolTip,
                CssClass = CssClass,
                Order = Order
            };
        }

        /// <summary>
        /// Konvertuje hodnotu na správny typ pre tento stĺpec
        /// </summary>
        public object? ConvertValue(object? value)
        {
            if (value == null) return DefaultValue;

            try
            {
                if (DataType == typeof(string))
                    return value.ToString();

                if (DataType.IsAssignableFrom(value.GetType()))
                    return value;

                return Convert.ChangeType(value, DataType);
            }
            catch
            {
                return DefaultValue;
            }
        }

        /// <summary>
        /// Formatuje hodnotu pre zobrazenie
        /// </summary>
        public string FormatDisplayValue(object? value)
        {
            if (value == null) return string.Empty;

            if (!string.IsNullOrEmpty(DisplayFormat))
            {
                try
                {
                    if (value is IFormattable formattable)
                        return formattable.ToString(DisplayFormat, null);
                }
                catch
                {
                    // Ak formátovanie zlyhá, vráť základný string
                }
            }

            return value.ToString() ?? string.Empty;
        }

        public override string ToString()
        {
            return $"{Name} ({DataType.Name}) - {Header}";
        }
    }

    /// <summary>
    /// Typ špeciálneho stĺpca
    /// </summary>
    public enum SpecialColumnType
    {
        None,
        DeleteButton,
        ValidationAlerts,
        RowIndex,
        Checkbox,
        Action
    }
}