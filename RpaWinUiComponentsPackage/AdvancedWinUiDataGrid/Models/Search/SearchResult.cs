// Models/SearchResult.cs - ✅ PUBLIC model pre Advanced Search results
using System;
using System.Collections.Generic;
using System.Linq;

namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Models.Search
{
    /// <summary>
    /// Výsledok advanced search operácie - ✅ PUBLIC API
    /// </summary>
    public class SearchResult
    {
        /// <summary>
        /// Unique ID search result
        /// </summary>
        public string Id { get; set; } = Guid.NewGuid().ToString("N")[..8];

        /// <summary>
        /// Index riadku v originálnych dátach
        /// </summary>
        public int RowIndex { get; set; }

        /// <summary>
        /// Dáta riadku
        /// </summary>
        public Dictionary<string, object?> RowData { get; set; } = new();

        /// <summary>
        /// Search matches v riadku
        /// </summary>
        public List<SearchMatch> Matches { get; set; } = new();

        /// <summary>
        /// Overall relevance score (0.0 - 1.0)
        /// </summary>
        public double RelevanceScore { get; set; } = 1.0;

        /// <summary>
        /// Počet matches v riadku
        /// </summary>
        public int MatchCount => Matches.Count;

        /// <summary>
        /// Najlepší match v riadku
        /// </summary>
        public SearchMatch? BestMatch => Matches.OrderByDescending(m => m.MatchScore).FirstOrDefault();

        /// <summary>
        /// Stĺpce kde boli nájdené matches
        /// </summary>
        public List<string> MatchedColumns => Matches.Select(m => m.ColumnName).Distinct().ToList();

        /// <summary>
        /// Či obsahuje exact match
        /// </summary>
        public bool HasExactMatch => Matches.Any(m => m.IsExactMatch);

        /// <summary>
        /// Či obsahuje fuzzy match
        /// </summary>
        public bool HasFuzzyMatch => Matches.Any(m => m.IsFuzzyMatch);

        /// <summary>
        /// Či obsahuje regex match
        /// </summary>
        public bool HasRegexMatch => Matches.Any(m => m.IsRegexMatch);

        /// <summary>
        /// Vytvorí SearchResult
        /// </summary>
        public SearchResult() { }

        /// <summary>
        /// Vytvorí SearchResult s dátami
        /// </summary>
        public SearchResult(int rowIndex, Dictionary<string, object?> rowData)
        {
            RowIndex = rowIndex;
            RowData = rowData ?? new Dictionary<string, object?>();
        }

        /// <summary>
        /// Pridá search match
        /// </summary>
        public void AddMatch(SearchMatch match)
        {
            if (match != null)
            {
                Matches.Add(match);
                RecalculateRelevanceScore();
            }
        }

        /// <summary>
        /// Prepočíta relevance score na základe matches
        /// </summary>
        private void RecalculateRelevanceScore()
        {
            if (!Matches.Any())
            {
                RelevanceScore = 0.0;
                return;
            }

            // Váhovaný priemer match scores
            var exactMatches = Matches.Where(m => m.IsExactMatch).ToList();
            var fuzzyMatches = Matches.Where(m => m.IsFuzzyMatch).ToList();
            var regexMatches = Matches.Where(m => m.IsRegexMatch).ToList();

            double score = 0.0;
            double totalWeight = 0.0;

            // Exact matches majú najvyššiu váhu
            if (exactMatches.Any())
            {
                score += exactMatches.Average(m => m.MatchScore) * 1.0;
                totalWeight += 1.0;
            }

            // Regex matches majú strednú váhu
            if (regexMatches.Any())
            {
                score += regexMatches.Average(m => m.MatchScore) * 0.8;
                totalWeight += 0.8;
            }

            // Fuzzy matches majú najnižšiu váhu
            if (fuzzyMatches.Any())
            {
                score += fuzzyMatches.Average(m => m.MatchScore) * 0.6;
                totalWeight += 0.6;
            }

            RelevanceScore = totalWeight > 0 ? score / totalWeight : 0.0;

            // Bonus za viacero matched columns
            if (MatchedColumns.Count > 1)
            {
                RelevanceScore *= 1.1; // 10% bonus
            }

            // Zabezpeč že score je v rozsahu 0-1
            RelevanceScore = Math.Min(1.0, Math.Max(0.0, RelevanceScore));
        }

        /// <summary>
        /// Získa highlighted text pre stĺpec
        /// </summary>
        public string GetHighlightedText(string columnName, AdvancedSearchConfiguration config)
        {
            if (!RowData.TryGetValue(columnName, out var cellValue))
                return string.Empty;

            var originalText = cellValue?.ToString() ?? string.Empty;
            var columnMatches = Matches.Where(m => m.ColumnName == columnName).ToList();

            if (!columnMatches.Any() || !config.EnableSearchHighlighting)
                return originalText;

            // Aplikuj highlighting na všetky matches v stĺpci
            var highlightedText = originalText;
            
            // Sorted by position (descending) aby sme nezmenili indexy pri replacement
            var sortedMatches = columnMatches
                .Where(m => m.StartPosition >= 0 && m.Length > 0)
                .OrderByDescending(m => m.StartPosition)
                .ToList();

            foreach (var match in sortedMatches)
            {
                if (match.StartPosition < highlightedText.Length && 
                    match.StartPosition + match.Length <= highlightedText.Length)
                {
                    var beforeText = highlightedText.Substring(0, match.StartPosition);
                    var matchText = highlightedText.Substring(match.StartPosition, match.Length);
                    var afterText = highlightedText.Substring(match.StartPosition + match.Length);

                    var highlightedMatch = $"<mark style=\"background-color: {config.HighlightBackgroundColor}; color: {config.HighlightTextColor};\">{matchText}</mark>";
                    
                    highlightedText = beforeText + highlightedMatch + afterText;
                }
            }

            return highlightedText;
        }

        /// <summary>
        /// Získa summary search result
        /// </summary>
        public string GetSummary()
        {
            var matchTypes = new List<string>();
            if (HasExactMatch) matchTypes.Add("Exact");
            if (HasRegexMatch) matchTypes.Add("Regex");
            if (HasFuzzyMatch) matchTypes.Add("Fuzzy");

            return $"Row {RowIndex}: {MatchCount} matches in {MatchedColumns.Count} columns " +
                   $"[{string.Join(", ", matchTypes)}], Score: {RelevanceScore:F2}";
        }

        public override string ToString()
        {
            return GetSummary();
        }
    }

    /// <summary>
    /// Jednotlivý search match v bunke - ✅ PUBLIC API
    /// </summary>
    public class SearchMatch
    {
        /// <summary>
        /// Názov stĺpca
        /// </summary>
        public string ColumnName { get; set; } = string.Empty;

        /// <summary>
        /// Originálny search term
        /// </summary>
        public string SearchTerm { get; set; } = string.Empty;

        /// <summary>
        /// Nájdený text
        /// </summary>
        public string MatchedText { get; set; } = string.Empty;

        /// <summary>
        /// Pozícia v texte (pre highlighting)
        /// </summary>
        public int StartPosition { get; set; } = -1;

        /// <summary>
        /// Dĺžka matched text (pre highlighting)
        /// </summary>
        public int Length { get; set; } = 0;

        /// <summary>
        /// Match score (0.0 - 1.0)
        /// </summary>
        public double MatchScore { get; set; } = 1.0;

        /// <summary>
        /// Typ match
        /// </summary>
        public SearchMatchType MatchType { get; set; } = SearchMatchType.Exact;

        /// <summary>
        /// Či je exact match
        /// </summary>
        public bool IsExactMatch => MatchType == SearchMatchType.Exact;

        /// <summary>
        /// Či je fuzzy match
        /// </summary>
        public bool IsFuzzyMatch => MatchType == SearchMatchType.Fuzzy;

        /// <summary>
        /// Či je regex match
        /// </summary>
        public bool IsRegexMatch => MatchType == SearchMatchType.Regex;

        /// <summary>
        /// Či je case-sensitive match
        /// </summary>
        public bool IsCaseSensitive { get; set; } = false;

        /// <summary>
        /// Či je whole word match
        /// </summary>
        public bool IsWholeWord { get; set; } = false;

        /// <summary>
        /// Vytvorí SearchMatch
        /// </summary>
        public SearchMatch() { }

        /// <summary>
        /// Vytvorí SearchMatch s parametrami
        /// </summary>
        public SearchMatch(string columnName, string searchTerm, string matchedText, 
                          int startPosition, int length, SearchMatchType matchType = SearchMatchType.Exact)
        {
            ColumnName = columnName;
            SearchTerm = searchTerm;
            MatchedText = matchedText;
            StartPosition = startPosition;
            Length = length;
            MatchType = matchType;
            MatchScore = matchType == SearchMatchType.Exact ? 1.0 : 0.8;
        }

        public override string ToString()
        {
            return $"{ColumnName}: '{SearchTerm}' → '{MatchedText}' " +
                   $"({MatchType}, Score: {MatchScore:F2})";
        }
    }

    /// <summary>
    /// Typ search match
    /// </summary>
    public enum SearchMatchType
    {
        /// <summary>
        /// Presná zhoda
        /// </summary>
        Exact,

        /// <summary>
        /// Fuzzy match s toleranciou
        /// </summary>
        Fuzzy,

        /// <summary>
        /// Regex pattern match
        /// </summary>
        Regex,

        /// <summary>
        /// Partial substring match
        /// </summary>
        Partial,

        /// <summary>
        /// Case-insensitive match
        /// </summary>
        CaseInsensitive
    }

}