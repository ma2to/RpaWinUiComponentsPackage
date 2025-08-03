// Services/Interfaces/IUIService.cs - ✅ NOVÝ: Base interface pre UI services
using System;
using System.Threading.Tasks;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Models.Grid;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Models.Search;

namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Services.Interfaces
{
    /// <summary>
    /// Base interface pre všetky UI services - INTERNAL
    /// </summary>
    internal interface IUIService
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
    /// Interface pre header management service - INTERNAL
    /// </summary>
    internal interface IHeaderManagementService : IUIService
    {
        /// <summary>
        /// Aktualizuje sort indikátor pre stĺpec
        /// </summary>
        void UpdateSortIndicator(string columnName, SortDirection sortDirection);

        /// <summary>
        /// Získa sort indikátor pre stĺpec
        /// </summary>
        SortDirection GetSortIndicator(string columnName);

        /// <summary>
        /// Vymaže všetky sort indikátory
        /// </summary>
        void ClearAllSortIndicators();

        /// <summary>
        /// Aktualizuje šírku stĺpca
        /// </summary>
        bool UpdateColumnWidth(string columnName, double newWidth);

        /// <summary>
        /// Získa šírku stĺpca
        /// </summary>
        double GetColumnWidth(string columnName);
    }

    /// <summary>
    /// Interface pre cell rendering service - INTERNAL
    /// </summary>
    internal interface ICellRenderingService : IUIService
    {
        /// <summary>
        /// Zebra rows sú povolené
        /// </summary>
        bool EnableZebraRows { get; set; }

        /// <summary>
        /// Aktualizuje color konfiguráciu
        /// </summary>
        void UpdateColorConfiguration(DataGridColorConfig colorConfig);

        /// <summary>
        /// Resetuje color konfiguráciu na default
        /// </summary>
        void ResetColorConfiguration();
    }

    /// <summary>
    /// Interface pre resize handling service - INTERNAL
    /// </summary>
    internal interface IResizeHandlingService : IUIService
    {
        /// <summary>
        /// Je práve v procese resizing
        /// </summary>
        bool IsResizing { get; }

        /// <summary>
        /// Názov stĺpca ktorý sa práve resize-uje
        /// </summary>
        string? CurrentResizingColumn { get; }

        /// <summary>
        /// Začne resize operáciu pre stĺpec
        /// </summary>
        bool StartResize(string columnName, double mouseX);

        /// <summary>
        /// Aktualizuje resize počas drag operácie
        /// </summary>
        bool UpdateResize(double currentMouseX);

        /// <summary>
        /// Ukončí resize operáciu
        /// </summary>
        bool EndResize();

        /// <summary>
        /// Zruší resize operáciu
        /// </summary>
        bool CancelResize();
    }
}