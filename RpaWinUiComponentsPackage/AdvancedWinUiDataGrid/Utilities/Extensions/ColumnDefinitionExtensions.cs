// AdvancedWinUiDataGrid/Utilities/Extensions/ColumnDefinitionExtensions.cs - ✅ PRESUNUTO
using System;
using System.Collections.Generic;
using System.Linq;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Models;

namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Utilities.Extensions
{
    /// <summary>
    /// Extension metódy pre ColumnDefinition triedy.
    /// Poskytuje pomocné metódy pre prácu s definíciami stĺpcov.
    /// ✅ INTERNAL - súčasť AdvancedWinUiDataGrid komponentu
    /// </summary>
    internal static class ColumnDefinitionExtensions
    {
        #region Fluent API extensions

        /// <summary>
        /// Nastaví šírku stĺpca (fluent API).
        /// </summary>
        /// <param name="column">Definícia stĺpca</param>
        /// <param name="width">Šírka</param>
        /// <param name="minWidth">Minimálna šírka (optional)</param>
        /// <param name="maxWidth">Maximálna šírka (optional)</param>
        /// <returns>Tá istá inštancia pre fluent chaining</returns>
        public static ColumnDefinition WithWidth(this ColumnDefinition column, double width, double? minWidth = null, double? maxWidth = null)
        {
            column.Width = width;
            if (minWidth.HasValue) column.MinWidth = minWidth.Value;
            if (maxWidth.HasValue) column.MaxWidth = maxWidth.Value;
            return column;
        }

        /// <summary>
        /// Nastaví header stĺpca (fluent API).
        /// </summary>
        /// <param name="column">Definícia stĺpca</param>
        /// <param name="header">Text header-u</param>
        /// <returns>Tá istá inštancia pre fluent chaining</returns>
        public static ColumnDefinition WithHeader(this ColumnDefinition column, string header)
        {
            column.Header = header;
            return column;
        }

        /// <summary>
        /// Nastaví stĺpec ako readonly (fluent API).
        /// </summary>
        /// <param name="column">Definícia stĺpca</param>
        /// <param name="isReadOnly">Či je readonly</param>
        /// <returns>Tá istá inštancia pre fluent chaining</returns>
        public static ColumnDefinition AsReadOnly(this ColumnDefinition column, bool isReadOnly = true)
        {
            column.IsEditable = !isReadOnly;
            return column;
        }

        /// <summary>
        /// Nastaví viditeľnosť stĺpca (fluent API).
        /// </summary>
        /// <param name="column">Definícia stĺpca</param>
        /// <param name="isVisible">Či je viditeľný</param>
        /// <returns>Tá istá inštancia pre fluent chaining</returns>
        public static ColumnDefinition WithVisibility(this ColumnDefinition column, bool isVisible)
        {
            column.IsVisible = isVisible;
            return column;
        }

        /// <summary>
        /// Nastaví default hodnotu (fluent API).
        /// </summary>
        /// <param name="column">Definícia stĺpca</param>
        /// <param name="defaultValue">Default hodnota</param>
        /// <returns>Tá istá inštancia pre fluent chaining</returns>
        public static ColumnDefinition WithDefaultValue(this ColumnDefinition column, object? defaultValue)
        {
            column.DefaultValue = defaultValue;
            return column;
        }

        #endregion

        #region Collection extensions

        /// <summary>
        /// Vráti iba dátové stĺpce z kolekcie.
        /// </summary>
        /// <param name="columns">Kolekcia stĺpcov</param>
        /// <returns>Filtrované dátové stĺpce</returns>
        public static IEnumerable<ColumnDefinition> DataColumnsOnly(this IEnumerable<ColumnDefinition> columns)
        {
            return columns.Where(c => !c.IsSpecialColumn);
        }

        /// <summary>
        /// Vráti iba viditeľné stĺpce z kolekcie.
        /// </summary>
        /// <param name="columns">Kolekcia stĺpcov</param>
        /// <returns>Filtrované viditeľné stĺpce</returns>
        public static IEnumerable<ColumnDefinition> VisibleOnly(this IEnumerable<ColumnDefinition> columns)
        {
            return columns.Where(c => c.IsVisible);
        }

        /// <summary>
        /// Vráti iba editovateľné stĺpce z kolekcie.
        /// </summary>
        /// <param name="columns">Kolekcia stĺpcov</param>
        /// <returns>Filtrované editovateľné stĺpce</returns>
        public static IEnumerable<ColumnDefinition> EditableOnly(this IEnumerable<ColumnDefinition> columns)
        {
            return columns.Where(c => c.IsEditable);
        }

        /// <summary>
        /// Vyhľadá stĺpec podľa názvu (case-insensitive).
        /// </summary>
        /// <param name="columns">Kolekcia stĺpcov</param>
        /// <param name="columnName">Názov stĺpca</param>
        /// <returns>Nájdený stĺpec alebo null</returns>
        public static ColumnDefinition? FindByName(this IEnumerable<ColumnDefinition> columns, string columnName)
        {
            return columns.FirstOrDefault(c => c.Name.Equals(columnName, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Vráti stĺpce určitého dátového typu.
        /// </summary>
        /// <param name="columns">Kolekcia stĺpcov</param>
        /// <param name="dataType">Dátový typ</param>
        /// <returns>Filtrované stĺpce podľa typu</returns>
        public static IEnumerable<ColumnDefinition> OfType(this IEnumerable<ColumnDefinition> columns, Type dataType)
        {
            return columns.Where(c => c.DataType == dataType);
        }

        /// <summary>
        /// Vráti číselné stĺpce.
        /// </summary>
        /// <param name="columns">Kolekcia stĺpcov</param>
        /// <returns>Číselné stĺpce</returns>
        public static IEnumerable<ColumnDefinition> NumericColumns(this IEnumerable<ColumnDefinition> columns)
        {
            return columns.Where(c => IsNumericType(c.DataType));
        }

        /// <summary>
        /// Vráti textové stĺpce.
        /// </summary>
        /// <param name="columns">Kolekcia stĺpcov</param>
        /// <returns>Textové stĺpce</returns>
        public static IEnumerable<ColumnDefinition> TextColumns(this IEnumerable<ColumnDefinition> columns)
        {
            return columns.Where(c => c.DataType == typeof(string));
        }

        #endregion

        #region Validation extensions

        /// <summary>
        /// Validuje kolekciu stĺpcov a vráti chyby.
        /// </summary>
        /// <param name="columns">Kolekcia stĺpcov</param>
        /// <returns>Zoznam chýb validácie</returns>
        public static List<string> ValidateColumns(this IEnumerable<ColumnDefinition> columns)
        {
            var errors = new List<string>();
            var columnsList = columns.ToList();

            if (!columnsList.Any())
            {
                errors.Add("Musí byť definovaný aspoň jeden stĺpec");
                return errors;
            }

            // Kontrola duplicitných názvov
            var duplicates = columnsList.GroupBy(c => c.Name.ToLowerInvariant())
                                      .Where(g => g.Count() > 1)
                                      .Select(g => g.Key);

            foreach (var duplicate in duplicates)
            {
                errors.Add($"Duplicitný názov stĺpca: '{duplicate}'");
            }

            // Validácia jednotlivých stĺpcov
            foreach (var column in columnsList)
            {
                try
                {
                    column.Validate();
                }
                catch (Exception ex)
                {
                    errors.Add($"Stĺpec '{column.Name}': {ex.Message}");
                }
            }

            return errors;
        }

        /// <summary>
        /// Skontroluje či je kolekcia stĺpcov validná.
        /// </summary>
        /// <param name="columns">Kolekcia stĺpcov</param>
        /// <returns>True ak je validná</returns>
        public static bool IsValid(this IEnumerable<ColumnDefinition> columns)
        {
            return !columns.ValidateColumns().Any();
        }

        #endregion

        #region Utility extensions

        /// <summary>
        /// Vytvorí kópiu stĺpca s novým názvom.
        /// </summary>
        /// <param name="column">Pôvodný stĺpec</param>
        /// <param name="newName">Nový názov</param>
        /// <returns>Kópia stĺpca</returns>
        public static ColumnDefinition Clone(this ColumnDefinition column, string? newName = null)
        {
            return column.Clone();
        }

        /// <summary>
        /// Vráti celkovú šírku všetkých stĺpcov.
        /// </summary>
        /// <param name="columns">Kolekcia stĺpcov</param>
        /// <returns>Celková šírka</returns>
        public static double GetTotalWidth(this IEnumerable<ColumnDefinition> columns)
        {
            return columns.Where(c => c.IsVisible).Sum(c => c.Width);
        }

        /// <summary>
        /// Vráti celkovú minimálnu šírku všetkých stĺpcov.
        /// </summary>
        /// <param name="columns">Kolekcia stĺpcov</param>
        /// <returns>Celková minimálna šírka</returns>
        public static double GetTotalMinWidth(this IEnumerable<ColumnDefinition> columns)
        {
            return columns.Where(c => c.IsVisible).Sum(c => c.MinWidth);
        }

        /// <summary>
        /// Upraví šírky stĺpcov proporcionálne na celkovú šírku.
        /// </summary>
        /// <param name="columns">Kolekcia stĺpcov</param>
        /// <param name="totalWidth">Cieľová celková šírka</param>
        public static void ResizeProportionally(this IEnumerable<ColumnDefinition> columns, double totalWidth)
        {
            var visibleColumns = columns.Where(c => c.IsVisible).ToList();
            if (!visibleColumns.Any()) return;

            var currentTotal = visibleColumns.Sum(c => c.Width);
            if (currentTotal <= 0) return;

            var ratio = totalWidth / currentTotal;

            foreach (var column in visibleColumns)
            {
                var newWidth = column.Width * ratio;
                column.Width = Math.Max(newWidth, column.MinWidth);
            }
        }

        #endregion

        #region Private helpers

        /// <summary>
        /// Určuje či je typ číselný.
        /// </summary>
        private static bool IsNumericType(Type type)
        {
            var underlyingType = Nullable.GetUnderlyingType(type) ?? type;

            return underlyingType == typeof(int) ||
                   underlyingType == typeof(long) ||
                   underlyingType == typeof(decimal) ||
                   underlyingType == typeof(double) ||
                   underlyingType == typeof(float) ||
                   underlyingType == typeof(short) ||
                   underlyingType == typeof(byte);
        }

        #endregion

        #region Factory helpers

        /// <summary>
        /// Vytvorí kolekciu základných stĺpcov pre dáta.
        /// </summary>
        /// <param name="dataTypes">Slovník názvov stĺpcov a ich typov</param>
        /// <returns>Kolekcia ColumnDefinition</returns>
        public static List<ColumnDefinition> CreateFromTypes(Dictionary<string, Type> dataTypes)
        {
            var columns = new List<ColumnDefinition>();

            foreach (var kvp in dataTypes)
            {
                var column = new ColumnDefinition(kvp.Key, kvp.Value)
                {
                    Header = kvp.Key,
                    Width = GetDefaultWidthForType(kvp.Value)
                };
                columns.Add(column);
            }

            return columns;
        }

        /// <summary>
        /// Vráti predvolenú šírku pre daný typ.
        /// </summary>
        private static double GetDefaultWidthForType(Type type)
        {
            var underlyingType = Nullable.GetUnderlyingType(type) ?? type;

            return underlyingType switch
            {
                Type t when t == typeof(bool) => 80,
                Type t when t == typeof(int) || t == typeof(long) => 100,
                Type t when t == typeof(decimal) || t == typeof(double) || t == typeof(float) => 120,
                Type t when t == typeof(DateTime) => 150,
                Type t when t == typeof(string) => 200,
                _ => 150
            };
        }

        #endregion
    }
}