// Services/Interfaces/IDataManagementService.cs - ✅ OPRAVENÝ - len IDataManagementService

using RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Models.Grid;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RpaWinUiComponentsPackage.AdvancedWinUiDataGrid.Services.Interfaces
{
    /// <summary>
    /// Interface pre správu dát v DataGrid - INTERNAL
    /// </summary>
    internal interface IDataManagementService
    {
        /// <summary>
        /// Inicializuje dátovú službu s konfiguráciou
        /// </summary>
        Task InitializeAsync(Models.Grid.GridConfiguration configuration);

        /// <summary>
        /// Načíta dáta do gridu
        /// </summary>
        Task LoadDataAsync(List<Dictionary<string, object?>> data);

        /// <summary>
        /// Získa všetky dáta z gridu
        /// </summary>
        Task<List<Dictionary<string, object?>>> GetAllDataAsync();

        /// <summary>
        /// Získa dáta špecifického riadku
        /// </summary>
        Task<Dictionary<string, object?>> GetRowDataAsync(int rowIndex);

        /// <summary>
        /// Nastaví hodnotu bunky
        /// </summary>
        Task SetCellValueAsync(int rowIndex, string columnName, object? value);

        /// <summary>
        /// Získa hodnotu bunky
        /// </summary>
        Task<object?> GetCellValueAsync(int rowIndex, string columnName);

        /// <summary>
        /// Pridá nový riadok
        /// </summary>
        Task<int> AddRowAsync(Dictionary<string, object?>? initialData = null);

        /// <summary>
        /// Zmaže riadok (vyčisti jeho obsah)
        /// </summary>
        Task DeleteRowAsync(int rowIndex);

        /// <summary>
        /// Vymaže všetky dáta
        /// </summary>
        Task ClearAllDataAsync();

        /// <summary>
        /// Kompaktuje riadky (odstráni prázdne medzery)
        /// </summary>
        Task CompactRowsAsync();

        /// <summary>
        /// Získa počet neprázdnych riadkov
        /// </summary>
        Task<int> GetNonEmptyRowCountAsync();

        /// <summary>
        /// Kontroluje či je riadok prázdny
        /// </summary>
        Task<bool> IsRowEmptyAsync(int rowIndex);
    }
}