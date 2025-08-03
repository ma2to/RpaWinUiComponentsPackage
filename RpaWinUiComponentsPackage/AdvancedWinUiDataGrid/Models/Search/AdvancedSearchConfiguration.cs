// Models/AdvancedSearchConfiguration.cs - ✅ PUBLIC konfigurácia pre Advanced Search
using System;
using System.Collections.Generic;

namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Models.Search
{
    /// <summary>
    /// Konfigurácia pre Advanced Search funkcionalitu - ✅ PUBLIC API
    /// </summary>
    public class AdvancedSearchConfiguration
    {
        /// <summary>
        /// Povoliť fuzzy search (default: true)
        /// </summary>
        public bool EnableFuzzySearch { get; set; } = true;

        /// <summary>
        /// Fuzzy search tolerance (0.0 = exact, 1.0 = very tolerant, default: 0.3)
        /// </summary>
        public double FuzzyTolerance { get; set; } = 0.3;

        /// <summary>
        /// Povoliť regex search (default: true)
        /// </summary>
        public bool EnableRegexSearch { get; set; } = true;

        /// <summary>
        /// Povoliť search highlighting (default: true)
        /// </summary>
        public bool EnableSearchHighlighting { get; set; } = true;

        /// <summary>
        /// Highlighting background color (default: žltá)
        /// </summary>
        public string HighlightBackgroundColor { get; set; } = "#FFFF00";

        /// <summary>
        /// Highlighting text color (default: čierna)
        /// </summary>
        public string HighlightTextColor { get; set; } = "#000000";

        /// <summary>
        /// Povoliť case-sensitive search (default: false)
        /// </summary>
        public bool EnableCaseSensitiveSearch { get; set; } = false;

        /// <summary>
        /// Povoliť whole word search (default: false)
        /// </summary>
        public bool EnableWholeWordSearch { get; set; } = false;

        /// <summary>
        /// Povoliť search v skrytých stĺpcoch (default: false)
        /// </summary>
        public bool SearchInHiddenColumns { get; set; } = false;

        /// <summary>
        /// Maximálny počet search results pre highlighting (default: 1000)
        /// </summary>
        public int MaxHighlightResults { get; set; } = 1000;

        /// <summary>
        /// Search debounce time v ms (default: 300)
        /// </summary>
        public int SearchDebounceMs { get; set; } = 300;

        /// <summary>
        /// Povoliť search history (default: true)
        /// </summary>
        public bool EnableSearchHistory { get; set; } = true;

        /// <summary>
        /// Maximálny počet search history items (default: 20)
        /// </summary>
        public int MaxSearchHistoryItems { get; set; } = 20;

        /// <summary>
        /// Search strategy pre multi-column search
        /// </summary>
        public SearchStrategy Strategy { get; set; } = SearchStrategy.Any;

        /// <summary>
        /// Predvolené konfigurácie
        /// </summary>
        public static AdvancedSearchConfiguration Default => new();

        public static AdvancedSearchConfiguration Fast => new()
        {
            EnableFuzzySearch = false,
            EnableRegexSearch = true,
            EnableSearchHighlighting = false,
            SearchDebounceMs = 150,
            MaxHighlightResults = 500
        };

        public static AdvancedSearchConfiguration Comprehensive => new()
        {
            EnableFuzzySearch = true,
            FuzzyTolerance = 0.4,
            EnableRegexSearch = true,
            EnableSearchHighlighting = true,
            EnableSearchHistory = true,
            MaxSearchHistoryItems = 50,
            MaxHighlightResults = 2000
        };

        public static AdvancedSearchConfiguration BasicHighlight => new()
        {
            EnableFuzzySearch = false,
            EnableRegexSearch = false,
            EnableSearchHighlighting = true,
            SearchDebounceMs = 200,
            MaxHighlightResults = 500
        };

        /// <summary>
        /// Validuje konfiguráciu
        /// </summary>
        public bool IsValid()
        {
            return FuzzyTolerance >= 0.0 && FuzzyTolerance <= 1.0 &&
                   MaxHighlightResults > 0 && MaxHighlightResults <= 10000 &&
                   SearchDebounceMs >= 0 && SearchDebounceMs <= 2000 &&
                   MaxSearchHistoryItems > 0 && MaxSearchHistoryItems <= 100;
        }

        /// <summary>
        /// Získa popis konfigurácie
        /// </summary>
        public string GetDescription()
        {
            var features = new List<string>();
            
            if (EnableFuzzySearch) features.Add($"Fuzzy({FuzzyTolerance:F1})");
            if (EnableRegexSearch) features.Add("Regex");
            if (EnableSearchHighlighting) features.Add("Highlight");
            if (EnableCaseSensitiveSearch) features.Add("CaseSensitive");
            if (EnableWholeWordSearch) features.Add("WholeWord");
            if (EnableSearchHistory) features.Add($"History({MaxSearchHistoryItems})");

            return $"AdvancedSearch: [{string.Join(", ", features)}], " +
                   $"Debounce: {SearchDebounceMs}ms, Strategy: {Strategy}";
        }

        public override string ToString()
        {
            return GetDescription();
        }
    }

    /// <summary>
    /// Search strategy pre multi-column search
    /// </summary>
    public enum SearchStrategy
    {
        /// <summary>
        /// Nájde riadky kde akýkoľvek stĺpec vyhovuje (OR logika)
        /// </summary>
        Any,

        /// <summary>
        /// Nájde riadky kde všetky search terms sú nájdené (AND logika)
        /// </summary>
        All,

        /// <summary>
        /// Nájde riadky v presnom poradí search terms
        /// </summary>
        Exact,

        /// <summary>
        /// Fuzzy matching s tolerance
        /// </summary>
        Fuzzy
    }
}