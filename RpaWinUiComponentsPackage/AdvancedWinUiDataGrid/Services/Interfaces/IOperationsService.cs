// Services/Interfaces/IOperationsService.cs - ✅ NOVÝ: Base interface pre Operations services
using System;
using System.Threading.Tasks;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Models.Search;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Models.ImportExport;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Models.Validation;
using System.Collections.Generic;

namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Services.Interfaces
{
    /// <summary>
    /// Base interface pre všetky Operations services - INTERNAL
    /// </summary>
    internal interface IOperationsService
    {
        /// <summary>
        /// Service je inicializovaný a ready na použitie
        /// </summary>
        bool IsInitialized { get; }

        /// <summary>
        /// Inicializuje service
        /// </summary>
        Task InitializeAsync();

        /// <summary>
        /// Diagnostické informácie
        /// </summary>
        string GetDiagnosticInfo();
    }

    /// <summary>
    /// Interface pre search and sort operations service - INTERNAL
    /// </summary>
    internal interface ISearchAndSortService : IOperationsService
    {
        /// <summary>
        /// Vykoná search operáciu
        /// </summary>
        Task<SearchResult> SearchAsync(string searchTerm, AdvancedSearchConfiguration? config = null);

        /// <summary>
        /// Vykoná sort operáciu
        /// </summary>
        Task SortAsync(string columnName, SortDirection direction);

        /// <summary>
        /// Vykoná multi-sort operáciu
        /// </summary>
        Task MultiSortAsync(System.Collections.Generic.List<MultiSortColumn> sortColumns);

        /// <summary>
        /// Vymaže všetky search filtre
        /// </summary>
        void ClearAllFilters();

        /// <summary>
        /// Vymaže všetky sort operácie
        /// </summary>
        void ClearAllSorts();
    }

    /// <summary>
    /// Interface pre export/import operations service - INTERNAL
    /// </summary>
    internal interface IExportImportService : IOperationsService
    {
        /// <summary>
        /// Exportuje dáta do súboru
        /// </summary>
        Task<bool> ExportToFileAsync(string filePath, ImportExportConfiguration config);

        /// <summary>
        /// Importuje dáta zo súboru
        /// </summary>
        Task<ImportResult> ImportFromFileAsync(string filePath, ImportExportConfiguration config);

        /// <summary>
        /// Kontroluje či súbor je kompatibilný pre import
        /// </summary>
        Task<bool> CanImportFileAsync(string filePath);

        /// <summary>
        /// Získa podporované export formáty
        /// </summary>
        System.Collections.Generic.List<string> GetSupportedExportFormats();

        /// <summary>
        /// Získa podporované import formáty
        /// </summary>
        System.Collections.Generic.List<string> GetSupportedImportFormats();
    }
}