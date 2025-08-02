// AdvancedFilter.cs - ✅ PUBLIC API pre advanced filtering
using System;
using System.Collections.Generic;
using System.Linq;

namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid
{
    /// <summary>
    /// PUBLIC API pre advanced multi-column filtering
    /// </summary>
    public class AdvancedFilter
    {
        #region Public Properties

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

        #region Static Factory Methods

        /// <summary>
        /// Vytvorí text filter
        /// </summary>
        public static AdvancedFilter Text(string columnName, string value, FilterOperator op = FilterOperator.Contains, bool caseSensitive = false)
        {
            return new AdvancedFilter
            {
                Name = $"Text_{columnName}",
                ColumnName = columnName,
                Operator = op,
                Value = value,
                CaseSensitive = caseSensitive
            };
        }

        /// <summary>
        /// Vytvorí equals filter
        /// </summary>
        public static AdvancedFilter Equals(string columnName, object value)
        {
            return new AdvancedFilter
            {
                Name = $"Equals_{columnName}",
                ColumnName = columnName,
                Operator = FilterOperator.Equals,
                Value = value
            };
        }

        /// <summary>
        /// Vytvorí contains filter
        /// </summary>
        public static AdvancedFilter Contains(string columnName, string value, bool caseSensitive = false)
        {
            return Text(columnName, value, FilterOperator.Contains, caseSensitive);
        }

        /// <summary>
        /// Vytvorí starts with filter
        /// </summary>
        public static AdvancedFilter StartsWith(string columnName, string value, bool caseSensitive = false)
        {
            return Text(columnName, value, FilterOperator.StartsWith, caseSensitive);
        }

        /// <summary>
        /// Vytvorí number range filter
        /// </summary>
        public static AdvancedFilter NumberRange(string columnName, decimal minValue, decimal maxValue)
        {
            return new AdvancedFilter
            {
                Name = $"Range_{columnName}",
                ColumnName = columnName,
                Operator = FilterOperator.Between,
                Value = minValue,
                SecondValue = maxValue
            };
        }

        /// <summary>
        /// Vytvorí greater than filter
        /// </summary>
        public static AdvancedFilter GreaterThan(string columnName, decimal value)
        {
            return new AdvancedFilter
            {
                Name = $"GT_{columnName}",
                ColumnName = columnName,
                Operator = FilterOperator.GreaterThan,
                Value = value
            };
        }

        /// <summary>
        /// Vytvorí less than filter
        /// </summary>
        public static AdvancedFilter LessThan(string columnName, decimal value)
        {
            return new AdvancedFilter
            {
                Name = $"LT_{columnName}",
                ColumnName = columnName,
                Operator = FilterOperator.LessThan,
                Value = value
            };
        }

        /// <summary>
        /// Vytvorí empty filter
        /// </summary>
        public static AdvancedFilter IsEmpty(string columnName)
        {
            return new AdvancedFilter
            {
                Name = $"Empty_{columnName}",
                ColumnName = columnName,
                Operator = FilterOperator.IsEmpty
            };
        }

        /// <summary>
        /// Vytvorí not empty filter
        /// </summary>
        public static AdvancedFilter IsNotEmpty(string columnName)
        {
            return new AdvancedFilter
            {
                Name = $"NotEmpty_{columnName}",
                ColumnName = columnName,
                Operator = FilterOperator.IsNotEmpty
            };
        }

        /// <summary>
        /// Vytvorí in list filter
        /// </summary>
        public static AdvancedFilter In(string columnName, params string[] values)
        {
            return new AdvancedFilter
            {
                Name = $"In_{columnName}",
                ColumnName = columnName,
                Operator = FilterOperator.In,
                Value = string.Join(",", values)
            };
        }

        /// <summary>
        /// Vytvorí regex filter
        /// </summary>
        public static AdvancedFilter Regex(string columnName, string pattern, bool caseSensitive = false)
        {
            return new AdvancedFilter
            {
                Name = $"Regex_{columnName}",
                ColumnName = columnName,
                Operator = FilterOperator.Regex,
                Value = pattern,
                CaseSensitive = caseSensitive
            };
        }

        #endregion
    }

    /// <summary>
    /// Filter operators - PUBLIC
    /// </summary>
    public enum FilterOperator
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
    /// Logical operators - PUBLIC
    /// </summary>
    public enum LogicalOperator
    {
        And,
        Or
    }

    /// <summary>
    /// Multi-column filter set - PUBLIC
    /// </summary>
    public class FilterSet
    {
        #region Public Properties

        /// <summary>
        /// Názov filter set
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Všetky filtre v sete (READ-ONLY)
        /// </summary>
        public IReadOnlyList<AdvancedFilter> Filters => _filters.AsReadOnly();

        /// <summary>
        /// Či je filter set enabled
        /// </summary>
        public bool IsEnabled { get; set; } = true;

        /// <summary>
        /// Default logical operator medzi filtrami
        /// </summary>
        public LogicalOperator DefaultOperator { get; set; } = LogicalOperator.And;

        #endregion

        #region Private Fields

        private readonly List<AdvancedFilter> _filters = new();

        #endregion

        #region Static Factory Methods

        /// <summary>
        /// Vytvorí nový FilterSet
        /// </summary>
        public static FilterSet Create(string name)
        {
            return new FilterSet { Name = name };
        }

        /// <summary>
        /// Vytvorí employee filter set
        /// </summary>
        public static FilterSet CreateEmployeeFilterSet()
        {
            return FilterSet.Create("EmployeeFilters")
                .AddFilter(AdvancedFilter.IsNotEmpty("FirstName"))
                .AddFilter(AdvancedFilter.IsNotEmpty("LastName"))
                .AddFilter(AdvancedFilter.Contains("Email", "@"))
                .WithOperator(LogicalOperator.And);
        }

        /// <summary>
        /// Vytvorí project filter set
        /// </summary>
        public static FilterSet CreateProjectFilterSet()
        {
            return FilterSet.Create("ProjectFilters")
                .AddFilter(AdvancedFilter.In("Status", "Active", "Planning", "Completed"))
                .AddFilter(AdvancedFilter.GreaterThan("Budget", 0))
                .WithOperator(LogicalOperator.And);
        }

        #endregion

        #region Filter Management

        /// <summary>
        /// Pridá filter do setu
        /// </summary>
        public FilterSet AddFilter(AdvancedFilter filter)
        {
            if (filter != null && !_filters.Any(f => f.Id == filter.Id))
            {
                if (_filters.Any())
                {
                    filter.LogicalOperator = DefaultOperator;
                }
                _filters.Add(filter);
            }
            return this;
        }

        /// <summary>
        /// Pridá viacero filtrov
        /// </summary>
        public FilterSet AddFilters(params AdvancedFilter[] filters)
        {
            foreach (var filter in filters)
            {
                AddFilter(filter);
            }
            return this;
        }

        /// <summary>
        /// Odstráni filter zo setu
        /// </summary>
        public FilterSet RemoveFilter(string filterId)
        {
            _filters.RemoveAll(f => f.Id == filterId);
            return this;
        }

        /// <summary>
        /// Vyčistí všetky filtre
        /// </summary>
        public FilterSet ClearFilters()
        {
            _filters.Clear();
            return this;
        }

        /// <summary>
        /// Nastaví default operator
        /// </summary>
        public FilterSet WithOperator(LogicalOperator logicalOperator)
        {
            DefaultOperator = logicalOperator;
            return this;
        }

        #endregion

        #region Quick Filter Helpers

        /// <summary>
        /// Pridá text filter
        /// </summary>
        public FilterSet AddTextFilter(string columnName, string value, FilterOperator op = FilterOperator.Contains)
        {
            return AddFilter(AdvancedFilter.Text(columnName, value, op));
        }

        /// <summary>
        /// Pridá number range filter
        /// </summary>
        public FilterSet AddNumberRangeFilter(string columnName, decimal minValue, decimal maxValue)
        {
            return AddFilter(AdvancedFilter.NumberRange(columnName, minValue, maxValue));
        }

        /// <summary>
        /// Pridá empty filter
        /// </summary>
        public FilterSet AddEmptyFilter(string columnName, bool isEmpty = true)
        {
            return AddFilter(isEmpty ? AdvancedFilter.IsEmpty(columnName) : AdvancedFilter.IsNotEmpty(columnName));
        }

        /// <summary>
        /// Pridá regex filter
        /// </summary>
        public FilterSet AddRegexFilter(string columnName, string pattern, bool caseSensitive = false)
        {
            return AddFilter(AdvancedFilter.Regex(columnName, pattern, caseSensitive));
        }

        #endregion

        #region Diagnostics

        /// <summary>
        /// Diagnostic info o filter set
        /// </summary>
        public string GetDiagnosticInfo()
        {
            var totalFilters = _filters.Count;
            var enabledFilters = _filters.Count(f => f.IsEnabled);
            var columnGroups = _filters.GroupBy(f => f.ColumnName).Count();

            return $"FilterSet '{Name}' - Total: {totalFilters}, Enabled: {enabledFilters}, " +
                   $"Columns: {columnGroups}, DefaultOp: {DefaultOperator}";
        }

        #endregion
    }
}