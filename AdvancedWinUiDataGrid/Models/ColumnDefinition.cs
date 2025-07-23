// Models/ColumnDefinition.cs
using System;

namespace RpaWinUiComponents.AdvancedWinUiDataGrid
{
    /// <summary>
    /// Definícia stĺpca pre AdvancedDataGrid komponent.
    /// Obsahuje všetky potrebné informácie pre dynamické generovanie stĺpcov.
    /// </summary>
    public class ColumnDefinition
    {
        #region Konštruktory

        /// <summary>
        /// Vytvára novú definíciu stĺpca.
        /// </summary>
        /// <param name="name">Názov stĺpca (kľúč v dátach)</param>
        /// <param name="dataType">Dátový typ stĺpca</param>
        public ColumnDefinition(string name, Type dataType)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            DataType = dataType ?? throw new ArgumentNullException(nameof(dataType));
            Header = name; // Default header je názov stĺpca
            Width = 150; // Default šírka
            MinWidth = 60; // Default minimálna šírka
            MaxWidth = double.PositiveInfinity; // Bez limitu
            IsReadOnly = false;
            IsVisible = true;
        }

        #endregion

        #region Základné properties

        /// <summary>
        /// Názov stĺpca - kľúč v dátach. Nesmie byť null alebo prázdny.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Dátový typ stĺpca (string, int, DateTime, atď.)
        /// </summary>
        public Type DataType { get; }

        /// <summary>
        /// Text ktorý sa zobrazí v hlavičke stĺpca. Môže obsahovať emoji a špeciálne znaky.
        /// </summary>
        public string Header { get; set; }

        #endregion

        #region Rozmery stĺpca

        /// <summary>
        /// Šírka stĺpca v pixeloch. Default: 150px
        /// </summary>
        public double Width { get; set; }

        /// <summary>
        /// Minimálna šírka stĺpca v pixeloch. Default: 60px
        /// </summary>
        public double MinWidth { get; set; }

        /// <summary>
        /// Maximálna šírka stĺpca v pixeloch. Default: neobmedzené
        /// </summary>
        public double MaxWidth { get; set; }

        #endregion

        #region Správanie stĺpca

        /// <summary>
        /// Určuje či je stĺpec iba na čítanie. Default: false
        /// </summary>
        public bool IsReadOnly { get; set; }

        /// <summary>
        /// Určuje či je stĺpec viditeľný. Default: true
        /// </summary>
        public bool IsVisible { get; set; }

        /// <summary>
        /// Placeholder text pre prázdne bunky.
        /// </summary>
        public string? PlaceholderText { get; set; }

        #endregion

        #region Špeciálne stĺpce

        /// <summary>
        /// Určuje či je to špeciálny "DeleteRows" stĺpec.
        /// </summary>
        public bool IsDeleteColumn => Name.Equals("DeleteRows", StringComparison.OrdinalIgnoreCase);

        /// <summary>
        /// Určuje či je to špeciálny "ValidAlerts" stĺpec.
        /// </summary>
        public bool IsValidationColumn => Name.Equals("ValidAlerts", StringComparison.OrdinalIgnoreCase);

        /// <summary>
        /// Určuje či je to štandardný dátový stĺpec (nie špeciálny).
        /// </summary>
        public bool IsDataColumn => !IsDeleteColumn && !IsValidationColumn;

        #endregion

        #region Validácia

        /// <summary>
        /// Validuje či je definícia stĺpca správna.
        /// </summary>
        /// <returns>True ak je definícia validná</returns>
        public bool IsValid()
        {
            if (string.IsNullOrWhiteSpace(Name))
                return false;

            if (DataType == null)
                return false;

            if (Width < 0 || MinWidth < 0 || MaxWidth < 0)
                return false;

            if (MinWidth > MaxWidth)
                return false;

            return true;
        }

        /// <summary>
        /// Vráti chybové správy validácie ako zoznam.
        /// </summary>
        public List<string> GetValidationErrors()
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(Name))
                errors.Add("Column name cannot be null or empty");

            if (DataType == null)
                errors.Add("DataType cannot be null");

            if (Width < 0)
                errors.Add("Width cannot be negative");

            if (MinWidth < 0)
                errors.Add("MinWidth cannot be negative");

            if (MaxWidth < 0)
                errors.Add("MaxWidth cannot be negative");

            if (MinWidth > MaxWidth)
                errors.Add("MinWidth cannot be greater than MaxWidth");

            return errors;
        }

        #endregion

        #region Overrides

        public override string ToString()
        {
            return $"Column '{Name}' ({DataType.Name}) - Width: {Width}, Header: '{Header}'";
        }

        public override bool Equals(object? obj)
        {
            if (obj is ColumnDefinition other)
            {
                return Name.Equals(other.Name, StringComparison.OrdinalIgnoreCase);
            }
            return false;
        }

        public override int GetHashCode()
        {
            return Name.ToLowerInvariant().GetHashCode();
        }

        #endregion

        #region Factory Methods

        /// <summary>
        /// Vytvorí DeleteRows stĺpec s predvolenými nastaveniami.
        /// </summary>
        public static ColumnDefinition CreateDeleteColumn()
        {
            return new ColumnDefinition("DeleteRows", typeof(string))
            {
                Header = "🗑️",
                Width = 40,
                MinWidth = 30,
                MaxWidth = 50,
                IsReadOnly = true
            };
        }

        /// <summary>
        /// Vytvorí ValidAlerts stĺpec s predvolenými nastaveniami.
        /// </summary>
        public static ColumnDefinition CreateValidationColumn()
        {
            return new ColumnDefinition("ValidAlerts", typeof(string))
            {
                Header = "⚠️ Validácie",
                Width = 200,
                MinWidth = 150,
                MaxWidth = 400,
                IsReadOnly = true
            };
        }

        #endregion
    }
}