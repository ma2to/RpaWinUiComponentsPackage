// Services/Interfaces/IExportService.cs - ✅ OPRAVENÝ - len IExportService

using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Models;
using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Services;
using System.Data;
using System.Threading.Tasks;

namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Services.Interfaces
{
    /// <summary>
    /// Interface pre export služby DataGrid - INTERNAL
    /// </summary>
    internal interface IExportService
    {
        /// <summary>
        /// Inicializuje export službu s konfiguráciou
        /// </summary>
        Task InitializeAsync(GridConfiguration configuration);

        /// <summary>
        /// Exportuje všetky dáta do DataTable (bez DeleteRows stĺpca, s ValidAlerts)
        /// </summary>
        Task<DataTable> ExportToDataTableAsync();

        /// <summary>
        /// Exportuje len validné riadky do DataTable
        /// </summary>
        Task<DataTable> ExportValidRowsOnlyAsync();

        /// <summary>
        /// Exportuje len nevalidné riadky do DataTable (pre debugging)
        /// </summary>
        Task<DataTable> ExportInvalidRowsOnlyAsync();

        /// <summary>
        /// Exportuje len špecifické stĺpce
        /// </summary>
        Task<DataTable> ExportSpecificColumnsAsync(string[] columnNames);

        /// <summary>
        /// Exportuje dáta do CSV formátu
        /// </summary>
        Task<string> ExportToCsvAsync(bool includeHeaders = true);

        /// <summary>
        /// Získa štatistiky exportovaných dát
        /// </summary>
        Task<ExportStatistics> GetExportStatisticsAsync();
    }
}