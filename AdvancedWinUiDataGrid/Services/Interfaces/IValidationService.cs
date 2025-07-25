// Services/Interfaces/IValidationService.cs - ✅ INTERNAL
using Microsoft.UI.Xaml.Input;
using RpaWinUiComponents.AdvancedWinUiDataGrid.Models;
using System.Data;

namespace RpaWinUiComponents.AdvancedWinUiDataGrid.Services.Interfaces
{
    /// <summary>
    /// Interface pre validačné služby DataGrid - ✅ INTERNAL
    /// </summary>
    internal interface IValidationService  // ✅ CHANGED: public -> internal
    {
        Task InitializeAsync(GridConfiguration configuration);
        Task<List<string>> ValidateCellAsync(string columnName, object? value);
        Task<List<string>> ValidateRowAsync(Dictionary<string, object?> rowData);
        Task<bool> ValidateAllRowsAsync();
        Task AddValidationRuleAsync(ValidationRule rule);
        Task RemoveValidationRuleAsync(string columnName, ValidationType type);
        List<ValidationRule> GetValidationRules(string columnName);
        Task ClearAllValidationErrorsAsync();
    }

    /// <summary>
    /// Interface pre správu dát v DataGrid - ✅ INTERNAL
    /// </summary>
    internal interface IDataManagementService  // ✅ CHANGED: public -> internal
    {
        Task InitializeAsync(GridConfiguration configuration);
        Task LoadDataAsync(List<Dictionary<string, object?>> data);
        Task<List<Dictionary<string, object?>>> GetAllDataAsync();
        Task<Dictionary<string, object?>> GetRowDataAsync(int rowIndex);
        Task SetCellValueAsync(int rowIndex, string columnName, object? value);
        Task<object?> GetCellValueAsync(int rowIndex, string columnName);
        Task<int> AddRowAsync(Dictionary<string, object?>? initialData = null);
        Task DeleteRowAsync(int rowIndex);
        Task ClearAllDataAsync();
        Task CompactRowsAsync();
        Task<int> GetNonEmptyRowCountAsync();
        Task<bool> IsRowEmptyAsync(int rowIndex);
    }

    /// <summary>
    /// Interface pre Copy/Paste funkcionalitu - ✅ INTERNAL
    /// </summary>
    internal interface ICopyPasteService  // ✅ CHANGED: public -> internal
    {
        Task InitializeAsync();
        Task CopySelectedCellsAsync(List<CellSelection> selectedCells);
        Task PasteFromClipboardAsync(int startRowIndex, int startColumnIndex);
        Task CutSelectedCellsAsync(List<CellSelection> selectedCells);
        Task HandleKeyboardShortcutAsync(KeyRoutedEventArgs e);
        Task<bool> CanPasteAsync();
        Task<string> GetClipboardPreviewAsync();
    }

    /// <summary>
    /// Interface pre export služby DataGrid - ✅ INTERNAL
    /// </summary>
    internal interface IExportService  // ✅ CHANGED: public -> internal
    {
        Task InitializeAsync(GridConfiguration configuration);
        Task<DataTable> ExportToDataTableAsync();
        Task<DataTable> ExportValidRowsOnlyAsync();
        Task<DataTable> ExportInvalidRowsOnlyAsync();
        Task<DataTable> ExportSpecificColumnsAsync(string[] columnNames);
        Task<string> ExportToCsvAsync(bool includeHeaders = true);
        Task<ExportStatistics> GetExportStatisticsAsync();
    }

    /// <summary>
    /// Interface pre navigáciu v DataGrid - ✅ INTERNAL
    /// </summary>
    internal interface INavigationService  // ✅ CHANGED: public -> internal
    {
        Task InitializeAsync();
        Task HandleKeyDownAsync(object sender, KeyRoutedEventArgs e);
        Task MoveToNextCellAsync(int currentRow, int currentColumn);
        Task MoveToPreviousCellAsync(int currentRow, int currentColumn);
        Task MoveToCellBelowAsync(int currentRow, int currentColumn);
        Task MoveToCellAboveAsync(int currentRow, int currentColumn);
        Task CancelCellEditAsync(object sender);
        Task FinishCellEditAsync(object sender);
        Task InsertNewLineInCellAsync(object sender);
    }
}