// Models/Validation/CrossRowValidationRule.cs - ✅ NOVÉ: Cross-row Validation Rules
using System;
using System.Collections.Generic;
using System.Linq;

namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Models.Validation
{
    /// <summary>
    /// ✅ NOVÉ: Validačné pravidlo závislé od iných riadkov
    /// </summary>
    public class CrossRowValidationRule
    {
        /// <summary>
        /// Názov stĺpca ktorý sa validuje
        /// </summary>
        public string ColumnName { get; set; } = string.Empty;

        /// <summary>
        /// Typ cross-row validácie
        /// </summary>
        public CrossRowValidationType ValidationType { get; set; }

        /// <summary>
        /// Názvy stĺpcov ktoré sa majú porovnávať (pre unique constraints)
        /// </summary>
        public List<string> ComparisonColumns { get; set; } = new();

        /// <summary>
        /// Custom validation function pre komplexné scenáre
        /// </summary>
        public Func<Dictionary<string, object?>, List<Dictionary<string, object?>>, CrossRowValidationResult>? CustomValidationFunction { get; set; }

        /// <summary>
        /// Error message pre validačnú chybu
        /// </summary>
        public string ErrorMessage { get; set; } = "Cross-row validation failed";

        /// <summary>
        /// Severity úrovne validačnej chyby
        /// </summary>
        public ValidationSeverity Severity { get; set; } = ValidationSeverity.Error;

        /// <summary>
        /// Či je pravidlo povolené
        /// </summary>
        public bool IsEnabled { get; set; } = true;

        /// <summary>
        /// Scope validácie (celý dataset vs visible rows)
        /// </summary>
        public ValidationScope Scope { get; set; } = ValidationScope.AllRows;

        /// <summary>
        /// Validuje konfiguráciu pravidla
        /// </summary>
        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(ColumnName))
                throw new ArgumentException("ColumnName nemôže byť prázdny");

            if (ValidationType == CrossRowValidationType.UniqueConstraint && !ComparisonColumns.Any())
                throw new ArgumentException("UniqueConstraint vyžaduje aspoň jeden ComparisonColumn");

            if (ValidationType == CrossRowValidationType.Custom && CustomValidationFunction == null)
                throw new ArgumentException("Custom validation vyžaduje CustomValidationFunction");

            if (string.IsNullOrWhiteSpace(ErrorMessage))
                throw new ArgumentException("ErrorMessage nemôže byť prázdna");
        }

        /// <summary>
        /// Vytvorí unique constraint rule
        /// </summary>
        public static CrossRowValidationRule CreateUniqueConstraint(string columnName, string errorMessage = "")
        {
            return new CrossRowValidationRule
            {
                ColumnName = columnName,
                ValidationType = CrossRowValidationType.UniqueConstraint,
                ComparisonColumns = new List<string> { columnName },
                ErrorMessage = string.IsNullOrEmpty(errorMessage) 
                    ? $"Value in {columnName} must be unique" 
                    : errorMessage
            };
        }

        /// <summary>
        /// Vytvorí composite unique constraint rule
        /// </summary>
        public static CrossRowValidationRule CreateCompositeUniqueConstraint(
            List<string> columnNames, 
            string errorMessage = "")
        {
            return new CrossRowValidationRule
            {
                ColumnName = columnNames.First(),
                ValidationType = CrossRowValidationType.CompositeUniqueConstraint,
                ComparisonColumns = columnNames,
                ErrorMessage = string.IsNullOrEmpty(errorMessage) 
                    ? $"Combination of {string.Join(", ", columnNames)} must be unique" 
                    : errorMessage
            };
        }

        /// <summary>
        /// Vytvorí dependency validation rule
        /// </summary>
        public static CrossRowValidationRule CreateDependencyConstraint(
            string columnName,
            string dependentColumn,
            string errorMessage = "")
        {
            return new CrossRowValidationRule
            {
                ColumnName = columnName,
                ValidationType = CrossRowValidationType.DependencyConstraint,
                ComparisonColumns = new List<string> { dependentColumn },
                ErrorMessage = string.IsNullOrEmpty(errorMessage) 
                    ? $"{columnName} depends on {dependentColumn}" 
                    : errorMessage
            };
        }

        public override string ToString()
        {
            return $"CrossRowRule: {ColumnName} ({ValidationType}) - {ErrorMessage}";
        }
    }

    /// <summary>
    /// ✅ NOVÉ: Typ cross-row validácie
    /// </summary>
    public enum CrossRowValidationType
    {
        /// <summary>
        /// Hodnota musí byť jedinečná v stĺpci
        /// </summary>
        UniqueConstraint,

        /// <summary>
        /// Kombinácia hodnôt musí byť jedinečná
        /// </summary>
        CompositeUniqueConstraint,

        /// <summary>
        /// Hodnota závisí od iného riadku/stĺpca
        /// </summary>
        DependencyConstraint,

        /// <summary>
        /// Hierarchická validácia (parent-child relationships)
        /// </summary>
        HierarchicalConstraint,

        /// <summary>
        /// Custom cross-row logic
        /// </summary>
        Custom
    }


    /// <summary>
    /// ✅ NOVÉ: Scope validácie
    /// </summary>
    public enum ValidationScope
    {
        /// <summary>
        /// Validovať všetky riadky v datasete
        /// </summary>
        AllRows,

        /// <summary>
        /// Validovať len visible/rendered riadky
        /// </summary>
        VisibleRows,

        /// <summary>
        /// Validovať len modified riadky
        /// </summary>
        ModifiedRows
    }

    /// <summary>
    /// ✅ NOVÉ: Výsledok cross-row validácie
    /// </summary>
    public class CrossRowValidationResult
    {
        /// <summary>
        /// Či je validácia úspešná
        /// </summary>
        public bool IsValid { get; set; } = true;

        /// <summary>
        /// Error message ak validácia zlyhala
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// Severity chyby
        /// </summary>
        public ValidationSeverity Severity { get; set; } = ValidationSeverity.Error;

        /// <summary>
        /// Indexy riadkov ktoré sú v konflikte
        /// </summary>
        public List<int> ConflictingRowIndices { get; set; } = new();

        /// <summary>
        /// Dodatočné detaily o chybe
        /// </summary>
        public Dictionary<string, object?> Details { get; set; } = new();

        /// <summary>
        /// Vytvorí successful result
        /// </summary>
        public static CrossRowValidationResult Success()
        {
            return new CrossRowValidationResult { IsValid = true };
        }

        /// <summary>
        /// Vytvorí failed result
        /// </summary>
        public static CrossRowValidationResult Failure(
            string errorMessage, 
            ValidationSeverity severity = ValidationSeverity.Error,
            params int[] conflictingRows)
        {
            return new CrossRowValidationResult
            {
                IsValid = false,
                ErrorMessage = errorMessage,
                Severity = severity,
                ConflictingRowIndices = conflictingRows.ToList()
            };
        }

        public override string ToString()
        {
            return IsValid 
                ? "Valid" 
                : $"{Severity}: {ErrorMessage} (Conflicts: {ConflictingRowIndices.Count} rows)";
        }
    }

    /// <summary>
    /// ✅ NOVÉ: Cross-row validation context
    /// </summary>
    public class CrossRowValidationContext
    {
        /// <summary>
        /// Index aktuálneho riadku ktorý sa validuje
        /// </summary>
        public int CurrentRowIndex { get; set; }

        /// <summary>
        /// Dáta aktuálneho riadku
        /// </summary>
        public Dictionary<string, object?> CurrentRowData { get; set; } = new();

        /// <summary>
        /// Všetky riadky v datasete
        /// </summary>
        public List<Dictionary<string, object?>> AllRowsData { get; set; } = new();

        /// <summary>
        /// Len ostatné riadky (bez aktuálneho)
        /// </summary>
        public List<Dictionary<string, object?>> OtherRowsData => 
            AllRowsData.Where((row, index) => index != CurrentRowIndex).ToList();

        /// <summary>
        /// Metadata o stĺpcoch
        /// </summary>
        public Dictionary<string, object?> ColumnMetadata { get; set; } = new();

        /// <summary>
        /// Získa hodnotu z aktuálneho riadku
        /// </summary>
        public T? GetCurrentValue<T>(string columnName)
        {
            if (CurrentRowData.TryGetValue(columnName, out var value))
            {
                if (value is T typedValue)
                    return typedValue;
                
                try
                {
                    return (T?)Convert.ChangeType(value, typeof(T));
                }
                catch
                {
                    return default(T);
                }
            }
            return default(T);
        }

        /// <summary>
        /// Získa všetky hodnoty z iných riadkov pre daný stĺpec
        /// </summary>
        public List<T> GetOtherValues<T>(string columnName)
        {
            var values = new List<T>();
            
            foreach (var row in OtherRowsData)
            {
                if (row.TryGetValue(columnName, out var value))
                {
                    try
                    {
                        if (value is T typedValue)
                            values.Add(typedValue);
                        else if (value != null)
                            values.Add((T)Convert.ChangeType(value, typeof(T)));
                    }
                    catch
                    {
                        // Skip invalid conversions
                    }
                }
            }
            
            return values;
        }
    }
}