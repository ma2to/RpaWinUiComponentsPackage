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
        Task<DataTable> ExportToDataTableAsync(bool includeValidAlerts = false);

        /// <summary>
        /// Exportuje len validné riadky do DataTable
        /// </summary>
        Task<DataTable> ExportValidRowsOnlyAsync(bool includeValidAlerts = false);

        /// <summary>
        /// Exportuje len nevalidné riadky do DataTable (pre debugging)
        /// </summary>
        Task<DataTable> ExportInvalidRowsOnlyAsync(bool includeValidAlerts = true);

        /// <summary>
        /// Exportuje len špecifické stĺpce
        /// </summary>
        Task<DataTable> ExportSpecificColumnsAsync(string[] columnNames, bool includeValidAlerts = false);

        /// <summary>
        /// Exportuje dáta do CSV formátu
        /// </summary>
        Task<string> ExportToCsvAsync(bool includeHeaders = true, bool includeValidAlerts = false);

        /// <summary>
        /// Získa štatistiky exportovaných dát
        /// </summary>
        Task<ExportStatistics> GetExportStatisticsAsync(bool includeValidAlerts = false);

        /// <summary>
        /// Importuje dáta zo súboru - ✅ NOVÉ
        /// </summary>
        Task<ImportResult> ImportFromFileAsync(string filePath, ImportExportConfiguration? config = null);

        /// <summary>
        /// Exportuje dáta do súboru - ✅ NOVÉ
        /// </summary>
        Task<string> ExportToFileAsync(string filePath, ImportExportConfiguration? config = null);

        /// <summary>
        /// Získa import history - ✅ NOVÉ
        /// </summary>
        Dictionary<string, ImportResult> GetImportHistory();

        /// <summary>
        /// Získa export history - ✅ NOVÉ
        /// </summary>
        Dictionary<string, string> GetExportHistory();

        /// <summary>
        /// Vyčistí import/export history - ✅ NOVÉ
        /// </summary>
        void ClearHistory();
    }
}