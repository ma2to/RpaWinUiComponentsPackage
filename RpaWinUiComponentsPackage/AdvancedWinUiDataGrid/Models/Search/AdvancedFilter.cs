// Models/AdvancedFilter.cs - ✅ INTERNAL Advanced filtering models
using System;
using System.Collections.Generic;
using System.Linq;

namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Models.Search
{
    /// <summary>
    /// Advanced filter pre multi-column filtering - INTERNAL
    /// </summary>
    internal class AdvancedFilter
    {
        #region Properties

        /// <summary>
        /// Unique identifier filtra
        /// </summary>
        public string Id { get; set; } = Guid.NewGuid().ToString("N")[..8];

        /// <summary>
        /// Názov filtra
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Target column
        /// </summary>
        public string ColumnName { get; set; } = string.Empty;

        /// <summary>
        /// Filter operator
        /// </summary>
        public FilterOperator Operator { get; set; } = FilterOperator.Contains;

        /// <summary>
        /// Filter value
        /// </summary>
        public object? Value { get; set; }

        /// <summary>
        /// Druhá hodnota pre range operators
        /// </summary>
        public object? SecondValue { get; set; }

        /// <summary>
        /// Či je filter enabled
        /// </summary>
        public bool IsEnabled { get; set; } = true;

        /// <summary>
        /// Case sensitive pre string operations
        /// </summary>
        public bool CaseSensitive { get; set; } = false;

        /// <summary>
        /// Logical operator s predchádzajúcim filtrom
        /// </summary>
        public LogicalOperator LogicalOperator { get; set; } = LogicalOperator.And;

        #endregion

        #region Filter Execution

        /// <summary>
        /// Aplikuje filter na row data
        /// </summary>
        public bool ApplyFilter(Dictionary<string, object?> rowData)
        {
            if (!IsEnabled || !rowData.ContainsKey(ColumnName))
                return true;

            var columnValue = rowData[ColumnName];
            return EvaluateCondition(columnValue);
        }

        /// <summary>
        /// Evaluuje filter condition
        /// </summary>
        private bool EvaluateCondition(object? columnValue)
        {
            var stringValue = columnValue?.ToString() ?? string.Empty;
            var filterValue = Value?.ToString() ?? string.Empty;

            var comparison = CaseSensitive 
                ? StringComparison.Ordinal 
                : StringComparison.OrdinalIgnoreCase;

            return Operator switch
            {
                FilterOperator.Equals => string.Equals(stringValue, filterValue, comparison),
                FilterOperator.NotEquals => !string.Equals(stringValue, filterValue, comparison),
                FilterOperator.Contains => stringValue.Contains(filterValue, comparison),
                FilterOperator.NotContains => !stringValue.Contains(filterValue, comparison),
                FilterOperator.StartsWith => stringValue.StartsWith(filterValue, comparison),
                FilterOperator.EndsWith => stringValue.EndsWith(filterValue, comparison),
                FilterOperator.IsEmpty => string.IsNullOrEmpty(stringValue),
                FilterOperator.IsNotEmpty => !string.IsNullOrEmpty(stringValue),
                FilterOperator.GreaterThan => CompareNumeric(columnValue, Value, (a, b) => a > b),
                FilterOperator.GreaterThanOrEqual => CompareNumeric(columnValue, Value, (a, b) => a >= b),
                FilterOperator.LessThan => CompareNumeric(columnValue, Value, (a, b) => a < b),
                FilterOperator.LessThanOrEqual => CompareNumeric(columnValue, Value, (a, b) => a <= b),
                FilterOperator.Between => EvaluateBetween(columnValue),
                FilterOperator.NotBetween => !EvaluateBetween(columnValue),
                FilterOperator.In => EvaluateIn(columnValue),
                FilterOperator.NotIn => !EvaluateIn(columnValue),
                FilterOperator.Regex => EvaluateRegex(stringValue, filterValue),
                _ => true
            };
        }

        /// <summary>
        /// Numeric comparison
        /// </summary>
        private bool CompareNumeric(object? value1, object? value2, Func<decimal, decimal, bool> comparison)
        {
            if (decimal.TryParse(value1?.ToString(), out var num1) && 
                decimal.TryParse(value2?.ToString(), out var num2))
            {
                return comparison(num1, num2);
            }
            return false;
        }

        /// <summary>
        /// Between range evaluation
        /// </summary>
        private bool EvaluateBetween(object? columnValue)
        {
            if (decimal.TryParse(columnValue?.ToString(), out var numValue) &&
                decimal.TryParse(Value?.ToString(), out var minValue) &&
                decimal.TryParse(SecondValue?.ToString(), out var maxValue))
            {
                return numValue >= minValue && numValue <= maxValue;
            }
            return false;
        }

        /// <summary>
        /// In list evaluation
        /// </summary>
        private bool EvaluateIn(object? columnValue)
        {
            var stringValue = columnValue?.ToString() ?? string.Empty;
            var listValue = Value?.ToString() ?? string.Empty;
            
            var values = listValue.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(v => v.Trim())
                .ToList();

            var comparison = CaseSensitive 
                ? StringComparison.Ordinal 
                : StringComparison.OrdinalIgnoreCase;

            return values.Any(v => string.Equals(stringValue, v, comparison));
        }

        /// <summary>
        /// Regex evaluation
        /// </summary>
        private bool EvaluateRegex(string stringValue, string pattern)
        {
            try
            {
                var options = CaseSensitive 
                    ? System.Text.RegularExpressions.RegexOptions.None
                    : System.Text.RegularExpressions.RegexOptions.IgnoreCase;

                return System.Text.RegularExpressions.Regex.IsMatch(stringValue, pattern, options);
            }
            catch
            {
                return false; // Invalid regex pattern
            }
        }

        #endregion

        #region Diagnostics

        /// <summary>
        /// Diagnostic info o filtri
        /// </summary>
        public string GetDiagnosticInfo()
        {
            return $"Filter '{Name}' (ID: {Id}) - Column: {ColumnName}, " +
                   $"Operator: {Operator}, Value: '{Value}', Enabled: {IsEnabled}";
        }

        #endregion
    }

    /// <summary>
    /// Filter operators - INTERNAL
    /// </summary>
    internal enum FilterOperator
    {
        Equals,
        NotEquals,
        Contains,
        NotContains,
        StartsWith,
        EndsWith,
        IsEmpty,
        IsNotEmpty,
        GreaterThan,
        GreaterThanOrEqual,
        LessThan,
        LessThanOrEqual,
        Between,
        NotBetween,
        In,
        NotIn,
        Regex
    }

    /// <summary>
    /// Logical operators pre kombinovanie filtrov - INTERNAL
    /// </summary>
    internal enum LogicalOperator
    {
        And,
        Or
    }

    /// <summary>
    /// Multi-column filter set - INTERNAL
    /// </summary>
    internal class MultiColumnFilterSet
    {
        #region Properties

        /// <summary>
        /// Názov filter set
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Všetky filtre v sete
        /// </summary>
        public List<AdvancedFilter> Filters { get; private set; } = new();

        /// <summary>
        /// Či je filter set enabled
        /// </summary>
        public bool IsEnabled { get; set; } = true;

        /// <summary>
        /// Default logical operator medzi filtrami
        /// </summary>
        public LogicalOperator DefaultOperator { get; set; } = LogicalOperator.And;

        #endregion

        #region Filter Management

        /// <summary>
        /// Pridá filter do setu
        /// </summary>
        public MultiColumnFilterSet AddFilter(AdvancedFilter filter)
        {
            if (filter != null && !Filters.Any(f => f.Id == filter.Id))
            {
                // Set logical operator pre nový filter
                if (Filters.Any())
                {
                    filter.LogicalOperator = DefaultOperator;
                }
                
                Filters.Add(filter);
            }
            return this;
        }

        /// <summary>
        /// Odstráni filter zo setu
        /// </summary>
        public MultiColumnFilterSet RemoveFilter(string filterId)
        {
            Filters.RemoveAll(f => f.Id == filterId);
            return this;
        }

        /// <summary>
        /// Vyčistí všetky filtre
        /// </summary>
        public MultiColumnFilterSet ClearFilters()
        {
            Filters.Clear();
            return this;
        }

        /// <summary>
        /// Získa filtre pre konkrétny stĺpec
        /// </summary>
        public List<AdvancedFilter> GetFiltersForColumn(string columnName)
        {
            return Filters
                .Where(f => f.IsEnabled && f.ColumnName == columnName)
                .ToList();
        }

        #endregion

        #region Filter Execution

        /// <summary>
        /// Aplikuje všetky filtre na row data
        /// </summary>
        public bool ApplyFilters(Dictionary<string, object?> rowData)
        {
            if (!IsEnabled || !Filters.Any(f => f.IsEnabled))
                return true;

            var enabledFilters = Filters.Where(f => f.IsEnabled).ToList();
            if (!enabledFilters.Any())
                return true;

            // Evaluate first filter
            bool result = enabledFilters.First().ApplyFilter(rowData);

            // Combine with rest using logical operators
            for (int i = 1; i < enabledFilters.Count; i++)
            {
                var filter = enabledFilters[i];
                var filterResult = filter.ApplyFilter(rowData);

                result = filter.LogicalOperator switch
                {
                    LogicalOperator.And => result && filterResult,
                    LogicalOperator.Or => result || filterResult,
                    _ => result && filterResult
                };
            }

            return result;
        }

        /// <summary>
        /// Batch filtering pre multiple rows
        /// </summary>
        public List<Dictionary<string, object?>> ApplyFiltersToRows(List<Dictionary<string, object?>> rows)
        {
            if (!IsEnabled || !Filters.Any(f => f.IsEnabled))
                return rows;

            return rows.Where(ApplyFilters).ToList();
        }

        #endregion

        #region Quick Filter Builders

        /// <summary>
        /// Pridá text filter
        /// </summary>
        public MultiColumnFilterSet AddTextFilter(string columnName, string value, FilterOperator op = FilterOperator.Contains)
        {
            var filter = new AdvancedFilter
            {
                Name = $"Text_{columnName}",
                ColumnName = columnName,
                Operator = op,
                Value = value
            };
            return AddFilter(filter);
        }

        /// <summary>
        /// Pridá number range filter
        /// </summary>
        public MultiColumnFilterSet AddNumberRangeFilter(string columnName, decimal minValue, decimal maxValue)
        {
            var filter = new AdvancedFilter
            {
                Name = $"Range_{columnName}",
                ColumnName = columnName,
                Operator = FilterOperator.Between,
                Value = minValue,
                SecondValue = maxValue
            };
            return AddFilter(filter);
        }

        /// <summary>
        /// Pridá empty/not empty filter
        /// </summary>
        public MultiColumnFilterSet AddEmptyFilter(string columnName, bool isEmpty = true)
        {
            var filter = new AdvancedFilter
            {
                Name = $"Empty_{columnName}",
                ColumnName = columnName,
                Operator = isEmpty ? FilterOperator.IsEmpty : FilterOperator.IsNotEmpty
            };
            return AddFilter(filter);
        }

        /// <summary>
        /// Pridá regex filter
        /// </summary>
        public MultiColumnFilterSet AddRegexFilter(string columnName, string pattern, bool caseSensitive = false)
        {
            var filter = new AdvancedFilter
            {
                Name = $"Regex_{columnName}",
                ColumnName = columnName,
                Operator = FilterOperator.Regex,
                Value = pattern,
                CaseSensitive = caseSensitive
            };
            return AddFilter(filter);
        }

        #endregion

        #region Diagnostics

        /// <summary>
        /// Diagnostic info o filter set
        /// </summary>
        public string GetDiagnosticInfo()
        {
            var totalFilters = Filters.Count;
            var enabledFilters = Filters.Count(f => f.IsEnabled);
            var columnGroups = Filters.GroupBy(f => f.ColumnName).Count();

            return $"FilterSet '{Name}' - Total: {totalFilters}, Enabled: {enabledFilters}, " +
                   $"Columns: {columnGroups}, DefaultOp: {DefaultOperator}";
        }

        #endregion
    }
}